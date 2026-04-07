using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Race")]
        public float finishDistance = 100f;
        public int countdownSeconds = 3;

        [Header("Spawns")]
        public Transform hostSpawn;
        public Transform clientSpawn;

        private readonly Dictionary<ulong, NetworkRacePlayer> _players = new();

        public NetworkVariable<bool> LobbyReady = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> CountdownActive = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> RaceStarted = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> RaceFinished = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> CountdownValue = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<ulong> WinnerClientId = new(
            999999,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null && IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;
            StartCoroutine(RegisterPlayerNextFrame(clientId));
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            if (_players.ContainsKey(clientId))
                _players.Remove(clientId);

            LobbyReady.Value = _players.Count >= 2;

            if (_players.Count < 2 && !RaceStarted.Value)
                CountdownActive.Value = false;
        }

        private IEnumerator RegisterPlayerNextFrame(ulong clientId)
        {
            yield return null;

            if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
                yield break;

            if (client.PlayerObject == null)
                yield break;

            NetworkRacePlayer racePlayer = client.PlayerObject.GetComponent<NetworkRacePlayer>();
            if (racePlayer == null)
                yield break;

            RegisterPlayer(racePlayer);

            if (_players.Count >= 2)
                LobbyReady.Value = true;
        }

        public void RegisterPlayer(NetworkRacePlayer player)
        {
            if (!IsServer) return;

            _players[player.OwnerClientId] = player;

            Transform spawn = player.OwnerClientId == NetworkManager.ServerClientId ? hostSpawn : clientSpawn;
            if (spawn != null)
                player.SetSpawnFromServer(spawn.position, spawn.rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartRaceServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            if (!LobbyReady.Value) return;
            if (CountdownActive.Value || RaceStarted.Value) return;

            ulong senderId = rpcParams.Receive.SenderClientId;

            // only host can start
            if (senderId != NetworkManager.ServerClientId)
                return;

            StartCoroutine(BeginRaceRoutine());
        }

        private IEnumerator BeginRaceRoutine()
        {
            CountdownActive.Value = true;
            RaceFinished.Value = false;
            WinnerClientId.Value = 999999;

            foreach (var kvp in _players)
            {
                kvp.Value.ServerResetForRace();
            }

            for (int i = countdownSeconds; i > 0; i--)
            {
                CountdownValue.Value = i;
                yield return new WaitForSeconds(1f);
            }

            CountdownValue.Value = 0;
            CountdownActive.Value = false;
            RaceStarted.Value = true;

            foreach (var kvp in _players)
            {
                kvp.Value.ServerGenerateNextPrompt();
            }
        }

        public void CheckForWinner(NetworkRacePlayer player)
        {
            if (!IsServer) return;
            if (!RaceStarted.Value || RaceFinished.Value) return;

            if (player.Progress.Value >= finishDistance)
            {
                RaceFinished.Value = true;
                RaceStarted.Value = false;
                CountdownActive.Value = false;
                WinnerClientId.Value = player.OwnerClientId;
            }
        }

        public int GetPlayerCount()
        {
            return _players.Count;
        }
    }
}
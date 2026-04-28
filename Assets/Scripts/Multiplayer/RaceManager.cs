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
        public int minPlayersToStart = 2;
        public int maxPlayers = 4;

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        private readonly Dictionary<ulong, NetworkRacePlayer> _players = new();
        private readonly List<ulong> _playerOrder = new();

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

            _playerOrder.Remove(clientId);

            LobbyReady.Value = _players.Count >= minPlayersToStart;

            if (_players.Count < minPlayersToStart && !RaceStarted.Value)
                CountdownActive.Value = false;

            ReassignSpawnPoints();
        }

        private IEnumerator RegisterPlayerNextFrame(ulong clientId)
        {
            yield return null;

            if (!_players.ContainsKey(clientId))
            {
                if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
                    yield break;

                if (client.PlayerObject == null)
                    yield break;

                NetworkRacePlayer racePlayer = client.PlayerObject.GetComponent<NetworkRacePlayer>();
                if (racePlayer == null)
                    yield break;

                RegisterPlayer(racePlayer);
            }

            LobbyReady.Value = _players.Count >= minPlayersToStart;
        }

        public void RegisterPlayer(NetworkRacePlayer player)
        {
            if (!IsServer) return;
            if (_players.ContainsKey(player.OwnerClientId)) return;

            if (_players.Count >= maxPlayers)
            {
                Debug.LogWarning("Tried to register player beyond maxPlayers.");
                return;
            }

            _players[player.OwnerClientId] = player;
            _playerOrder.Add(player.OwnerClientId);

            AssignSpawnPoint(player, _playerOrder.Count - 1);
        }

        private void ReassignSpawnPoints()
        {
            if (!IsServer) return;

            for (int i = 0; i < _playerOrder.Count; i++)
            {
                ulong clientId = _playerOrder[i];

                if (_players.TryGetValue(clientId, out var player))
                {
                    AssignSpawnPoint(player, i);
                }
            }
        }

        private void AssignSpawnPoint(NetworkRacePlayer player, int index)
        {
            if (!IsServer) return;
            if (spawnPoints == null || spawnPoints.Length == 0) return;
            if (index < 0 || index >= spawnPoints.Length) return;

            Transform spawn = spawnPoints[index];
            if (spawn != null)
                player.SetSpawnFromServer(spawn.position, spawn.rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartRaceServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            if (_players.Count < minPlayersToStart) return;
            if (CountdownActive.Value || RaceStarted.Value) return;

            ulong senderId = rpcParams.Receive.SenderClientId;

            if (senderId != NetworkManager.ServerClientId)
                return;

            StartCoroutine(BeginRaceRoutine());
        }

        private IEnumerator BeginRaceRoutine()
        {
            CountdownActive.Value = true;
            RaceFinished.Value = false;
            WinnerClientId.Value = 999999;

            for (int i = 0; i < _playerOrder.Count; i++)
            {
                ulong clientId = _playerOrder[i];
                if (_players.TryGetValue(clientId, out var player))
                {
                    AssignSpawnPoint(player, i);
                    player.ServerResetForRace();
                }
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

        public List<NetworkRacePlayer> GetPlayersInOrder()
        {
            List<NetworkRacePlayer> orderedPlayers = new();

            for (int i = 0; i < _playerOrder.Count; i++)
            {
                ulong clientId = _playerOrder[i];
                if (_players.TryGetValue(clientId, out var player))
                    orderedPlayers.Add(player);
            }

            return orderedPlayers;
        }
    }
}
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
        public float finishDistance = 50f;
        public float countdownSeconds = 3f;

        [Header("Spawns")]
        public Transform hostSpawn;
        public Transform clientSpawn;

        private readonly Dictionary<ulong, NetworkRacePlayer> _players = new();
        private bool _countdownStarted = false;

        public NetworkVariable<bool> RaceStarted = new(false);
        public NetworkVariable<bool> RaceFinished = new(false);
        public NetworkVariable<ulong> WinnerClientId = new(999999);

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null && IsServer)
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            StartCoroutine(RegisterPlayerNextFrame(clientId));
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

            if (NetworkManager.ConnectedClientsList.Count >= 1 && !_countdownStarted && !RaceStarted.Value)
            {
                _countdownStarted = true;
                StartCoroutine(BeginRaceRoutine());
            }
        }

        public IEnumerator BeginRaceRoutine()
        {
            yield return new WaitForSeconds(countdownSeconds);
            RaceStarted.Value = true;

            foreach (var kvp in _players)
                kvp.Value.ServerGenerateNextPrompt();
        }

        public void RegisterPlayer(NetworkRacePlayer player)
        {
            if (!IsServer) return;

            _players[player.OwnerClientId] = player;

            Transform spawn = player.OwnerClientId == 0 ? hostSpawn : clientSpawn;
            if (spawn != null)
                player.SetSpawnFromServer(spawn.position, spawn.rotation);
        }

        public void CheckForWinner(NetworkRacePlayer player)
        {
            if (!IsServer || RaceFinished.Value) return;

            if (player.Progress.Value >= finishDistance)
            {
                RaceFinished.Value = true;
                WinnerClientId.Value = player.OwnerClientId;
            }
        }
    }
}
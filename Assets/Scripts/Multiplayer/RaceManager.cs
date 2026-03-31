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
        public float countdownSeconds = 3f;

        [Header("Spawns")]
        public Transform hostSpawn;
        public Transform clientSpawn;

        private readonly Dictionary<ulong, NetworkRacePlayer> _players = new();

        public NetworkVariable<bool> RaceStarted = new(false);
        public NetworkVariable<bool> RaceFinished = new(false);
        public NetworkVariable<ulong> WinnerClientId = new(999999);

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += OnClientConnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null && IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            if (NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObj = client.PlayerObject;
                if (playerObj != null)
                {
                    var racePlayer = playerObj.GetComponent<NetworkRacePlayer>();
                    if (racePlayer != null)
                    {
                        RegisterPlayer(racePlayer);
                    }
                }
            }

            if (NetworkManager.ConnectedClientsList.Count == 2 && !RaceStarted.Value)
            {
                StartCoroutine(BeginRaceRoutine());
            }
        }

        private System.Collections.IEnumerator BeginRaceRoutine()
        {
            yield return new WaitForSeconds(countdownSeconds);
            RaceStarted.Value = true;

            foreach (var kvp in _players)
            {
                kvp.Value.ServerGenerateNextPrompt();
            }
        }

        public void RegisterPlayer(NetworkRacePlayer player)
        {
            _players[player.OwnerClientId] = player;

            Transform spawn = player.OwnerClientId == 0 ? hostSpawn : clientSpawn;
            if (spawn != null)
            {
                player.SetSpawnFromServer(spawn.position, spawn.rotation);
            }
        }

        public void CheckForWinner(NetworkRacePlayer player)
        {
            if (!IsServer || RaceFinished.Value)
                return;

            if (player.Progress.Value >= finishDistance)
            {
                RaceFinished.Value = true;
                WinnerClientId.Value = player.OwnerClientId;
            }
        }
    }
}
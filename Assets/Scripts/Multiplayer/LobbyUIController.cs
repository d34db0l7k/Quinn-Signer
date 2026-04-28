using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Multiplayer
{
    public class LobbyUIController : MonoBehaviour
    {
        [Header("UI")]
        public TMP_Text lobbyStatusText;
        public TMP_Text playerCountText;
        public TMP_Text joinCodeText;
        public TMP_Text playerListText;
        public Button startRaceButton;

        [Header("Optional")]
        public TMP_InputField playerNameInput;

        private RelayConnectionManager _relayManager;

        private void Start()
        {
            _relayManager = FindObjectOfType<RelayConnectionManager>(true);
        }

        private void Update()
        {
            if (RaceManager.Instance == null || NetworkManager.Singleton == null)
                return;

            bool isHost = NetworkManager.Singleton.IsHost;
            int playerCount = RaceManager.Instance.GetPlayerCount();

            if (startRaceButton != null)
            {
                startRaceButton.gameObject.SetActive(isHost);
                startRaceButton.interactable = isHost && playerCount >= 2;
            }

            if (playerCountText != null)
                playerCountText.text = $"Players: {playerCount}/4";

            if (joinCodeText != null)
            {
                if (_relayManager != null && !string.IsNullOrWhiteSpace(_relayManager.CurrentJoinCode))
                    joinCodeText.text = $"Join Code: {_relayManager.CurrentJoinCode}";
                else
                    joinCodeText.text = "Join Code: ----";
            }

            if (playerListText != null)
            {
                var orderedPlayers = RaceManager.Instance.GetPlayersInOrder();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < orderedPlayers.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {orderedPlayers[i].GetPlayerName()}");
                }

                playerListText.text = sb.ToString();
            }

            if (lobbyStatusText != null)
            {
                if (playerCount < 2)
                    lobbyStatusText.text = "Waiting for more players...";
                else if (isHost)
                    lobbyStatusText.text = "Ready to start. You can begin with 2 to 4 players.";
                else
                    lobbyStatusText.text = "Waiting for host to start.";
            }
        }

        public void OnStartRacePressed()
        {
            if (RaceManager.Instance == null) return;
            RaceManager.Instance.RequestStartRaceServerRpc();
        }

        public void OnApplyNamePressed()
        {
            string chosenName = playerNameInput != null ? playerNameInput.text : "Player";

            NetworkRacePlayer[] players = FindObjectsOfType<NetworkRacePlayer>();
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    player.SubmitNameServerRpc(chosenName);
                    break;
                }
            }
        }
    }
}
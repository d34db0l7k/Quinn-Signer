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

            if (startRaceButton != null)
            {
                startRaceButton.gameObject.SetActive(isHost);
                startRaceButton.interactable = isHost && RaceManager.Instance.LobbyReady.Value;
            }

            if (playerCountText != null)
                playerCountText.text = $"Players: {RaceManager.Instance.GetPlayerCount()}/2";

            if (joinCodeText != null)
            {
                if (_relayManager != null && !string.IsNullOrWhiteSpace(_relayManager.CurrentJoinCode))
                    joinCodeText.text = $"Join Code: {_relayManager.CurrentJoinCode}";
                else
                    joinCodeText.text = "Join Code: ----";
            }

            if (lobbyStatusText != null)
            {
                if (!RaceManager.Instance.LobbyReady.Value)
                {
                    lobbyStatusText.text = "Waiting for another player...";
                }
                else if (isHost)
                {
                    lobbyStatusText.text = "Both players joined. Press Start Race.";
                }
                else
                {
                    lobbyStatusText.text = "Both players joined. Waiting for host to start.";
                }
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
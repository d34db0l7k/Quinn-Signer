using UnityEngine;
using TMPro;
using Unity.Netcode;

namespace Multiplayer
{
    public class RaceUIController : MonoBehaviour
    {
        public TMP_InputField nameInput;
        public TMP_Text statusText;

        private void Update()
        {
            if (RaceManager.Instance == null || statusText == null)
                return;

            if (!RaceManager.Instance.RaceStarted.Value)
            {
                int playerCount = NetworkManager.Singleton != null
                    ? NetworkManager.Singleton.ConnectedClientsList.Count
                    : 0;

                statusText.text = playerCount < 2 ? "Waiting for second player..." : "Get Ready!";
                return;
            }

            if (!RaceManager.Instance.RaceFinished.Value)
            {
                statusText.text = "Race in progress!";
                return;
            }

            ulong localId = NetworkManager.Singleton.LocalClientId;
            statusText.text = RaceManager.Instance.WinnerClientId.Value == localId ? "You Win!" : "You Lose!";
        }

        public void ApplyLocalName()
        {
            string chosen = nameInput != null ? nameInput.text : "Player";

            foreach (var player in FindObjectsOfType<NetworkRacePlayer>())
            {
                if (player.IsOwner)
                {
                    player.SubmitNameServerRpc(chosen);
                    break;
                }
            }
        }
    }
}
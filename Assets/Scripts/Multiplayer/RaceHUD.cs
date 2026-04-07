using TMPro;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer
{
    public class RaceHUD : MonoBehaviour
    {
        public TMP_Text promptText;
        public TMP_Text statusText;

        public void SetPrompt(int prompt)
        {
            if (promptText == null) return;

            promptText.text = prompt switch
            {
                0 => "↑",
                1 => "↓",
                2 => "←",
                3 => "→",
                _ => "?"
            };
        }
        private void Update()
        {
            if (statusText == null || RaceManager.Instance == null || NetworkManager.Singleton == null)
                return;

            if (!RaceManager.Instance.RaceStarted.Value)
            {
                int count = NetworkManager.Singleton.ConnectedClientsList.Count;
                statusText.text = count < 2 ? "Waiting for players..." : "Starting...";
            }
            else if (!RaceManager.Instance.RaceFinished.Value)
            {
                statusText.text = "Race in progress";
            }
            else
            {
                ulong localId = NetworkManager.Singleton.LocalClientId;
                statusText.text = RaceManager.Instance.WinnerClientId.Value == localId ? "You Win!" : "You Lose!";
            }
        }
    }
}
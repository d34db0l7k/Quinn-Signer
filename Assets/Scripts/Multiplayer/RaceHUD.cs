using TMPro;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer
{
    public class RaceHUD : MonoBehaviour
    {
        public TMP_Text promptText;
        public TMP_Text statusText;
        public TMP_Text countdownText;

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
            if (RaceManager.Instance == null || NetworkManager.Singleton == null)
                return;

            if (RaceManager.Instance.CountdownActive.Value)
            {
                if (countdownText != null)
                    countdownText.text = RaceManager.Instance.CountdownValue.Value.ToString();

                if (statusText != null)
                    statusText.text = "Get Ready!";
            }
            else
            {
                if (countdownText != null)
                    countdownText.text = "";
            }

            if (statusText != null)
            {
                if (RaceManager.Instance.RaceFinished.Value)
                {
                    ulong localId = NetworkManager.Singleton.LocalClientId;
                    statusText.text = RaceManager.Instance.WinnerClientId.Value == localId ? "You Win!" : "You Lose!";
                }
                else if (RaceManager.Instance.RaceStarted.Value)
                {
                    statusText.text = "Race in progress";
                }
                else if (!RaceManager.Instance.CountdownActive.Value)
                {
                    statusText.text = "Waiting...";
                }
            }
        }
    }
}
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer
{
    public class RaceCanvasController : MonoBehaviour
    {
        [Header("Canvas References")]
        public GameObject connectionCanvas;
        public GameObject raceHudCanvas;

        private bool _hasSwitchedToRaceUI = false;

        private void Start()
        {
            if (connectionCanvas != null)
                connectionCanvas.SetActive(true);

            if (raceHudCanvas != null)
                raceHudCanvas.SetActive(false);
        }

        private void Update()
        {
            if (_hasSwitchedToRaceUI)
                return;

            if (NetworkManager.Singleton == null || RaceManager.Instance == null)
                return;

            // swap once the local client is connected and the race has started
            if (NetworkManager.Singleton.IsClient && RaceManager.Instance.RaceStarted.Value)
            {
                ShowRaceHUD();
            }
        }

        public void ShowRaceHUD()
        {
            _hasSwitchedToRaceUI = true;

            if (connectionCanvas != null)
                connectionCanvas.SetActive(false);

            if (raceHudCanvas != null)
                raceHudCanvas.SetActive(true);
        }

        public void ShowConnectionUI()
        {
            _hasSwitchedToRaceUI = false;

            if (connectionCanvas != null)
                connectionCanvas.SetActive(true);

            if (raceHudCanvas != null)
                raceHudCanvas.SetActive(false);
        }
    }
}
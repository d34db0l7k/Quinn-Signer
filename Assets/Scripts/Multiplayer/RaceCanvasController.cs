using UnityEngine;
using Unity.Netcode;

namespace Multiplayer
{
    public class RaceCanvasController : MonoBehaviour
    {
        public GameObject connectionCanvas;
        public GameObject lobbyCanvas;
        public GameObject raceHudCanvas;

        private void Start()
        {
            ShowConnection();
        }

        private void Update()
        {
            if (NetworkManager.Singleton == null || RaceManager.Instance == null)
                return;

            if (RaceManager.Instance.RaceStarted.Value || RaceManager.Instance.CountdownActive.Value || RaceManager.Instance.RaceFinished.Value)
            {
                ShowRaceHUD();
            }
            else if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                ShowLobby();
            }
            else
            {
                ShowConnection();
            }
        }

        public void ShowConnection()
        {
            if (connectionCanvas != null) connectionCanvas.SetActive(true);
            if (lobbyCanvas != null) lobbyCanvas.SetActive(false);
            if (raceHudCanvas != null) raceHudCanvas.SetActive(false);
        }

        public void ShowLobby()
        {
            if (connectionCanvas != null) connectionCanvas.SetActive(false);
            if (lobbyCanvas != null) lobbyCanvas.SetActive(true);
            if (raceHudCanvas != null) raceHudCanvas.SetActive(false);
        }

        public void ShowRaceHUD()
        {
            if (connectionCanvas != null) connectionCanvas.SetActive(false);
            if (lobbyCanvas != null) lobbyCanvas.SetActive(false);
            if (raceHudCanvas != null) raceHudCanvas.SetActive(true);
        }
    }
}
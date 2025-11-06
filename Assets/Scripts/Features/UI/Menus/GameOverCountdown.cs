using System.Collections;
using Core.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Features.UI.Menus
{
    public class GameOverCountdown : MonoBehaviour
    {
        [Header("UI References")]
        public Text countdownText;

        public Button continueButton;
        [Header("Settings")]
        [Tooltip("Seconds the player has to press Continue")]
        public int pressTimeLimit;
        public string nextSceneIdx;
        // Internal flag to track if the user has pressed the continue button.
        private bool _continueClicked = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {     
            // start the countdown coroutine
            StartCoroutine(CountdownCoroutine());
        }

        private IEnumerator CountdownCoroutine()
        {
            var timeLeft = pressTimeLimit;

            // loop until the counter reaches zero and then quit the game
            while (timeLeft > 0)
            {
                if (countdownText)
                {
                    // update the text with curr count
                    countdownText.text = timeLeft.ToString();
                }
                yield return new WaitForSeconds(1f);
                timeLeft--;
            }
            // show 0 on the screen:
            if (countdownText)
                countdownText.text = "0";

            // If the user has not clicked the button within the time limit, exit
            if (!_continueClicked)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
        public void OnContinueClicked()
        {
            _continueClicked = true;
            SceneSwitcher.Instance.SwitchSceneAfterDelay(0,0);
        }
    }
}

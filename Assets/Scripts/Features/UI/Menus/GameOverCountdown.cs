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
        private bool _continueClicked = false;

        private void Start()
        {     
            StartCoroutine(CountdownCoroutine());
        }

        private IEnumerator CountdownCoroutine()
        {
            var timeLeft = pressTimeLimit;

            while (timeLeft > 0)
            {
                if (countdownText)
                {
                    countdownText.text = timeLeft.ToString();
                }
                yield return new WaitForSeconds(1f);
                timeLeft--;
            }
            if (countdownText)
                countdownText.text = "0";

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

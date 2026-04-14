using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Features.Gameplay.Entities.Enemy;

namespace Features.Signing
{
    public class HintMode : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private InputField inputField;

        [Header("Panels Disabled in Hint Mode")]
        [SerializeField] private GameObject slrtkPanel;
        [SerializeField] private GameObject extraTextPanel;

        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;

        private bool _hintModeEnabled;
        private bool _hintActive;

        private EnemyController _currentEnemy;
        private string _currentWord;

        private void Awake()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            if (!_videoPlayer)
            {
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = true;
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            _renderTexture = new RenderTexture(1280, 720, 0);
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _renderTexture;

            if (rawImage)
            {
                rawImage.texture = _renderTexture;
            }

            if (hintPanel)
            {
                hintPanel.SetActive(false);
            }

            if (slrtkPanel)
            {
                slrtkPanel.SetActive(true);
            }

            if (extraTextPanel)
            {
                extraTextPanel.SetActive(true);
            }
        }

        public void EnableHintMode()
        {
            _hintModeEnabled = true;

            Debug.Log("Hint mode enabled (button)");

            if (slrtkPanel)
            {
                slrtkPanel.SetActive(false);
            }

            if (extraTextPanel)
            {
                extraTextPanel.SetActive(false);
            }
        }

        public bool IsHintModeEnabled()
        {
            return _hintModeEnabled;
        }

        public void TryActivateHintForEnemy(GameObject enemyObj)
        {
            if (!_hintModeEnabled || _hintActive) return;
            if (enemyObj == null) return;

            EnemyLabel label = enemyObj.GetComponentInChildren<EnemyLabel>(true);
            if (label == null) return;

            _currentEnemy = enemyObj.GetComponent<EnemyController>();
            _currentWord = label.targetWord;

            if (_currentEnemy == null || string.IsNullOrEmpty(_currentWord)) return;

            ShowHint();
        }

        private void ShowHint()
        {
            string url = VideoCatalog.GetVideoUrlForWord(_currentWord);
            if (string.IsNullOrEmpty(url)) return;

            _videoPlayer.url = url;
            _videoPlayer.Play();

            if (hintPanel)
            {
                hintPanel.SetActive(true);
            }

            if (slrtkPanel)
            {
                slrtkPanel.SetActive(false);
            }

            if (extraTextPanel)
            {
                extraTextPanel.SetActive(false);
            }

            _hintActive = true;

            if (inputField)
            {
                inputField.text = "";
                inputField.gameObject.SetActive(true);
                inputField.ActivateInputField();
            }
        }

        private void HideHint()
        {
            _videoPlayer.Stop();

            if (hintPanel)
            {
                hintPanel.SetActive(false);
            }

            if (slrtkPanel && !_hintModeEnabled)
            {
                slrtkPanel.SetActive(true);
            }

            if (extraTextPanel && !_hintModeEnabled)
            {
                extraTextPanel.SetActive(true);
            }

            _hintActive = false;
            _currentEnemy = null;
            _currentWord = null;

            if (inputField)
            {
                inputField.text = "";
            }
        }

        public void OnEnemyDestroyed()
        {
            if (_hintActive)
            {
                HideHint();
            }
        }

        public void SubmitTypedAnswer()
        {
            if (!_hintActive) return;

            string raw = inputField ? inputField.text : "";

            if (Normalize(raw) == Normalize(_currentWord))
            {
                if (_currentEnemy)
                {
                    _currentEnemy.Explode();
                }

                HideHint();
            }
        }

        private string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            return s.Trim().ToLower()
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "")
                .Replace("\u200B", "");
        }
    }
}
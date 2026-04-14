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

        [Header("SLRTK")]
        [SerializeField] private GameObject slrtkPanel;

        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private bool _hintActive;

        private EnemyController _currentEnemy;
        private string _currentWord;

        private void Awake()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            if (!_videoPlayer) _videoPlayer = gameObject.AddComponent<VideoPlayer>();

            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = true;
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            _renderTexture = new RenderTexture(1280, 720, 0);
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _renderTexture;

            if (rawImage) rawImage.texture = _renderTexture;
            if (hintPanel) hintPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (_hintActive) HideHint();
                else ShowHint();
            }
        }

        private void ShowHint()
        {
            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            if (enemyLabels == null || enemyLabels.Length == 0) return;

            EnemyLabel target = enemyLabels[0];
            if (target == null) return;

            _currentEnemy = target.GetComponentInParent<EnemyController>();
            _currentWord = target.targetWord;

            if (_currentEnemy == null || string.IsNullOrEmpty(_currentWord)) return;

            Debug.Log($"[HintMode] Selected word: [{_currentWord}]");

            string url = VideoCatalog.GetVideoUrlForWord(_currentWord);
            if (string.IsNullOrEmpty(url)) return;

            _videoPlayer.url = url;
            _videoPlayer.Play();

            if (hintPanel) hintPanel.SetActive(true);
            if (slrtkPanel) slrtkPanel.SetActive(false);

            _hintActive = true;

            if (inputField)
            {
                inputField.text = "";
                inputField.ActivateInputField();
                inputField.Select();
            }
        }

        private void HideHint()
        {
            _videoPlayer.Stop();

            if (hintPanel) hintPanel.SetActive(false);
            if (slrtkPanel) slrtkPanel.SetActive(true);

            _hintActive = false;

            _currentEnemy = null;
            _currentWord = null;

            if (inputField)
                inputField.text = "";
        }

        private void OnDestroy()
        {
            if (_videoPlayer)
            {
                _videoPlayer.targetTexture = null;
                Destroy(_videoPlayer);
            }

            if (_renderTexture)
                Destroy(_renderTexture);
        }

        public void OnEnemyDestroyed()
        {
            if (_hintActive)
                HideHint();
        }

        private string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            return s.Trim()
                    .ToLower()
                    .Replace(" ", "")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("\t", "")
                    .Replace("\u200B", "");
        }

        public void SubmitTypedAnswer()
        {
            if (!_hintActive) return;

            if (_currentEnemy == null || _currentEnemy.gameObject == null)
            {
                HideHint();
                return;
            }

            string rawInput = inputField != null ? inputField.text : "";

            string normalizedInput = Normalize(rawInput);
            string correct = Normalize(_currentWord);

            Debug.Log($"[HintMode] INPUT  = [{rawInput}]");
            Debug.Log($"[HintMode] TARGET = [{_currentWord}]");

            if (normalizedInput == correct)
            {
                Debug.Log("[HintMode] Correct typed answer!");

                _currentEnemy.Explode();
                HideHint();
            }
            else
            {
                Debug.Log("[HintMode] Incorrect typed answer.");
            }
        }
    }
}
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;

namespace Features.Signing
{
    public class HintModeTyping : MonoBehaviour
    {
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private InputField inputField;
        [SerializeField] private Button submitButton;

        [SerializeField] private GameObject slrtkPanel;
        [SerializeField] private PlayerHealth playerHealth;

        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;

        private bool _hintModeActive = false;
        private EnemyLabel _currentTarget;

        private Signer _signer;

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

            _signer = FindFirstObjectByType<Signer>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            if (submitButton)
                submitButton.onClick.AddListener(OnSubmit);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (_hintModeActive)
                    DisableHintMode();
                else
                    EnableHintMode();
            }
        }

        private void EnableHintMode()
        {
            SelectNextTarget();
            if (_currentTarget == null) return;

            PlayVideoForCurrent();

            if (hintPanel) hintPanel.SetActive(true);
            if (slrtkPanel) slrtkPanel.SetActive(false);

            _hintModeActive = true;
        }

        private void DisableHintMode()
        {
            _videoPlayer.Stop();

            if (hintPanel) hintPanel.SetActive(false);
            if (slrtkPanel) slrtkPanel.SetActive(true);

            _hintModeActive = false;
        }

        private void SelectNextTarget()
        {
            var enemies = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);

            if (enemies == null || enemies.Length == 0)
            {
                _currentTarget = null;
                return;
            }

            _currentTarget = enemies
                .Where(e => e != null && !string.IsNullOrEmpty(e.targetWord))
                .FirstOrDefault();
        }

        private void PlayVideoForCurrent()
        {
            if (_currentTarget == null) return;

            string url = VideoCatalog.GetVideoUrlForWord(_currentTarget.targetWord);
            if (string.IsNullOrEmpty(url)) return;

            _videoPlayer.url = url;
            _videoPlayer.Play();
        }

        public void OnSubmit()
        {
            if (!_hintModeActive) return;

            if (_currentTarget == null)
            {
                SelectNextTarget();
                return;
            }

            string typed = Normalize(inputField.text);
            string correct = Normalize(_currentTarget.targetWord);

            if (string.IsNullOrEmpty(typed)) return;

            if (typed == correct)
            {
                var controller = _currentTarget.GetComponentInParent<EnemyController>();

                if (controller)
                    controller.Explode();
                else
                    Destroy(_currentTarget.gameObject);

                Invoke(nameof(AfterEnemyDestroyed), 0.05f);
            }
            else
            {
                if (playerHealth)
                    playerHealth.Damage(1);
            }

            inputField.text = "";
        }

        private void AfterEnemyDestroyed()
        {
            SelectNextTarget();

            if (_currentTarget != null)
            {
                PlayVideoForCurrent();
            }
            else
            {
                DisableHintMode();
            }
        }

        private string Normalize(string s)
        {
            return (s ?? "").Trim().ToLowerInvariant();
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
    }
}
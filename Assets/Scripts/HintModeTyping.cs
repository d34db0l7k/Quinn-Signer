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
                if (GameModeState.HintTypingModeActive)
                    DisableHintMode();
                else
                    EnableHintMode();
            }
        }

        private void EnableHintMode()
        {
            Debug.Log("[HintModeTyping] ENABLE");

            GameModeState.HintTypingModeActive = true;

            SelectNextTarget();
            if (_currentTarget == null)
            {
                Debug.Log("[HintModeTyping] No enemies found");
                return;
            }

            PlayVideo();

            if (hintPanel) hintPanel.SetActive(true);
            if (slrtkPanel) slrtkPanel.SetActive(false);
        }

        private void DisableHintMode()
        {
            Debug.Log("[HintModeTyping] DISABLE");

            GameModeState.HintTypingModeActive = false;

            _videoPlayer.Stop();

            if (hintPanel) hintPanel.SetActive(false);
            if (slrtkPanel) slrtkPanel.SetActive(true);
        }

        private void SelectNextTarget()
        {
            var enemies = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);

            Debug.Log("[HintModeTyping] Enemy count: " + enemies.Length);

            _currentTarget = enemies
                .Where(e => e != null && !string.IsNullOrEmpty(e.targetWord))
                .FirstOrDefault();

            Debug.Log("[HintModeTyping] Target: " + (_currentTarget ? _currentTarget.targetWord : "NULL"));
        }

        private void PlayVideo()
        {
            if (_currentTarget == null) return;

            string url = VideoCatalog.GetVideoUrlForWord(_currentTarget.targetWord);
            if (string.IsNullOrEmpty(url)) return;

            _videoPlayer.url = url;
            _videoPlayer.Play();
        }

        public void OnSubmit()
        {
            if (!GameModeState.HintTypingModeActive) return;

            Debug.Log("[HintModeTyping] Submit pressed");

            if (_currentTarget == null)
            {
                SelectNextTarget();
                return;
            }

            string typed = Normalize(inputField.text);
            string correct = Normalize(_currentTarget.targetWord);

            Debug.Log("[HintModeTyping] typed=" + typed + " correct=" + correct);

            if (typed == correct)
            {
                Debug.Log("[HintModeTyping] CORRECT");

                var controller = _currentTarget.GetComponentInParent<EnemyController>();
                if (controller) controller.Explode();
            }
            else
            {
                Debug.Log("[HintModeTyping] WRONG");
                if (playerHealth) playerHealth.Damage(1);
            }

            inputField.text = "";

            Invoke(nameof(Advance), 0.05f);
        }

        private void Advance()
        {
            SelectNextTarget();

            if (_currentTarget != null)
            {
                PlayVideo();
            }
            else
            {
                Debug.Log("[HintModeTyping] ALL ENEMIES CLEARED");
                DisableHintMode();
            }
        }

        private string Normalize(string s)
        {
            return (s ?? "").Trim().ToLowerInvariant();
        }
    }
}
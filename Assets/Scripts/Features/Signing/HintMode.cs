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

        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private bool _hintActive = false;

        private void Awake()
        {
            _videoPlayer = gameObject.GetComponent<VideoPlayer>();
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
                if (_hintActive)
                    HideHint();
                else
                    ShowHint();
            }
        }

        private void ShowHint()
        {
            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            if (enemyLabels == null || enemyLabels.Length == 0) return;

            EnemyLabel target = enemyLabels[0];
            string word = target.targetWord;

            if (string.IsNullOrEmpty(word)) return;

            string url = VideoCatalog.GetVideoUrlForWord(word);
            if (string.IsNullOrEmpty(url)) return;

            _videoPlayer.url = url;
            _videoPlayer.Play();

            if (hintPanel) hintPanel.SetActive(true);
            _hintActive = true;
        }

        private void HideHint()
        {
            _videoPlayer.Stop();
            if (hintPanel) hintPanel.SetActive(false);
            _hintActive = false;
        }

        private void OnDestroy()
        {
            if (_videoPlayer) { _videoPlayer.targetTexture = null; Destroy(_videoPlayer); }
            if (_renderTexture) Destroy(_renderTexture);
        }
        public void OnEnemyDestroyed()
        {
            if (_hintActive)
                HideHint();
        }
    }

}
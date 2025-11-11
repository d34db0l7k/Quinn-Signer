namespace Features.Signing
{
// Assets/Scripts/Signing/UI/VideoTile.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoTile : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text label;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;

    [SerializeField] private Button videoSpeedButton;
    [SerializeField] private Text videoSpeedText;
    
    private VideoPlayer _player;
    private RenderTexture _renderTexture;
    private string _url;
    private bool _slowed = false;

    public void Setup(string word, string videoUrl, bool autoplay = false)
    {
        _url = videoUrl;
        if (label) label.text = word.ToUpperInvariant();

        // lazy-create components
        _player = gameObject.GetComponent<VideoPlayer>();
        if (!_player) _player = gameObject.AddComponent<VideoPlayer>();
        
        _player.playOnAwake = false;
        _player.isLooping = true;
        _player.source = VideoSource.Url;
        _player.url = _url;
        _player.audioOutputMode = VideoAudioOutputMode.AudioSource; // change to None if you want silence

        // Create a runtime RenderTexture so no asset needed
        if (_renderTexture == null)
        {
            _renderTexture = new RenderTexture(1280, 720, 0); // adjust as you like
            _renderTexture.name = $"RT_{word}";
        }
        _player.renderMode = VideoRenderMode.RenderTexture;
        _player.targetTexture = _renderTexture;
        if (rawImage) rawImage.texture = _renderTexture;

        // Wire buttons
        if (playButton)  playButton.onClick.AddListener(Play);
        if (pauseButton) pauseButton.onClick.AddListener(Pause);
        if (videoSpeedButton) videoSpeedButton.onClick.AddListener(ToggleSlowdown);

        if (autoplay) Play();
    }

    private void Play()
    {
        if (_player == null) return;
        if (!string.IsNullOrEmpty(_url))
        {
            // Ensure texture is bound then Play
            _player.Play();
        }
    }

    private void Pause()
    {
        if (_player) _player.Pause();
    }

    private void ToggleSlowdown()
    {
        _slowed = !_slowed;
        var speed = _slowed ? 0.5f : 1f;
        if (_player) _player.playbackSpeed = speed;
        UpdateVideoSpeedUI();
    }


    private void UpdateVideoSpeedUI()
    {
        if (videoSpeedText)
            videoSpeedText.text = _slowed ? "0.5x" : "1x";
    }

    private void OnDisable()
    {
        // Stop decoding when scrolled off or scene changes
        if (_player) _player.Pause();
    }

    private void OnDestroy()
    {
        if (_player) { _player.targetTexture = null; Destroy(_player); }
        if (_renderTexture) Destroy(_renderTexture);
    }
}

}
namespace Features.Signing
{
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoTile : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text label;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;

    [SerializeField] private Button videoSpeedButton;
    [SerializeField] private Text videoSpeedText;

    [Header("Selection (Optional)")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image selectedHighlight;

    private VideoPlayer _player;
    private RenderTexture _renderTexture;
    private string _url;
    private bool _slowed = false;

    // NEW: expose the word for external code
    public string Word { get; private set; } = "";

    // NEW: selection state
    public bool IsSelected { get; private set; } = false;
    public System.Action<VideoTile, bool> onSelectionChanged; // (tile, isSelected)

    public void Setup(string word, string videoUrl, bool autoplay = false)
    {
        Word = (word ?? "").Trim().ToLowerInvariant();
        _url = videoUrl;
        if (label) label.text = Word.ToUpperInvariant();

        _player = gameObject.GetComponent<VideoPlayer>();
        if (!_player) _player = gameObject.AddComponent<VideoPlayer>();

        _player.playOnAwake = false;
        _player.isLooping = true;
        _player.source = VideoSource.Url;
        _player.url = _url;
        _player.audioOutputMode = VideoAudioOutputMode.AudioSource;

        if (_renderTexture == null)
        {
            _renderTexture = new RenderTexture(1280, 720, 0);
            _renderTexture.name = $"RT_{Word}";
        }
        _player.renderMode = VideoRenderMode.RenderTexture;
        _player.targetTexture = _renderTexture;
        if (rawImage) rawImage.texture = _renderTexture;

        if (playButton)  playButton.onClick.AddListener(Play);
        if (pauseButton) pauseButton.onClick.AddListener(Pause);
        if (videoSpeedButton) videoSpeedButton.onClick.AddListener(ToggleSlowdown);

        // NEW: optional selection wiring
        if (selectButton) selectButton.onClick.AddListener(ToggleSelected);
        RefreshSelectionVisual();

        if (autoplay) Play();
    }

    private void Play()  { if (_player != null && !string.IsNullOrEmpty(_url)) _player.Play(); }
    private void Pause() { if (_player) _player.Pause(); }

    private void ToggleSlowdown()
    {
        _slowed = !_slowed;
        var speed = _slowed ? 0.5f : 1f;
        if (_player) _player.playbackSpeed = speed;
        if (videoSpeedText) videoSpeedText.text = _slowed ? "0.5x" : "1x";
    }

    // NEW:
    private void ToggleSelected()
    {
        IsSelected = !IsSelected;
        RefreshSelectionVisual();
        onSelectionChanged?.Invoke(this, IsSelected);
    }
    private void RefreshSelectionVisual()
    {
        if (selectedHighlight) selectedHighlight.enabled = IsSelected;
    }

    private void OnDisable()
    {
        if (_player) _player.Pause();
    }

    private void OnDestroy()
    {
        if (_player) { _player.targetTexture = null; Destroy(_player); }
        if (_renderTexture) Destroy(_renderTexture);
    }
}
}

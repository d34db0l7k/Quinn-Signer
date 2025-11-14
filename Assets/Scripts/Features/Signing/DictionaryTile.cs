using UnityEngine;
using UnityEngine.UI;

namespace Features.Signing
{
    /// Wraps an existing VideoTile to add "select" behavior for the Dictionary scene
    public class DictionaryTile : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private VideoTile videoTile;     // assign the existing VideoTile on this prefab
        [SerializeField] private Button selectButton;     // a button covering the tile (or a dedicated "Select" btn)
        [SerializeField] private Image selectedHighlight; // optional outline/glow image

        public string Word { get; private set; } = "";
        public bool IsSelected { get; private set; } = false;

        // Raised when selection toggles; (this, IsSelected)
        public System.Action<DictionaryTile, bool> onSelectionChanged;

        public void Setup(string word, string videoUrl, bool autoplay = false)
        {
            Word = (word ?? "").Trim().ToLowerInvariant();
            if (!videoTile) videoTile = GetComponent<VideoTile>();
            if (videoTile) videoTile.Setup(Word, videoUrl, autoplay);

            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(ToggleSelected);
            }
            RefreshVisual();
        }

        void ToggleSelected()
        {
            IsSelected = !IsSelected;
            RefreshVisual();
            onSelectionChanged?.Invoke(this, IsSelected);
        }

        void RefreshVisual()
        {
            if (selectedHighlight) selectedHighlight.enabled = IsSelected;
        }
    }
}
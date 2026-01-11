using UnityEngine;
using UnityEngine.UI;

namespace Features.Signing
{
    public class DictionaryTile : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private VideoTile videoTile;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image selectedHighlight;

        public string Word { get; private set; } = "";
        public bool IsSelected { get; private set; } = false;
        
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
using UnityEngine;
using UnityEngine.UI;

namespace Features.Signing
{
    public class DictionaryWordItem : MonoBehaviour
    {
        [SerializeField] private Text wordLabel;
        [SerializeField] private Toggle toggle;

        public string Word { get; private set; } = "";
        public bool IsOn => toggle && toggle.isOn;

        public System.Action<DictionaryWordItem, bool> onToggled;

        public void Setup(string word, bool isOn = false)
        {
            Word = (word ?? "").Trim().ToLowerInvariant();
            if (wordLabel) wordLabel.text = Word.ToUpperInvariant();

            if (toggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = isOn;
                toggle.onValueChanged.AddListener(v => onToggled?.Invoke(this, v));
            }
        }
    }
}
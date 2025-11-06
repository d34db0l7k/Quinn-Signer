using UnityEngine;
using UnityEngine.UI;

namespace Features.Gameplay.Entities.Enemy
{
    public class EnemyLabel : MonoBehaviour
    {
        public Text label;
        [HideInInspector] public string targetWord;

        [SerializeField] private string defaultText = "Word to Sign";
        private bool _explicitlySet;

        private void Start()
        {
            // Only set default if nothing explicit yet
            if (!_explicitlySet && label) label.text = defaultText;
        }

        public void SetWord(string word)
        {
            targetWord = word;
            _explicitlySet = true;
            if (label) label.text = word;
        }
    }
}

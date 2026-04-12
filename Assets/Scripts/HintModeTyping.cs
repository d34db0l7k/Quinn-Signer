using UnityEngine;
using UnityEngine.UI;
using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;

namespace Features.Signing
{
    public class HintModeTyping : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private InputField typingInput;
        [SerializeField] private Button submitButton;

        [Header("Hint Panel")]
        [SerializeField] private GameObject hintPanel;

        [Header("Player")]
        [SerializeField] private PlayerHealth playerHealth;

        private Signer _signer;

        private void OnEnable()
        {
            _signer = FindFirstObjectByType<Signer>();
            if (submitButton) submitButton.onClick.AddListener(OnSubmit);
            if (typingInput) typingInput.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnDisable()
        {
            if (submitButton) submitButton.onClick.RemoveListener(OnSubmit);
            if (typingInput) typingInput.onEndEdit.RemoveListener(OnEndEdit);
        }

        private void OnEndEdit(string value)
        {
            if (!hintPanel || !hintPanel.activeSelf) return;
            if (Input.GetKeyDown(KeyCode.Return))
                OnSubmit();
        }

        private void OnSubmit()
        {
            if (!hintPanel || !hintPanel.activeSelf) return;
            if (typingInput == null) return;

            string typed = typingInput.text.Trim().ToLowerInvariant();
            typingInput.text = "";

            if (string.IsNullOrEmpty(typed)) return;

            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            if (enemyLabels == null || enemyLabels.Length == 0) return;

            string currentEnemyWord = enemyLabels[0].targetWord?.Trim().ToLowerInvariant();

            if (string.Equals(typed, currentEnemyWord, System.StringComparison.OrdinalIgnoreCase))
            {
                if (_signer == null) _signer = FindFirstObjectByType<Signer>();
                if (_signer) _signer.SimulateSign(typed);
            }
            else
            {
                if (playerHealth) playerHealth.Damage(1);
            }
        }

        private void OnDestroy()
        {
            if (submitButton) submitButton.onClick.RemoveListener(OnSubmit);
        }
    }
}
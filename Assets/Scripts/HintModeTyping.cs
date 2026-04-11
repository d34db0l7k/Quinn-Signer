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

        [Header("Player")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Player Movement")]
        [SerializeField] private InfinitePlayerMovement playerMovement;

        private bool _wasFocused = false;

        private void Start()
        {
            if (submitButton) submitButton.onClick.AddListener(OnSubmit);
            if (typingInput) typingInput.onEndEdit.AddListener(OnEndEdit);
        }

        private void Update()
        {
            if (typingInput == null) return;

            bool isFocused = typingInput.isFocused;

            if (isFocused && !_wasFocused)
                if (playerMovement) playerMovement.enabled = false;

            if (!isFocused && _wasFocused)
                if (playerMovement) playerMovement.enabled = true;

            _wasFocused = isFocused;
        }

        private void OnEndEdit(string value)
        {
            if (Input.GetKeyDown(KeyCode.Return))
                OnSubmit();
        }

        private void OnSubmit()
        {
            if (typingInput == null) return;

            string typed = typingInput.text.Trim().ToLowerInvariant();
            typingInput.text = "";

            if (string.IsNullOrEmpty(typed)) return;

            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            EnemyLabel match = null;

            foreach (var label in enemyLabels)
            {
                if (!label) continue;
                if (string.Equals(label.targetWord, typed, System.StringComparison.OrdinalIgnoreCase))
                {
                    match = label;
                    break;
                }
            }

            if (match)
            {
                var controller = match.GetComponentInParent<EnemyController>() ?? match.GetComponent<EnemyController>();
                if (controller) controller.Explode(); else Destroy(match.gameObject);
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
namespace Features.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using Features.Gameplay.Entities.Player;

    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Image fill; // Image.type = Filled (Horizontal)

        void Reset()
        {
            if (!health) health = FindAnyObjectByType<PlayerHealth>();
            if (!fill)   fill   = GetComponentInChildren<Image>();
        }

        void OnEnable()
        {
            if (!health) health = FindAnyObjectByType<PlayerHealth>();
            if (health != null) health.OnHealthChanged += HandleChanged;
            // init
            if (health != null) HandleChanged(health.Current, health.maxHealth);
        }

        void OnDisable()
        {
            if (health != null) health.OnHealthChanged -= HandleChanged;
        }

        void HandleChanged(int current, int max)
        {
            if (!fill) return;
            fill.fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        }
    }

}
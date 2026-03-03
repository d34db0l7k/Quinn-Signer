namespace Features.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using Features.Gameplay.Entities.Player;
    using System.Collections;
    using UnityEngine.Rendering;

    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Image fill; // Image.type = Filled (Horizontal)
        [SerializeField] private Color32 origFillColor = new Color32(29, 140, 1, 255);
        [SerializeField] private float hpDrainSpeed = 3.0f;

        private Coroutine hpDrainCoroutine;

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
            //fill.fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            float end = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            if (hpDrainCoroutine != null) StopCoroutine(hpDrainCoroutine);
            hpDrainCoroutine = StartCoroutine(HealthDrainAnimation(end));
        }

        IEnumerator HealthDrainAnimation(float end)
        {
            fill.color = Color.red;
            while (fill.fillAmount - end > 0.001f)
            {
                fill.fillAmount = Mathf.Lerp(fill.fillAmount, end, Time.deltaTime * hpDrainSpeed);
                yield return null;
            }
            fill.color = origFillColor;
            fill.fillAmount = end;
        }
    }

}
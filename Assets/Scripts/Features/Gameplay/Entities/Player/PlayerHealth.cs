namespace Features.Gameplay.Entities.Player
{
    using System;
    using UnityEngine;

    public class PlayerHealth : MonoBehaviour
    {
        [Range(1, 10)] public int maxHealth = 10;
        public int Current { get; private set; }

        public event Action<int,int> OnHealthChanged; // (current, max)
        public event Action OnDeath;

        void Awake()
        {
            Current = Mathf.Clamp(maxHealth, 1, 10);
            OnHealthChanged?.Invoke(Current, maxHealth);
        }

        public void Damage(int amount = 1, bool showToast = true)
        {
            if (Current <= 0) return;
            Current = Mathf.Max(0, Current - Mathf.Max(1, amount));
            OnHealthChanged?.Invoke(Current, maxHealth);
            //Debug.Log("Calling Toast");
            if (showToast) Toast.Instance.ShowToast($"Took {amount} damage from enemy!", 1.5f, new Vector2(0f, 0f), new Vector2((Screen.width * 1.5f), 0f));
            if (Current <= 0) OnDeath?.Invoke();
        }

        public void Heal(int amount = 1)
        {
            if (Current <= 0) return;
            Current = Mathf.Min(maxHealth, Current + Mathf.Max(1, amount));
            OnHealthChanged?.Invoke(Current, maxHealth);
        }

        public void ResetHealth(int to = -1)
        {
            Current = (to > 0 ? to : maxHealth);
            OnHealthChanged?.Invoke(Current, maxHealth);
        }
    }

}
namespace Features.UI
{
    using System;
    using UnityEngine;

    public static class CrystalWallet
    {
        private const string Key = "QS_CrystalCount";

        public static event Action<int> OnChanged;

        public static int Load() => PlayerPrefs.GetInt(Key, 0);

        public static void Save(int value)
        {
            var v = Mathf.Max(0, value);
            PlayerPrefs.SetInt(Key, v);
            PlayerPrefs.Save();
            OnChanged?.Invoke(v);
        }

        public static void Add(int delta)
        {
            if (delta <= 0) return;
            Save(Load() + delta);
        }

        public static bool CanAfford(int cost) => Load() >= Mathf.Max(0, cost);

        public static bool Spend(int cost)
        {
            cost = Mathf.Max(0, cost);
            var curr = Load();
            if (curr < cost) return false;
            Save(curr - cost);
            return true;
        }

        public static void ResetTo(int value = 0) => Save(value);
    }
}
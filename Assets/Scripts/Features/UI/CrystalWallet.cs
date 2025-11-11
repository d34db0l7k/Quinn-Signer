namespace Features.UI
{
    using UnityEngine;

    public static class CrystalWallet
    {
        private const string Key = "QS_CrystalCount";

        public static int Load() => PlayerPrefs.GetInt(Key, 0);

        public static void Save(int value)
        {
            PlayerPrefs.SetInt(Key, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }

        public static void Add(int delta)
        {
            var curr = Load();
            Save(curr + Mathf.Max(0, delta));
        }

        public static void ResetTo(int value = 0) => Save(value);
    }

}
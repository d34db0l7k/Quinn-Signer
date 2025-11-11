namespace Features.Signing
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class SessionWordPicker
    {
        static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();

        public static List<string> PickWordsWithVideos(
            Features.Signing.WordBank bank,
            int count,
            string videosSubfolder = "Reference Videos")
        {
            var videoSet = VideoCatalog.IndexWordsWithVideos(videosSubfolder); // filenames -> set
            var bankWords = bank.GetWordList().Select(Norm);                   // from your txt

            // Only words that have videos
            var pool = bankWords.Where(w => videoSet.Contains(w)).Distinct().ToList();

            // Shuffle and take up to count
            Shuffle(pool);
            if (pool.Count < count)
                Debug.LogWarning($"[SessionWordPicker] Only {pool.Count} words have videos; requested {count}.");

            return pool.Take(Mathf.Min(count, pool.Count)).ToList();
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

}
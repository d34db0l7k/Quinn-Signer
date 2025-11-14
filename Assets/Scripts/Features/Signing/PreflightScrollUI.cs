namespace Features.Signing
{
// Features/Signing/PreflightScrollUI.cs  (UNIFIED)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Features.Signing
{
    public class PreflightScrollUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WordBank wordBank;
        [SerializeField] private SessionSelection sessionSelection; // asset
        [SerializeField] private int wordsNeeded = 5;
        [SerializeField] private string videosSubfolder = "Reference Videos";
        [SerializeField] private string mainSceneName = "GlyphwayScene";
        [Tooltip("If true, clears any leftover words in SessionSelection at play.")]
        [SerializeField] private bool forceRepickOnPlay = true;

        [Header("UI")]
        [SerializeField] private Transform contentParent;   // ScrollView/Viewport/Content
        [SerializeField] private VideoTile videoTilePrefab; // Panel-root prefab

        void OnValidate()
        {
            if (!contentParent) Debug.LogWarning("[Preflight] contentParent not assigned.");
            if (!videoTilePrefab) Debug.LogWarning("[Preflight] videoTilePrefab not assigned.");
        }

        void Start()
        {
            // STRICT: if words exist in SessionSelection, use them exactly
            if (sessionSelection && sessionSelection.HasWords)
            {
                var words = new System.Collections.Generic.List<string>();
                foreach (var w in sessionSelection.Words)
                {
                    var k = (w ?? "").Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(k)) words.Add(k);
                }

                Debug.Log($"[Preflight] Using session words ({words.Count}): [{string.Join(",", words)}]");

                if (words.Count == 0)
                {
                    Debug.LogError("[Preflight] SessionSelection has no usable words (after normalization).");
                    return;
                }

                BuildList(words); // your existing method signature
                return;
            }

            // If no session words exist, fall back to your existing picker:
            var wordsFallback = PickWordsWithVideos(wordsNeeded); // helper we added earlier in this class
            if (sessionSelection) { sessionSelection.SetWords(wordsFallback); sessionSelection.ResetRuntimeBag(); }

            Debug.Log($"[Preflight] Fallback words ({wordsFallback.Count}): [{string.Join(",", wordsFallback)}]");
            BuildList(wordsFallback);
        }
        
        private List<string> PickWordsWithVideos(int count)
        {
            // Build the set of words that actually have videos on disk
            HashSet<string> videoSet = null;

            // Prefer IndexWordsWithVideos if your VideoCatalog has it; otherwise fallback to ListAllVideoWords
            try
            {
                videoSet = VideoCatalog.IndexWordsWithVideos(videosSubfolder);
            }
            catch
            {
                var all = VideoCatalog.ListAllVideoWords(videosSubfolder);
                videoSet = new HashSet<string>(all);
            }

            // Pool = WordBank ∩ video-backed words (lowercased & distinct)
            var pool = new List<string>();
            if (wordBank != null)
            {
                foreach (var w in wordBank.GetWordList())
                {
                    var k = (w ?? "").Trim().ToLowerInvariant();
                    if (k.Length == 0) continue;
                    if (videoSet.Contains(k) && !pool.Contains(k)) pool.Add(k);
                }
            }
            else
            {
                // If no WordBank, use the video set directly
                pool.AddRange(videoSet);
            }

            // Shuffle and take up to 'count'
            Shuffle(pool);
            if (pool.Count > count) pool.RemoveRange(count, pool.Count - count);

            Debug.Log($"[PreflightScrollUI] PickWordsWithVideos -> {pool.Count} (requested {count})");
            return pool;
        }

        private void Shuffle<T>(IList<T> list)
        {
            var rng = new System.Random(unchecked(System.Environment.TickCount ^ GetHashCode()));
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        
        // Inlined picker (formerly SessionWordPicker)
        static List<string> PickWordsWithVideos_Random(WordBank bank, int count, string subfolder, List<string> allVideoWords)
        {
            var rng = new System.Random(unchecked(
                Environment.TickCount ^ Guid.NewGuid().GetHashCode() ^ (int)DateTime.Now.Ticks));

            if (!bank)
            {
                // Fallback to any video filenames (still random) so UI shows something
                var fallback = new List<string>(allVideoWords);
                Shuffle(fallback, rng);
                return fallback.Take(Mathf.Min(count, fallback.Count)).ToList();
            }

            var videoSet = new HashSet<string>(VideoCatalog.IndexWordsWithVideos(subfolder));
            var pool = bank.GetWordList()
                           .Select(w => (w ?? "").Trim().ToLowerInvariant())
                           .Where(w => videoSet.Contains(w))
                           .Distinct()
                           .ToList();

            Shuffle(pool, rng);
            if (pool.Count < count)
                Debug.LogWarning($"[Preflight] Only {pool.Count} words have videos; requested {count}.");

            return pool.Take(Mathf.Min(count, pool.Count)).ToList();
        }

        static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        void BuildList(List<string> list)
        {
            if (!contentParent || !videoTilePrefab)
            {
                Debug.LogError("[Preflight] Missing contentParent or videoTilePrefab.");
                return;
            }

            foreach (Transform child in contentParent) Destroy(child.gameObject);

            int made = 0;
            foreach (var w in list)
            {
                var url = VideoCatalog.GetVideoUrlForWord(w, videosSubfolder);
                if (string.IsNullOrEmpty(url))
                {
                    Debug.LogWarning($"[Preflight] No URL for '{w}' (filename mismatch?).");
                    continue;
                }

                var tile = Instantiate(videoTilePrefab, contentParent, false);
                // normalize rect (protect against zero scale)
                var rt = tile.GetComponent<RectTransform>();
                if (rt)
                {
                    rt.localScale = Vector3.one;
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot     = new Vector2(0.5f, 1f);
                    rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
                    rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
                }

                tile.Setup(w, url, autoplay: false);
                made++;
            }

            var contentRT = contentParent as RectTransform;
            if (contentRT) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

            Debug.Log($"[Preflight] Built {made} tiles.");
        }

        public void ContinueToMain() => SceneManager.LoadScene(mainSceneName);
    }
}

}
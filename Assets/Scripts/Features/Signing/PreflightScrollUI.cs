namespace Features.Signing
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class PreflightScrollUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WordBank wordBank;  // ok if null (we can fall back)
        [SerializeField] private SessionSelection sessionSelection;    // asset; preferred source
        [SerializeField] private int wordsNeeded = 5;
        [SerializeField] private string videosSubfolder = "Reference Videos";
        [SerializeField] private string mainSceneName = "GlyphwayScene";
        [SerializeField] private bool forceRepickOnPlay = true;

        [Header("UI")]
        [SerializeField] Transform contentParent;   // MUST be ScrollView/Viewport/Content
        [SerializeField] VideoTile videoTilePrefab; // MUST be assigned

        private List<string> _words = new();

        void OnValidate()
        {
            if (!contentParent) Debug.LogWarning("[PreflightScrollUI] contentParent is not assigned (ScrollView/Viewport/Content).");
            if (!videoTilePrefab) Debug.LogWarning("[PreflightScrollUI] videoTilePrefab is not assigned.");
        }

        void Start()
        {
            Debug.Log($"[PreflightScrollUI] streamingAssetsPath = {Application.streamingAssetsPath}");
            var allVideoWords = VideoCatalog.ListAllVideoWords(videosSubfolder);
            Debug.Log($"[PreflightScrollUI] Found {allVideoWords.Count} video files in '{videosSubfolder}'.");
            if (forceRepickOnPlay)
            {
                if (sessionSelection) sessionSelection.SetWords(System.Array.Empty<string>());
            }
            // Preferred: session words (picked in the prior step)
            if (!forceRepickOnPlay && sessionSelection && sessionSelection.HasWords)
            {
                _words = new List<string>(sessionSelection.Words);
                Debug.Log($"[PreflightScrollUI] Using SessionSelection words: {_words.Count}");
            }
            else
            {
                // Secondary: pick N words that also exist in videos (requires a bank)
                if (wordBank)
                {
                    _words = SessionWordPicker.PickWordsWithVideos(wordBank, wordsNeeded, videosSubfolder);
                    Debug.Log($"[PreflightScrollUI] Picked {_words.Count} words with videos (requested {wordsNeeded}).");
                    if (sessionSelection) sessionSelection.SetWords(_words);
                }

                // Ultimate fallback: if still empty, just show *all* video words (so you SEE something)
                if (_words.Count == 0 && allVideoWords.Count > 0)
                {
                    _words = (allVideoWords.Count > wordsNeeded)
                        ? allVideoWords.GetRange(0, wordsNeeded)
                        : new List<string>(allVideoWords);
                    Debug.Log($"[PreflightScrollUI] Falling back to video filenames only: showing {_words.Count}.");
                }
            }

            if (_words.Count == 0)
            {
                Debug.LogError("[PreflightScrollUI] No words to show. Likely causes:\n" +
                               " - 'Reference Videos' folder name mismatch or empty\n" +
                               " - SessionSelection has no words and WordBank not assigned\n" +
                               " - Word names don’t match video filenames");
            }
            BuildList(_words);
        }

        void BuildList(List<string> list)
        {
            if (!contentParent || !videoTilePrefab)
            {
                Debug.LogError("[PreflightScrollUI] Missing contentParent or videoTilePrefab. Cannot build UI.");
                return;
            }

            foreach (Transform child in contentParent) Destroy(child.gameObject);

            int made = 0;
            foreach (var w in list)
            {
                var url = VideoCatalog.GetVideoUrlForWord(w, videosSubfolder);
                if (string.IsNullOrEmpty(url))
                {
                    Debug.LogWarning($"[PreflightScrollUI] No URL for '{w}' (check filename). Skipping.");
                    continue;
                }

                var tile = Instantiate(videoTilePrefab, contentParent);
                tile.Setup(w, url, autoplay: false);
                Debug.Log($"[PreflightScrollUI] Spawned tile: {w} → {url}");
                made++;
            }

            Debug.Log($"[PreflightScrollUI] Built {made} tiles under Content.");
        }

        public void ContinueToMain() => SceneManager.LoadScene(mainSceneName);
    }
}
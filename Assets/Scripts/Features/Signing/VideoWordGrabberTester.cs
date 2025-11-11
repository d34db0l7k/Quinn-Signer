namespace Features.Signing
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class VideoWordGrabberTester : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private Features.Signing.WordBank wordBank;
        [SerializeField] private int wordsNeeded = 6;
        [SerializeField] private string videosSubfolder = "Reference Videos";

        [Header("Diagnostics")]
        [SerializeField] private int showMissingSamples = 20;

        [ContextMenu("Test Grab Now")]
        public void TestGrabNow()
        {
            if (!wordBank)
            {
                Debug.LogError("[Tester] WordBank is not assigned.");
                return;
            }

            // 1) Pick N words guaranteed to have videos
            var picked = SessionWordPicker.PickWordsWithVideos(wordBank, wordsNeeded, videosSubfolder);

            Debug.Log($"[Tester] PICKED {picked.Count} / requested {wordsNeeded} words with videos:");
            foreach (var w in picked)
            {
                var path = VideoCatalog.GetVideoPathForWord(w, videosSubfolder);
                Debug.Log($"  • {w.ToUpperInvariant()}  →  {path}");
            }

            // 2) Optional: show a few words that DON'T have videos (for cleanup)
            var videoSet = VideoCatalog.IndexWordsWithVideos(videosSubfolder);
            var allBank = wordBank.GetWordList().Select(s => s.Trim().ToLowerInvariant());
            var missing = allBank.Where(w => !videoSet.Contains(w)).Distinct().Take(showMissingSamples).ToList();

            if (missing.Count == 0)
            {
                Debug.Log("[Tester] Nice! Every WordBank entry has a video.");
            }
            else
            {
                Debug.Log($"[Tester] Examples of words missing videos (showing up to {showMissingSamples}):");
                foreach (var m in missing) Debug.Log($"  – {m}");
            }
        }

        void Start()
        {
            // Auto-run once on Play for convenience
            TestGrabNow();
        }
    }

}
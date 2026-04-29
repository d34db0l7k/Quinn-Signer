using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Features.Signing
{
    public class DictionaryUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WordBank wordBank;
        [SerializeField] private SessionSelection sessionSelection;
        [SerializeField] private string videosSubfolder = "Reference Videos";

        [Header("Counts")]
        [SerializeField] private int showCount = 20;
        [SerializeField] private int selectLimit = 5;

        [Header("UI")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private DictionaryWordItem wordItemPrefab;
        [SerializeField] private Button saveAndExitButton;
        [SerializeField] private Text selectionCounterText;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private readonly HashSet<string> _selected = new();

        void Start()
        {
            if (!contentParent || !wordItemPrefab || !sessionSelection)
            {
                Debug.LogError("[DictionaryUI] Missing references.");
                return;
            }

            var videoSet = VideoCatalog.IndexWordsWithVideos(videosSubfolder);
            var pool = BuildPool(wordBank, videoSet);
            
            if (pool.Count == 0)
            {
                Debug.LogWarning("[DictionaryUI] No valid dictionary words with videos were found.");
            }

            List<string> twenty;

            if (sessionSelection.HasCandidates20)
            {
                twenty = sessionSelection.Candidates20
                    .Select(w => (w ?? "").Trim().ToLowerInvariant())
                    .Where(w => !string.IsNullOrEmpty(w) && videoSet.Contains(w))
                    .Distinct()
                    .ToList();
            }
            else
            {
                twenty = new List<string>();
            }

            if (twenty.Count < showCount)
            {
                var extras = pool.Where(w => !twenty.Contains(w)).ToList();
                extras = TakeRandom(extras, showCount - twenty.Count);
                twenty.AddRange(extras);
            }

            if (twenty.Count > showCount)
                twenty = twenty.Take(showCount).ToList();

            _selected.Clear();
            if (sessionSelection && sessionSelection.HasWords)
            {
                foreach (var w in sessionSelection.Words)
                {
                    var k = (w ?? "").Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(k) && videoSet.Contains(k))
                        _selected.Add(k);
                }
            }

            EnsureSelectedInCandidates(twenty, videoSet);
            sessionSelection.SetCandidates20(twenty);
            BuildList(twenty);

            if (saveAndExitButton)
            {
                saveAndExitButton.onClick.RemoveAllListeners();
                saveAndExitButton.onClick.AddListener(SaveAndExit);
                saveAndExitButton.interactable = false;
            }
            UpdateSelectionUI();
        }
        
        void EnsureSelectedInCandidates(List<string> candidates, HashSet<string> videoSet)
        {
            if (candidates == null) return;

            for (int i = 0; i < candidates.Count; i++)
                candidates[i] = (candidates[i] ?? "").Trim().ToLowerInvariant();

            var required = new HashSet<string>(_selected);
            if (sessionSelection && sessionSelection.HasWords)
            {
                foreach (var w in sessionSelection.Words)
                {
                    var k = (w ?? "").Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(k)) required.Add(k);
                }
            }

            foreach (var r in required)
            {
                if (videoSet.Contains(r) && !candidates.Contains(r))
                    candidates.Add(r);
            }

            if (candidates.Count > showCount)
            {
                var extras = candidates.Where(w => !required.Contains(w)).ToList();
                var rng = new System.Random(unchecked(Environment.TickCount ^ candidates.Count));
                for (int i = extras.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (extras[i], extras[j]) = (extras[j], extras[i]);
                }

                var final = new List<string>(required.Where(videoSet.Contains));
                foreach (var e in extras)
                {
                    if (final.Count >= showCount) break;
                    if (!final.Contains(e)) final.Add(e);
                }

                candidates.Clear();
                candidates.AddRange(final);
            }
        }
        
        public void RegenerateCandidates()
        {
            var videoSet = VideoCatalog.IndexWordsWithVideos(videosSubfolder);
            var pool     = BuildPool(wordBank, videoSet);
            var twenty   = TakeRandom(pool, showCount);

            EnsureSelectedInCandidates(twenty, videoSet);

            if (twenty.Count > showCount)
                twenty = twenty.Take(showCount).ToList();

            sessionSelection.SetCandidates20(twenty);
            BuildList(twenty);
        }

        static List<string> BuildPool(WordBank bank, HashSet<string> videoSet)
        {
            if (bank)
            {
                return bank.GetWordList()
                           .Select(w => (w ?? "").Trim().ToLowerInvariant())
                           .Where(w => videoSet.Contains(w))
                           .Distinct()
                           .ToList();
            }
            return videoSet.ToList();
        }

        static List<string> TakeRandom(List<string> list, int count)
        {
            var rng = new System.Random(unchecked(Environment.TickCount ^ list.Count));
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            if (list.Count > count) list.RemoveRange(count, list.Count - count);
            return list;
        }

        void BuildList(List<string> words)
        {
            foreach (Transform c in contentParent) Destroy(c.gameObject);

            foreach (var w in words)
            {
                var item = Instantiate(wordItemPrefab, contentParent, false);
                var rt = item.transform as RectTransform;
                if (rt)
                {
                    rt.localScale = Vector3.one;
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot     = new Vector2(0.5f, 1f);
                }

                item.Setup(w, isOn: _selected.Contains(w));
                item.onToggled += HandleItemToggled;
            }

            var contentRT = contentParent as RectTransform;
            if (contentRT)
            {
                Canvas.ForceUpdateCanvases();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
                Canvas.ForceUpdateCanvases();
            }

            UpdateSelectionUI();
        }

        void HandleItemToggled(DictionaryWordItem item, bool isOn)
        {
            if (!item) return;
            var w = item.Word; if (string.IsNullOrEmpty(w)) return;

            if (isOn)
            {
                if (_selected.Count >= selectLimit)
                {
                    item.onToggled -= HandleItemToggled;
                    item.GetComponent<Toggle>()?.SetIsOnWithoutNotify(false);
                    var t = item.GetComponentInChildren<Toggle>();
                    if (t) t.isOn = false;
                    item.onToggled += HandleItemToggled;
                    return;
                }
                _selected.Add(w);
            }
            else
            {
                _selected.Remove(w);
            }
            UpdateSelectionUI();
        }

        void UpdateSelectionUI()
        {
            if (selectionCounterText) selectionCounterText.text = $"Selected: {_selected.Count}/{selectLimit}";
            if (saveAndExitButton)     saveAndExitButton.interactable = _selected.Count > 0 && _selected.Count <= selectLimit;
        }

        void SaveAndExit()
        {
            var picks = _selected
                .Select(w => (w ?? "").Trim().ToLowerInvariant())
                .Where(w => !string.IsNullOrEmpty(w))
                .Take(selectLimit)
                .ToList();

            Debug.Log($"[DictionaryUI] Saving {picks.Count} picks to SessionSelection: [{string.Join(",", picks)}]");

            sessionSelection.SetWords(picks);
            sessionSelection.ResetRuntimeBag();

            if (!string.IsNullOrEmpty(mainMenuSceneName))
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}

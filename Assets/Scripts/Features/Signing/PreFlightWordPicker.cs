namespace Features.Signing
{
// Assets/Scripts/Signing/PreflightWordPicker.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class PreflightWordPicker : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private Features.Signing.WordBank wordBank; // same one you already use
    [SerializeField] private SessionSelection sessionSelection;
    [SerializeField] private int wordsNeeded = 6;
    [SerializeField] private string videosSubfolder = "Reference Videos";
    [SerializeField] private string mainGameSceneName = "MainScene";

    [Header("UI Hookups")]
    [SerializeField] private Transform gridParent;   // Vertical/ Grid Layout Group
    [SerializeField] private GameObject videoTilePrefab; // has RawImage + VideoPlayer + Text

    private List<string> _picked = new();

    void Start()
    {
        // 1) Build the set of words that have videos
        var videoSet = VideoCatalog.IndexWordsWithVideos(videosSubfolder);

        // 2) Intersect with the bank’s current pool and randomly take N (unique)
        var candidates = wordBank.GetWordList();        // already lowercased in your code
        var filtered = new List<string>();
        foreach (var w in candidates)
            if (videoSet.Contains(w)) filtered.Add(w);

        if (filtered.Count == 0)
        {
            Debug.LogWarning("[PreflightWordPicker] No overlap between vocab and videos.");
        }
        Shuffle(filtered);
        var take = Mathf.Min(wordsNeeded, filtered.Count);
        _picked = filtered.GetRange(0, take);

        // 3) Persist for the next scene
        if (sessionSelection) sessionSelection.SetWords(_picked);

        // 4) Build the UI with embedded players
        BuildTiles(_picked);
    }

    void BuildTiles(List<string> words)
    {
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        foreach (var w in words)
        {
            var go = Instantiate(videoTilePrefab, gridParent);
            var vp = go.GetComponentInChildren<VideoPlayer>(true);
            var label = go.GetComponentInChildren<TMPro.TMP_Text>(true);
            var ri = go.GetComponentInChildren<RawImage>(true);

            if (label) label.text = w.ToUpperInvariant();

            var url = VideoCatalog.GetVideoPathForWord(w);
            if (vp && !string.IsNullOrEmpty(url))
            {
                vp.source = VideoSource.Url;
                vp.url = url;
                vp.isLooping = true;
                vp.Play();
            }
        }
    }

    public void ContinueToMain()
    {
        // Optional: ensure we picked at least one word
        if (_picked.Count == 0) Debug.LogWarning("[PreflightWordPicker] Continuing with 0 picked words.");
        SceneManager.LoadScene(mainGameSceneName);
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
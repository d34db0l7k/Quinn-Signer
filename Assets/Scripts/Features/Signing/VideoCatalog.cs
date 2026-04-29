using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Features.Signing
{
    public static class VideoCatalog
    {
        static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();

        public static HashSet<string> IndexWordsWithVideos(string subfolder = "Reference Videos")
        {
            var set = new HashSet<string>();

            TextAsset manifest = Resources.Load<TextAsset>("video_manifest");
            if (manifest == null)
            {
                Debug.LogWarning("[VideoCatalog] Could not load Resources/video_manifest.txt");
                return set;
            }

            var lines = manifest.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var word = Norm(line);
                if (!string.IsNullOrEmpty(word))
                    set.Add(word);
            }

            return set;
        }

        public static string GetVideoPathForWord(string word, string subfolder = "Reference Videos")
        {
            var norm = (word ?? "").Trim().ToLowerInvariant();
            return Path.Combine(Application.streamingAssetsPath, subfolder, norm + ".mp4");
        }

        public static string GetVideoUrlForWord(string word, string subfolder = "Reference Videos")
        {
            var path = GetVideoPathForWord(word, subfolder);
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_ANDROID && !UNITY_EDITOR
            return path;
#else
            return new Uri(path).AbsoluteUri;
#endif
        }

        public static List<string> ListAllVideoWords(string subfolder = "Reference Videos")
        {
            return new List<string>(IndexWordsWithVideos(subfolder));
        }
    }
}
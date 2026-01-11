namespace Features.Signing
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using System;

    public static class VideoCatalog
    {
        static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();

        public static HashSet<string> IndexWordsWithVideos(string subfolder = "Reference Videos")
        {
            var set = new HashSet<string>();
            var root = Path.Combine(Application.streamingAssetsPath, subfolder);
            if (!Directory.Exists(root)) { Debug.LogWarning($"[VideoCatalog] Missing: {root}"); return set; }

            var exts = new HashSet<string> { ".mp4", ".mov", ".m4v", ".webm", ".avi" };
            foreach (var path in Directory.GetFiles(root))
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (!exts.Contains(ext)) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                set.Add(Norm(name));
            }
            return set;
        }

        public static string GetVideoPathForWord(string word, string subfolder = "Reference Videos")
        {
            var root = Path.Combine(Application.streamingAssetsPath, subfolder);
            var norm = Norm(word);
            if (!Directory.Exists(root)) return null;

            foreach (var file in Directory.GetFiles(root))
            {
                var nameNoExt = Norm(Path.GetFileNameWithoutExtension(file));
                if (nameNoExt == norm) return file;
            }
            return null;
        }
        public static string GetVideoUrlForWord(string word, string subfolder = "Reference Videos")
        {
            var path = GetVideoPathForWord(word, subfolder);
            if (string.IsNullOrEmpty(path)) return null;
            return new Uri(path).AbsoluteUri;
        }
        public static List<string> ListAllVideoWords(string subfolder = "Reference Videos")
        {
            var set = IndexWordsWithVideos(subfolder);
            return new List<string>(set);
        }
    }

}
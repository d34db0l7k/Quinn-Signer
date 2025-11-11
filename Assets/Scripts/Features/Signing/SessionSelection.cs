namespace Features.Signing
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "QuinnSigner/SessionSelection")]
    public class SessionSelection : ScriptableObject
    {
        [SerializeField] private List<string> words = new();
        public IReadOnlyList<string> Words => words;
        public void SetWords(IEnumerable<string> src)
        {
            words.Clear();
            foreach (var w in src) if (!string.IsNullOrWhiteSpace(w)) words.Add(w.ToLowerInvariant().Trim());
        }
        public bool HasWords => words != null && words.Count > 0;
    }
}
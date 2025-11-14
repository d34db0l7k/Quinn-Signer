using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Signing
{
    [CreateAssetMenu(menuName = "QuinnSigner/SessionSelection")]
    public class SessionSelection : ScriptableObject
    {
        [SerializeField] private List<string> words = new();
        [SerializeField] private List<string> dictionaryCandidates = new();
        public IReadOnlyList<string> Words => words;
        public bool HasWords => words != null && words.Count > 0;

        // runtime-only bag (not saved)
        [NonSerialized] private List<string> _bag;
        [NonSerialized] private int _bagIndex = 0;
        
        public IReadOnlyList<string> Candidates20 => dictionaryCandidates;
        public bool HasCandidates20 => dictionaryCandidates != null && dictionaryCandidates.Count > 0;

        public void SetCandidates20(IEnumerable<string> words)
        {
            if (dictionaryCandidates == null) dictionaryCandidates = new List<string>();
            dictionaryCandidates.Clear();

            if (words == null) return;

            // normalize to lowercase, de-dup, and keep order
            var seen = new HashSet<string>();
            foreach (var w in words)
            {
                var k = (w ?? "").Trim().ToLowerInvariant();
                if (k.Length == 0 || !seen.Add(k)) continue;
                dictionaryCandidates.Add(k);
            }
        }

        public void ClearCandidates20()
        {
            dictionaryCandidates?.Clear();
        }
        
        static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();

        public void SetWords(IEnumerable<string> src)
        {
            words.Clear();
            if (src != null)
            {
                foreach (var w in src)
                    if (!string.IsNullOrWhiteSpace(w)) words.Add(Norm(w));
            }
            ResetRuntimeBag();
        }

        /// Call this once after setting words (e.g., in Preflight) or to reshuffle mid-run.
        public void ResetRuntimeBag()
        {
            _bag = new List<string>(words);
            var rng = new System.Random(unchecked(Environment.TickCount ^ GetHashCode()));
            for (int i = _bag.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
            }
            _bagIndex = 0;
        }

        /// Pops a single word from the runtime bag (non-repeating until the bag cycles).
        public bool TryPop(out string word)
        {
            word = null;
            if (_bag == null || _bag.Count == 0) ResetRuntimeBag();
            if (_bag == null || _bag.Count == 0) return false;

            if (_bagIndex >= _bag.Count)
            {
                // cycle: reshuffle for a fresh round
                ResetRuntimeBag();
            }

            word = _bag[_bagIndex++];
            return true;
        }

        /// Returns up to N distinct words (does NOT consume the bag).
        public List<string> GetRandomDistinct(int count)
        {
            var result = new List<string>();
            if (!HasWords) return result;

            var tmp = new List<string>(words);
            var rng = new System.Random(unchecked(Environment.TickCount ^ GetHashCode()));
            for (int i = tmp.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (tmp[i], tmp[j]) = (tmp[j], tmp[i]);
            }
            if (count < tmp.Count) tmp.RemoveRange(count, tmp.Count - count);
            return tmp;
        }
    }
}

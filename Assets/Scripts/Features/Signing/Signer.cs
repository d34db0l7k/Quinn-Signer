using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Core.SceneManagement;
using Engine;
using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Features.Signing
{
    public class Signer : MonoBehaviour
    {
        [Header("SLRTK Overhead")]
        public SimpleExecutionEngine engine;

        [Header("Plug in")]
        public WordBank wordBank;
        public Text inferenceText;
        public Image background;

        [SerializeField] private SessionSelection sessionSelection;

        [Header("Win")]
        [SerializeField] private string winSceneName = "WinScene";
        [SerializeField] private float winDelaySeconds = 2f;
        
        [Header("Player Data")]
        [SerializeField] private PlayerHealth playerHealth;

        // internals
        private bool _hasExecuted = false;
        private SceneBindings _bindings;
        private readonly List<string> _filterWords = new();
        private bool _signingActive = false;

        private void Awake()
        {
            if (background) background.color = Color.black;
            if (engine) engine.Toggle(); // hide preview if engine shows on Start
        }

        private void Start()
        {
            StartCoroutine(AssignEnemyLabelsWhenReady());
            StartCoroutine(ForceEngineIdleAtLaunch());
        }

        private void Update()
        {
            if (!_hasExecuted && engine)
            {
                engine.recognizer.AddCallback("check", OnSignRecognized);
                engine.recognizer.outputFilters.Clear();
                ApplyRecognizerFilterToDictionaryWords();
                _hasExecuted = true;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) SimulateCorrectSign();
            if (Input.GetKeyDown(KeyCode.Alpha2)) SimulateIncorrectSign();
            
            UserSigning(); // desktop (Enter/Return); mobile calls are public below
        }

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _bindings = FindFirstObjectByType<SceneBindings>(FindObjectsInactive.Include);

            if (!_bindings)
            {
                // Win/defeat/etc. scenes may not have bindings—just reset for next scene with bindings
                _hasExecuted = false;
                _filterWords.Clear();
                return;
            }

            // Rebind scene references
            wordBank      = _bindings.wordBank;
            engine        = _bindings.engine;
            inferenceText = _bindings.inferenceText;
            background    = _bindings.background;

            _hasExecuted = false;

            if (background) background.color = Color.black;
            if (engine && !engine.enabled) engine.enabled = false;

            ApplyRecognizerFilterToDictionaryWords();

            StartCoroutine(AssignEnemyLabelsWhenReady());
        }

        private IEnumerator AssignEnemyLabelsWhenReady()
        {
            // let spawners place enemies first
            yield return null;

            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            if (enemyLabels == null || enemyLabels.Length == 0) yield break;

            // Prefer preflight-picked words; fallback to WordBank
            List<string> chosen;
            if (sessionSelection && sessionSelection.HasWords)
            {
                chosen = new List<string>(sessionSelection.Words)
                    .Take(enemyLabels.Length)
                    .Select(w => (w ?? "").Trim().ToLowerInvariant())
                    .ToList();
            }
            else if (wordBank)
            {
                chosen = wordBank.GetRandomWords(enemyLabels.Length, unique: true);
            }
            else
            {
                chosen = new List<string>();
            }

            _filterWords.Clear();
            _filterWords.AddRange(chosen);
            ApplyRecognizerFilterToDictionaryWords();

            if (engine)
            {
                engine.recognizer.outputFilters.Clear();
                if (_filterWords.Count > 0)
                    engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            }

            // Set visible words above enemies
            for (int i = 0; i < enemyLabels.Length && i < _filterWords.Count; i++)
                SafeSetEnemyWord(enemyLabels[i], _filterWords[i]);
        }

        private static void SafeSetEnemyWord(EnemyLabel label, string word)
        {
            if (!label || !label.label) return;
            label.SetWord(word);
        }

        private IEnumerator ForceEngineIdleAtLaunch()
        {
            yield return null; // let SimpleExecutionEngine.Start() run once
            if (!engine) yield break;

            engine.Toggle();                   // ensure hidden
            _signingActive = false;
            if (background) background.color = Color.black;
        }

        // --- Desktop key handling (for testing) ---
        private void UserSigning()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (engine && !_signingActive)
                {
                    engine.enabled = true;
                    _signingActive = true;
                }
                if (background) background.color = Color.white;
                engine.Toggle();
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                if (engine) engine.buffer.TriggerCallbacks();
                engine.Toggle();
                if (background) background.color = Color.black;

                if (engine && _signingActive)
                {
                    _signingActive = false;
                    engine.enabled = false;
                }
            }
        }

        // --- Mobile button hooks (wire your UI buttons or HoldToSignButtonRelay here) ---
        public void BeginMobileSign()
        {
            if (engine && !_signingActive)
            {
                engine.enabled = true;
                _signingActive = true;
            }
            if (background) background.color = Color.white;
            if (engine) engine.Toggle();
        }

        public void EndMobileSign()
        {
            if (engine) engine.buffer.TriggerCallbacks();
            if (engine) engine.Toggle();
            if (background) background.color = Color.black;

            if (engine && _signingActive)
            {
                _signingActive = false;
                engine.enabled = false;
            }
        }

        // --- DEV HELPERS --- \\
        void SimulateCorrectSign()
        {
            // Grab any active enemy word
            var labels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            foreach (var l in labels)
            {
                if (!string.IsNullOrEmpty(l.targetWord))
                {
                    OnSignRecognized(l.targetWord);
                    return;
                }
            }
            // fallback if none present
            OnSignRecognized("dev_correct_missing");
        }

        void SimulateIncorrectSign()
        {
            // Choose a token that won't match anything
            OnSignRecognized("__dev_wrong__");
        }
        
        private void OnSignRecognized(string rawInput)
        {
            var signed = (rawInput ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(signed))
            {
                if (inferenceText) { inferenceText.text = rawInput; inferenceText.color = Color.red; }
                return;
            }

            // find a matching enemy label by word
            var labels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            EnemyLabel match = null;
            foreach (var label in labels)
            {
                if (!label) continue;
                if (string.Equals(label.targetWord, signed, System.StringComparison.OrdinalIgnoreCase))
                {
                    match = label;
                    break;
                }
            }

            if (!match)
            {
                if (inferenceText) { inferenceText.text = signed; inferenceText.color = Color.red; }
                if (playerHealth) playerHealth.Damage(1);
                return;
            }

            // explode that enemy
            var controller = match.GetComponentInParent<EnemyController>() ?? match.GetComponent<EnemyController>();
            if (controller) controller.Explode(); else Destroy(match.gameObject);

            if (inferenceText) { inferenceText.text = signed; inferenceText.color = Color.green; }

            RemoveWordFromList(signed);
            StartCoroutine(CheckForWinNextFrame());
        }

        private void RemoveWordFromList(string word)
        {
            if (string.IsNullOrEmpty(word)) return;
            var key = word.Trim().ToLowerInvariant();

            for (int i = sessionSelection.words.Count - 1; i >= 0; i--)
            {
                var s = sessionSelection.words[i];
                if (string.Equals(s?.Trim(), key, System.StringComparison.OrdinalIgnoreCase))
                {
                    sessionSelection.words.RemoveAt(i);
                }
            }

            if (engine)
            {
                engine.recognizer.outputFilters.Clear();
                if (_filterWords.Count > 0)
                    engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            }
        }

        private IEnumerator CheckForWinNextFrame()
        {
            yield return null; // wait for destroys to complete

            bool noWordsLeft = _filterWords == null || _filterWords.Count == 0;
            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            bool noEnemiesLeft = enemyLabels == null || enemyLabels.Length == 0;

            if (noEnemiesLeft) TriggerWin();
        }

        private void TriggerWin()
        {
            if (engine) engine.enabled = false;
            if (background) background.color = Color.black;
            StartCoroutine(LoadWinAfterDelay());
        }

        private IEnumerator LoadWinAfterDelay()
        {
            yield return new WaitForSeconds(winDelaySeconds);
            if (!string.IsNullOrEmpty(winSceneName))
                SceneManager.LoadScene(winSceneName, LoadSceneMode.Single);
        }

        // external helper for other scripts to notify a kill
        public void HandleEnemyKilled(EnemyLabel label)
        {
            if (label && !string.IsNullOrEmpty(label.targetWord))
                RemoveWordFromList(label.targetWord.ToLowerInvariant());

            StartCoroutine(CheckForWinNextFrame());
        }
        private List<string> GetDictionaryWords()
        {
            // Use your real API only: HasWords + Words
            if (sessionSelection != null && sessionSelection.HasWords && sessionSelection.Words != null)
                return Normalize(sessionSelection.Words);

            return new List<string>();
        }

        private static List<string> Normalize(IEnumerable<string> raw)
        {
            if (raw == null) return new List<string>();
            var list = new List<string>();
            foreach (var w in raw)
            {
                var s = (w ?? "").Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(s) && !list.Contains(s))
                    list.Add(s);
            }
            return list;
        }

        private void ApplyRecognizerFilterToDictionaryWords()
        {
            if (engine == null) return;

            var focus = GetDictionaryWords();
            engine.recognizer.outputFilters.Clear();
            if (focus.Count > 0)
                engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(focus));

            // Optional: keep a local copy if you also match enemies by this list
            _filterWords.Clear();
            _filterWords.AddRange(focus);
        }

    }
}

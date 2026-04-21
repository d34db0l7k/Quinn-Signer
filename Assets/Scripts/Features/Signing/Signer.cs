using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Core.SceneManagement;
using Engine;
using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
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
        public Text confidenceScoreText;
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
            if (engine) engine.Toggle();
        }

        private void Start()
        {
            if (sessionSelection && sessionSelection.HasWords)
            {
                _filterWords.Clear();
                _filterWords.AddRange(sessionSelection.Words.Select(w => (w ?? "").Trim().ToLowerInvariant()));
                //ApplyRecognizerFilterToDictionaryWords();
            }
            StartCoroutine(AssignEnemyLabelsWhenReady());
            StartCoroutine(ForceEngineIdleAtLaunch());
        }

        private void Update()
        {
            if (!_hasExecuted && engine && engine.recognizer != null)
            {
                engine.recognizer.AddCallback("check", (word) => OnSignRecognized(word, 1.0f));
                engine.recognizer.outputFilters.Clear();
                ApplyRecognizerFilterToDictionaryWords();
                _hasExecuted = true;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) SimulateCorrectSign();
            if (Input.GetKeyDown(KeyCode.Alpha2)) SimulateIncorrectSign();
            UserSigning();
        }

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _bindings = FindFirstObjectByType<SceneBindings>(FindObjectsInactive.Include);

            if (!_bindings)
            {
                _hasExecuted = false;
                _filterWords.Clear();
                return;
            }

            wordBank            = _bindings.wordBank;
            engine              = _bindings.engine;
            inferenceText       = _bindings.inferenceText;
            confidenceScoreText = _bindings.condifenceScoreText;
            background          = _bindings.background;

            _hasExecuted = false;

            if (background) background.color = Color.black;
            if (engine && !engine.enabled) engine.enabled = false;

            ApplyRecognizerFilterToDictionaryWords();

            StartCoroutine(AssignEnemyLabelsWhenReady());
        }

        public void RefreshEnemyLabels()
        {
            StartCoroutine(AssignEnemyLabelsWhenReady());
        }

        private IEnumerator AssignEnemyLabelsWhenReady()
        {
            yield return null;

            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            if (enemyLabels == null || enemyLabels.Length == 0) yield break;

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

            if (engine && engine.recognizer != null)
            {
                engine.recognizer.outputFilters.Clear();
                if (_filterWords.Count > 0)
                    engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            }

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
            yield return null;
            if (!engine) yield break;

            engine.Toggle();
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

        // --- Mobile button hooks --- \\
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
            var labels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            foreach (var l in labels)
            {
                if (!string.IsNullOrEmpty(l.targetWord))
                {
                    OnSignRecognized(l.targetWord, 1.0f);
                    return;
                }
            }
            OnSignRecognized("dev_correct_missing", 1.0f);
        }

        void SimulateIncorrectSign()
        {
            OnSignRecognized("__dev_wrong__", 0.0f);
        }
        
        private void OnSignRecognized(string rawInput, float confidenceScore)
        {
            var signed = (rawInput ?? "").Trim().ToLowerInvariant();
            var normalizedScore = Mathf.RoundToInt(confidenceScore * 100f);
            if (string.IsNullOrEmpty(signed))
            {
                SetInferenceTextFields(signed, normalizedScore, Color.red);
                return;
            }

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
                SetInferenceTextFields(signed, normalizedScore, Color.red);
                if (playerHealth) playerHealth.Damage(1);
                return;
            }

            // The initial if statements in Update() within EnemyEncounterController should handle mutual exclusivity
            // of boss and enemy appearances. So, in theory, only one of these controllers should have any valid
            // reference during any encounter.
            var enemyController = match.GetComponentInParent<EnemyController>() ?? match.GetComponent<EnemyController>();
            var bossController = match.GetComponentInParent<TutorialBossController>() ?? match.GetComponent<TutorialBossController>();
            if (enemyController) HandleEnemySign(match, enemyController, signed);
            else if (bossController) HandleTutorialBossSign(match, bossController, signed);
            else Destroy(match.gameObject);
            SetInferenceTextFields(signed, normalizedScore, Color.green);
        }

        private void HandleEnemySign(EnemyLabel match, EnemyController cont, string signed)
        {
            cont.Explode();
            RemoveWordFromList(signed);
            StartCoroutine(CheckForWinNextFrame());
        }

        private void HandleTutorialBossSign(EnemyLabel match, TutorialBossController cont, string signed)
        {
            cont.HandleSignedWord(match, 1);
        }

        private void SetInferenceTextFields(string signed, int score, Color textColor)
        {
            if (inferenceText)
            {
                inferenceText.text = signed;
                inferenceText.color = textColor;
                if (confidenceScoreText && score >= 0f && score <= 100)
                {
                    confidenceScoreText.text = score.ToString() + "%";
                    confidenceScoreText.color = textColor;
                }
            }
        }

        private void RemoveWordFromList(string word)
        {
            if (string.IsNullOrEmpty(word)) return;
            var key = word.Trim().ToLowerInvariant();

            // THIS is what actually tracks progress
            for (int i = _filterWords.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_filterWords[i], key, System.StringComparison.OrdinalIgnoreCase))
                    _filterWords.RemoveAt(i);
            }

            // Optional (safe) remove from session list too
            if (sessionSelection != null && sessionSelection.words != null)
            {
                for (int i = sessionSelection.words.Count - 1; i >= 0; i--)
                {
                    var s = sessionSelection.words[i];
                    if (string.Equals(s?.Trim().ToLowerInvariant(), key, System.StringComparison.OrdinalIgnoreCase))
                        sessionSelection.words.RemoveAt(i);
                }
            }

            // Update recognizer so it only listens for remaining words
            if (engine != null && engine.recognizer != null)
            {
                engine.recognizer.outputFilters.Clear();
                if (_filterWords.Count > 0)
                    engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            }
        }


        private IEnumerator CheckForWinNextFrame()
        {
            yield return null;

            bool noWordsLeft = _filterWords == null || _filterWords.Count == 0;

            if (noWordsLeft)
                TriggerWin();
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

        public void HandleEnemyKilled(EnemyLabel label)
        {
            if (label && !string.IsNullOrEmpty(label.targetWord))
                RemoveWordFromList(label.targetWord.ToLowerInvariant());

            StartCoroutine(CheckForWinNextFrame());
        }

        private List<string> GetDictionaryWords()
        {
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
            if (engine.recognizer == null) return; // <- THIS is the important fix

            var focus = GetDictionaryWords();

            engine.recognizer.outputFilters.Clear();
            if (focus.Count > 0)
                engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(focus));

            _filterWords.Clear();
            _filterWords.AddRange(focus);
        }
    }
}
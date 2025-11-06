using System.Collections;
using System.Collections.Generic;
using Common;
using Core.SceneManagement;
using Engine;
using Features.Gameplay.Entities.Enemy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Features.Signing
{
    public class Signer : MonoBehaviour
    {
        [Header("SLRTK Overhead")]
        public SimpleExecutionEngine engine;

        [Space]

        [Header("Plug in")]
        public WordBank wordBank;
        public Text scoreText;
        public Text inferenceText;
        public Image background;

        [Header("Win vars")]
        [SerializeField] private string winSceneName = "WinScene";
        [SerializeField] private float winDelaySeconds = 2;

        // Local vars
        private int _score = 0;
        // variable for filters and callbacks to execute only once
        private bool _hasExecuted;
        private SceneBindings _bindings;
        private List<string> _filterWords = new List<string>();

        private bool _signingActive = false;

        private void Awake()
        {
            if (background) background.color = Color.black;
            if (engine) engine.Toggle();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            // set score to be "score: " initially on start up
            UpdateScoreText();
            // set up text fields of enemies
            StartCoroutine(AssignEnemyLabelsWhenReady());
            StartCoroutine(ForceEngineIdleAtLaunch());
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_hasExecuted && engine)
            {
                // where initialization goes
                engine.recognizer.AddCallback("check", OnSignRecognized);
                engine.recognizer.outputFilters.Clear();

                _hasExecuted = true;
            }
            UserSigning();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _bindings = FindFirstObjectByType<SceneBindings>(FindObjectsInactive.Include);

            if (!_bindings)
            {
                // Not every scene will have bindings (e.g., defeat screen). That's OK.
                Debug.Log($"[Signer] No SceneBindings found in scene '{scene.name}' (mode={mode}). Skipping rebind.");
                _hasExecuted = false;        // so we re-init next time there *is* a scene with bindings
                _filterWords.Clear();
                return;
            }

            wordBank = _bindings.wordBank;
            engine = _bindings.engine;
            scoreText = _bindings.scoreText;
            inferenceText = _bindings.inferenceText;
            background = _bindings.background;

            _hasExecuted = false;

            if (wordBank) wordBank.ResetWorkingWords(); // FRESH POOL OF WORDS FOR EACH SCENE

            // UI/engine initial state
            if (background) background.color = Color.black;
            if (engine && !engine.enabled) engine.enabled = false;

            InitializeHUDAndWord();
            StartCoroutine(AssignEnemyLabelsWhenReady());
            // StartCoroutine(ForceEngineIdleAtLaunch());
        }
        private void InitializeHUDAndWord()
        {
            UpdateScoreText();
        }
        private IEnumerator AssignEnemyLabelsWhenReady()
        {
            // Wait a couple frames to let spawners finish
            yield return null;
            yield return null;

            // Grab whatever is in-scene right now
            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);

            if (enemyLabels == null || enemyLabels.Length == 0)
            {
                yield break;
            }

            // Pull a set of unique words (or allow repeats if you prefer)
            _filterWords = wordBank.GetRandomWords(enemyLabels.Length, unique: true);
            engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            for (var i = 0; i < enemyLabels.Length && i < _filterWords.Count; i++)
            {
                SafeSetEnemyWord(enemyLabels[i], _filterWords[i]);
            }

            if (engine)
            {
                engine.recognizer.outputFilters.Clear();
                engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            }
        }
        private static void SafeSetEnemyWord(EnemyLabel label, string word)
        {
            if (!label) return;
            if (!label.label) return;
            
            label.SetWord(word);
        }
        private IEnumerator ForceEngineIdleAtLaunch()
        {
            yield return null;                 // let SimpleExecutionEngine.Start() run
            if (!engine) yield break;

            engine.Toggle();                   // hide preview (engine showed it in Start)
            _signingActive = false;
            if (background) background.color = Color.black;

        }
        // Functions to handle scoring behavior
        public void AddScore(int points)
        {
            _score += points;
            UpdateScoreText();
        }
        private void UpdateScoreText()
        {
            scoreText.text = "Score: " + _score;
        }
        private void UserSigning()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (engine && !_signingActive)
                {
                    engine.enabled = true;
                    _signingActive = true;
                }
                background.color = Color.white;
                engine.Toggle();
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                engine.buffer.TriggerCallbacks();
                engine.Toggle();
                background.color = Color.black;

                if (engine && _signingActive)
                {
                    _signingActive = false;
                    engine.enabled = false;
                }
            }
        }
        private void OnSignRecognized(string rawInput)
        {
            var signed = (rawInput ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(signed))
            {
                if (inferenceText) { inferenceText.text = rawInput; inferenceText.color = Color.red; }
                return;
            }

            // find a matching enemy label by word (case-insensitive)
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
                return;
            }

            // explode that enemy + award points based on the matched word length
            var pts = Mathf.Max(1, (match.targetWord.Length / 3) + 1);
            AddScore(pts);

            var controller = match.GetComponentInParent<EnemyController>() ?? match.GetComponent<EnemyController>();
            if (controller) controller.Explode();
            else Destroy(match.gameObject);

            if (inferenceText) { inferenceText.text = signed; inferenceText.color = Color.green; }

            // remove signed word
            RemoveWordFromList(signed);
            StartCoroutine(CheckForWinNextFrame());
        }
        /*for mobile signing button usage*/
        public void BeginMobileSign()
        {
            if (engine != null && !_signingActive)
            {
                engine.enabled = true;
                _signingActive = true;
            }

            background.color = Color.white;
            engine.Toggle();
        }
        public void EndMobileSign()
        {
            engine.buffer.TriggerCallbacks();
            engine.Toggle();
            background.color = Color.black;

            if (engine != null && _signingActive)
            {
                _signingActive = false;
                engine.enabled = false;
            }
        }
        
        private void RemoveWordFromList(string word)
        {
            if (string.IsNullOrEmpty(word)) return;
            var key = word.Trim().ToLowerInvariant();

            // Remove one instance of the word from filterWords (they were unique anyway)
            for (int i = 0; i < _filterWords.Count; i++)
            {
                if (_filterWords[i] == key)
                {
                    _filterWords.RemoveAt(i);
                    break;
                }
            }

            // Rebuild recognizer filter with the remaining words
            if (engine)
            {
                engine.recognizer.outputFilters.Clear();
                if (_filterWords.Count > 0)
                    engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(_filterWords));
            }
        }

        private IEnumerator CheckForWinNextFrame()
        {
            // wait one frame so destroyed enemies are actually gone
            yield return null;

            // If nothing left to check (filterWords empty OR wordBank out) AND no EnemyLabels remain → win
            bool noWordsLeft = (_filterWords == null || _filterWords.Count == 0) || (wordBank && wordBank.GetWordList().Count == 0);

            var enemyLabels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
            bool noEnemiesLeft = enemyLabels == null || enemyLabels.Length == 0;

            if (noWordsLeft && noEnemiesLeft)
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

        // dev kill key helper
        public void HandleEnemyKilled(EnemyLabel label)
        {
            if (label && !string.IsNullOrEmpty(label.targetWord))
                RemoveWordFromList(label.targetWord.ToLowerInvariant());

            StartCoroutine(CheckForWinNextFrame());
        }
    }
}

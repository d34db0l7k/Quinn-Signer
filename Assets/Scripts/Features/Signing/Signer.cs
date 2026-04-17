using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Features.Gameplay.Entities.Enemy;
using Features.Gameplay.Entities.Player;

namespace Features.Signing
{
    public class Signer : MonoBehaviour
    {
        [Header("UI")]
        public Text inferenceText;
        public Text confidenceScoreText;
        public Image background;

        [Header("Session")]
        [SerializeField] private SessionSelection sessionSelection;

        [Header("Player")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Win")]
        [SerializeField] private string winSceneName = "WinScene";
        [SerializeField] private float winDelaySeconds = 2f;

        private readonly List<string> _filterWords = new();
        private bool _initialized;

        private void Awake()
        {
            if (background) background.color = Color.black;
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
        }

        private void Update()
        {
            if (!_hasExecuted && engine)
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

            _initialized = true;
            ApplyRecognizerFilterToDictionaryWords();
        }

        private IEnumerator AssignEnemyLabelsWhenReady()
        {
            yield return null;

            var enemies = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);

            if (enemies == null || enemies.Length == 0)
                yield break;

            List<string> chosen = new();

            if (sessionSelection != null && sessionSelection.HasWords)
                chosen = sessionSelection.Words.ToList();

            _filterWords.Clear();
            _filterWords.AddRange(chosen);

            int count = Mathf.Min(enemies.Length, chosen.Count);

            for (int i = 0; i < count; i++)
            {
                if (enemies[i] != null)
                    enemies[i].SetWord(chosen[i]);
            }
        }

        public void HandleEnemyKilled(EnemyLabel label)
        {
            if (GameModeState.HintTypingModeActive)
            {
                return;
            }

            if (label == null || string.IsNullOrEmpty(label.targetWord))
                return;

            string word = label.targetWord.Trim().ToLowerInvariant();

            if (_filterWords.Contains(word))
                _filterWords.Remove(word);

            CheckWin();
        }

        public void OnSignRecognized(string rawInput, float confidence)
        {
            if (GameModeState.HintTypingModeActive)
            {
                return;
            }

            string signed = (rawInput ?? "").Trim().ToLowerInvariant();

            var match = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None)
                .FirstOrDefault(e => e != null && e.targetWord == signed);

            if (!match)
            {
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
                if (score >= 0f && score <= 100)
                {
                    confidenceScoreText.text = score.ToString() + "%";
                    confidenceScoreText.color = textColor;
                }
            }
        }

        private void RemoveWord(string word)
        {
            for (int i = _filterWords.Count - 1; i >= 0; i--)
            {
                if (_filterWords[i] == word)
                    _filterWords.RemoveAt(i);
            }
        }

        private void CheckWin()
        {
            var aliveEnemies = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None)
                .Where(e => e != null && !string.IsNullOrEmpty(e.targetWord))
                .ToList();

            if (aliveEnemies.Count == 0)
            {
                StartCoroutine(LoadWin());
            }
        }

        private IEnumerator LoadWin()
        {
            yield return new WaitForSeconds(winDelaySeconds);
            SceneManager.LoadScene(winSceneName);
        }

        private void ApplyRecognizerFilterToDictionaryWords()
        {
        }

        public void BeginMobileSign()
        {
            if (GameModeState.HintTypingModeActive)
                return;

            if (background) background.color = Color.white;
        }

        public void EndMobileSign()
        {
            if (GameModeState.HintTypingModeActive)
                return;

            if (background) background.color = Color.black;
        }
    }
}
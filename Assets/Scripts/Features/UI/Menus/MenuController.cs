using UnityEngine;
using UnityEngine.SceneManagement;

namespace Features.UI.Menus
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuController : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("CanvasGroup to show/hide. If empty, this component's CanvasGroup is used.")]
        public CanvasGroup target;

        [Header("Behavior Flags")]
        [Tooltip("Hide the target on Start(). Turn OFF for Main Menu panels.")]
        public bool autoHideOnStart = true;

        [Tooltip("Listen for Esc/Back and double-tap to toggle. Turn OFF on Main Menu.")]
        public bool listenForPauseInputs = true;

        [Tooltip("Pause/unpause game time when showing/hiding. Turn OFF on Main Menu.")]
        public bool manageTimeScale = true;

        [Header("Mobile double-tap")]
        public bool enableDoubleTapToPause = true;
        public float doubleTapMaxDelay = 0.35f;
        [Tooltip("0 = DPI-based threshold (~0.45 cm)")]
        public float doubleTapMaxMove = 0f;

        // internals
        private CanvasGroup _selfCg;
        private float _lastTapTime = -1f;
        private Vector2 _lastTapPos;
        private bool _tapArmed = false;

        private void Awake()
        {
            _selfCg = GetComponent<CanvasGroup>();
            if (target == null) target = _selfCg; // default to self
        }

        private void Start()
        {
            if (autoHideOnStart) HideMenu();
            if (manageTimeScale) Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            if (manageTimeScale && Time.timeScale == 0f) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!listenForPauseInputs) return;

        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_ANDROID
            if (Input.GetKeyUp(KeyCode.Escape))
                TogglePause();
        #endif
            if (enableDoubleTapToPause)
                HandleDoubleTapToPause();
        }

        private void Open()  { ShowMenu(); if (manageTimeScale) Time.timeScale = 0f; }
        private void Close() { HideMenu(); if (manageTimeScale) Time.timeScale = 1f; }

        private void TogglePause()
        {
            if (IsVisible(target)) Close();
            else Open();
        }

        /* ---------- Core show/hide ---------- */
        private void ShowMenu()
        {
            if (!target) return;
            target.alpha = 1f;
            target.interactable = true;
            target.blocksRaycasts = true;
        }

        private void HideMenu()
        {
            if (!target) return;
            target.alpha = 0f;
            target.interactable = false;
            target.blocksRaycasts = false;
        }

        private static bool IsVisible(CanvasGroup cg) => cg && cg.interactable && cg.blocksRaycasts && cg.alpha > 0.99f;

        /* ---------- Double-tap detector (unscaled, DPI-aware) ---------- */
        private void HandleDoubleTapToPause()
        {
            if (Input.touchCount == 0) return;

            var now = Time.unscaledTime;
            var dpi = Screen.dpi <= 0 ? 326f : Screen.dpi;   // fallback
            var movePx = (doubleTapMaxMove > 0f) ? doubleTapMaxMove : 0.45f * dpi;
            var movePx2 = movePx * movePx;

            for (var i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase != TouchPhase.Began) continue;

                if (_tapArmed && (now - _lastTapTime) <= doubleTapMaxDelay)
                {
                    if ((t.position - _lastTapPos).sqrMagnitude <= movePx2)
                    {
                        TogglePause();
                        _tapArmed = false;
                        return;
                    }
                }

                _tapArmed = true;
                _lastTapTime = now;
                _lastTapPos = t.position;
            }

            if (_tapArmed && (now - _lastTapTime) > doubleTapMaxDelay) _tapArmed = false;
        }

        /* ---------- Scene buttons ---------- */
        public void RestartGame()
        {
            if (manageTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene("TitleScene");
        }
        public void StartGame()
        {
            if (manageTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene("MainSceneMobile");
        }
        public void StartGlyphway()
        {
            if (manageTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene("GlyphwayScene");
        }
        public void StartTraining()
        {
            if (manageTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene("TrainingScene");
        }

        public void StartDictionary()
        {
            if (manageTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene("DictionaryScene");
        }
        public void EnterShop()
        {
            if (manageTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene("ShopScene");
        }
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

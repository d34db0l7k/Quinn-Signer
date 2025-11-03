using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class MenuLogic : MonoBehaviour
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
    CanvasGroup selfCg;
    float lastTapTime = -1f;
    Vector2 lastTapPos;
    bool tapArmed = false;

    void Awake()
    {
        selfCg = GetComponent<CanvasGroup>();
        if (target == null) target = selfCg; // default to self
    }

    void Start()
    {
        if (autoHideOnStart) HideMenu();
        if (manageTimeScale) Time.timeScale = 1f;
    }

    void OnDisable()
    {
        if (manageTimeScale && Time.timeScale == 0f) Time.timeScale = 1f;
    }

    void Update()
    {
        if (!listenForPauseInputs) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_ANDROID
        if (Input.GetKeyUp(KeyCode.Escape))
            TogglePause();
#endif
        if (enableDoubleTapToPause)
            HandleDoubleTapToPause();
    }

    /* ---------- Public actions (use these from buttons) ---------- */
    public void Open()  { ShowMenu(); if (manageTimeScale) Time.timeScale = 0f; }
    public void Close() { HideMenu(); if (manageTimeScale) Time.timeScale = 1f; }

    public void TogglePause()
    {
        if (IsVisible(target)) Close();
        else Open();
    }

    /* ---------- Core show/hide ---------- */
    void ShowMenu()
    {
        if (!target) return;
        target.alpha = 1f;
        target.interactable = true;
        target.blocksRaycasts = true;
    }

    void HideMenu()
    {
        if (!target) return;
        target.alpha = 0f;
        target.interactable = false;
        target.blocksRaycasts = false;
    }

    static bool IsVisible(CanvasGroup cg) => cg && cg.interactable && cg.blocksRaycasts && cg.alpha > 0.99f;

    /* ---------- Double-tap detector (unscaled, DPI-aware) ---------- */
    void HandleDoubleTapToPause()
    {
        if (Input.touchCount == 0) return;

        float now = Time.unscaledTime;
        float dpi = Screen.dpi <= 0 ? 326f : Screen.dpi;   // fallback
        float movePx = (doubleTapMaxMove > 0f) ? doubleTapMaxMove : 0.45f * dpi;
        float movePx2 = movePx * movePx;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            if (tapArmed && (now - lastTapTime) <= doubleTapMaxDelay)
            {
                if ((t.position - lastTapPos).sqrMagnitude <= movePx2)
                {
                    TogglePause();
                    tapArmed = false;
                    return;
                }
            }

            tapArmed = true;
            lastTapTime = now;
            lastTapPos = t.position;
        }

        if (tapArmed && (now - lastTapTime) > doubleTapMaxDelay) tapArmed = false;
    }

    /* ---------- Scene buttons (optional) ---------- */
    public void RestartGame()
    {
        if (manageTimeScale) Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void StartGame()
    {
        if (manageTimeScale) Time.timeScale = 1f;
        SceneManager.LoadScene("MainSceneMobile");
    }

    public void InfiniteRunner()
    {
        if (manageTimeScale) Time.timeScale = 1f;
        SceneManager.LoadScene("InfiniteRunner");
    }
    public void Shop()
    {
        if (manageTimeScale) Time.timeScale = 1f;
        SceneManager.LoadScene("Shop");
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

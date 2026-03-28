using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavBarManager : MonoBehaviour
{
    private static NavBarManager _instance;

    [Header("Hide On Scenes")]
    private string[] hideOnScenes = { "GameOverScene", "GlyphwayScene", "TrainingScene", "WinScene"};

    [Header("Nav Buttons")]
    public Button homeButton;
    public Button missionsButton;
    public Button dictionaryButton;
    public Button garageButton;
    public Button shopButton;
    public Button settingsButton;

    [Header("Highlight Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.4f);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HighlightCurrent(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldHide = System.Array.Exists(hideOnScenes, s => s == scene.name);
        gameObject.SetActive(!shouldHide);
        HighlightCurrent(scene.name);
    }

    private void HighlightCurrent(string sceneName)
    {
        SetColor(homeButton, inactiveColor);
        SetColor(missionsButton, inactiveColor);
        SetColor(dictionaryButton, inactiveColor);
        SetColor(garageButton, inactiveColor);
        SetColor(shopButton, inactiveColor);
        SetColor(settingsButton, inactiveColor);

        switch (sceneName)
        {
            case "TitleScene":        SetColor(homeButton, activeColor);       break;
            case "MissionsScene":     SetColor(missionsButton, activeColor);   break;
            case "DictionaryScene":   SetColor(dictionaryButton, activeColor); break;
            case "GarageScene":       SetColor(garageButton, activeColor);     break;
            case "ShopScene":         SetColor(shopButton, activeColor);       break;
            case "SettingsScene":     SetColor(settingsButton, activeColor);   break;
        }
    }

    private void SetColor(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    /* ---------- Navigation ---------- */
    public void GoToHome()       { SceneManager.LoadScene("TitleScene"); }
    public void GoToMissions()   { SceneManager.LoadScene("MissionsScene"); }
    public void GoToDictionary() { SceneManager.LoadScene("DictionaryScene"); }
    public void GoToGarage()     { SceneManager.LoadScene("GarageScene"); }
    public void GoToShop()       { SceneManager.LoadScene("ShopScene"); }
    public void GoToSettings()   { SceneManager.LoadScene("SettingsScene"); }
}
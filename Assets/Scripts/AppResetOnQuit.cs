using UnityEngine;
using Features.UI;
using Features.Signing;
public class AppResetOnQuit : MonoBehaviour
{
    [Header("Primary refs (optional)")]
    [SerializeField] private SessionSelection sessionSelection; // your dictionary pick store
    [SerializeField] private SkinManager skinManager;           // may be null / per-scene

    [Header("Fallback if no SkinManager is found")]
    [Tooltip("If no SkinManager instance is present when quitting, these skins will be used to clear ownership keys.")]
    [SerializeField] private Skin[] fallbackAllSkins;           // drag all Skin assets here (or leave empty to use Resources)

    [Header("Behavior")]
    [SerializeField] private bool persistAcrossScenes = true;

    void Awake()
    {
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
    }

    void OnApplicationQuit() => ResetAll();
    void OnApplicationPause(bool paused) { if (paused) ResetAll(); }

    void ResetAll()
    {
        // 1) Clear dictionary picks
        if (sessionSelection && sessionSelection.words != null)
            sessionSelection.words.Clear();

        // 2) Reset crystal count
        CrystalWallet.ResetTo(0);

        // 3) Clear owned/equipped skins
        var sm = skinManager != null
            ? skinManager
            : FindFirstObjectByType<SkinManager>(FindObjectsInactive.Include);

        if (sm != null)
        {
            // Use your real manager’s API if present
            foreach (var s in sm.allSkins)
                sm.SetOwned(s, false);
            sm.ClearCurrentSkin();
        }
        else
        {
            // No SkinManager in this scene → clear PlayerPrefs keys directly
            var skins = fallbackAllSkins;
            if ((skins == null || skins.Length == 0))
            {
                // last resort: load all skins from Resources (only works if you store them there)
                skins = Resources.LoadAll<Skin>("");
            }

            if (skins != null)
            {
                foreach (var s in skins)
                {
                    if (!s) continue;
                    PlayerPrefs.SetInt(SkinManager.OwnedKey + s.id, 0); // uses your manager’s key scheme
                }
            }
            PlayerPrefs.DeleteKey(SkinManager.CurrentKey);
        }

        PlayerPrefs.Save();

        #if UNITY_EDITOR
        Debug.Log("[AppResetOnQuit] Reset words, crystals, and skins on quit/pause.");
        #endif
    }
}

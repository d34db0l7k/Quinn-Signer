using UnityEngine;
using Features.UI;
using Features.Signing;
public class AppResetOnQuit : MonoBehaviour
{
    [Header("Primary refs (optional)")]
    [SerializeField] private SessionSelection sessionSelection;
    [SerializeField] private SkinManager skinManager;

    [Header("Fallback if no SkinManager is found")]
    [Tooltip("If no SkinManager instance is present when quitting, these skins will be used to clear ownership keys.")]
    [SerializeField] private Skin[] fallbackAllSkins;

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
        if (sessionSelection && sessionSelection.words != null)
            sessionSelection.words.Clear();
        
        CrystalWallet.ResetTo(0);
        
        var sm = skinManager != null
            ? skinManager
            : FindFirstObjectByType<SkinManager>(FindObjectsInactive.Include);

        if (sm != null)
        {
            foreach (var s in sm.allSkins)
                sm.SetOwned(s, false);
            sm.ClearCurrentSkin();
        }
        else
        {
            var skins = fallbackAllSkins;
            if ((skins == null || skins.Length == 0))
            {
                skins = Resources.LoadAll<Skin>("");
            }

            if (skins != null)
            {
                foreach (var s in skins)
                {
                    if (!s) continue;
                    PlayerPrefs.SetInt(SkinManager.OwnedKey + s.id, 0);
                }
            }
            PlayerPrefs.DeleteKey(SkinManager.CurrentKey);
        }

        PlayerPrefs.Save();
    }
}

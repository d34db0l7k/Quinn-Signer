using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("Catalog")]
    public List<Skin> knownSkins = new List<Skin>(); // assign in Inspector

    [Header("IDs")]
    [Tooltip("ID of the always-available default skin (e.g., \"ship_default\").")]
    public string defaultId = "ship_default";

    // runtime state (no persistence)
    private readonly HashSet<string> _unlocked = new HashSet<string>();
    private string _equippedId;

    // --- lifecycle ---

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsForDomainReload()
    {
        // ensures a clean state if Enter Play Mode Options keep domain alive
        Instance = null;
    }

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeRuntimeState();
    }

    private void InitializeRuntimeState()
    {
        _unlocked.Clear();

        // Make sure default skin exists and is unlocked/equipped
        if (!knownSkins.Any(s => s && s.id == defaultId))
        {
            Debug.LogWarning($"SkinManager: defaultId '{defaultId}' not found in knownSkins. Using first skin.");
            if (knownSkins.Count > 0) defaultId = knownSkins[0].id;
        }

        _unlocked.Add(defaultId);
        _equippedId = defaultId;
    }

    // --- API ---

    public bool IsUnlocked(Skin skin) => skin != null && _unlocked.Contains(skin.id);

    public bool IsEquipped(Skin skin) => skin != null && skin.id == _equippedId;

    public Skin EquippedSkin => knownSkins.FirstOrDefault(s => s && s.id == _equippedId)
                                ?? knownSkins.FirstOrDefault(s => s && s.id == defaultId);

    public void Equip(Skin skin)
    {
        if (skin == null) return;
        _unlocked.Add(skin.id);        // buying or equipping unlocks
        _equippedId = skin.id;
        // (spawn code in your Runner scene will read EquippedSkin)
    }

    public void Unequip(Skin skin)
    {
        if (skin == null) return;
        if (skin.id == defaultId) return; // cannot unequip default
        _equippedId = defaultId;
    }
}

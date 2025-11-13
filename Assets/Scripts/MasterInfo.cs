using UnityEngine;
using UnityEngine.UI;

public class MasterInfo : MonoBehaviour
{
    // --- Currency & scoring ---
    public static int CrystalCount = 0;
    private static int _distanceScore;

    // --- UI refs (assign in Inspector) ---
    [SerializeField] private Text crystalDisplay;
    [SerializeField] private Text scoreDisplay;

    // --- Player & scoring settings ---
    [SerializeField] private Transform player;
    [SerializeField] private float pointsPerMeter = 1f;

    // --- Internals ---
    private float _startZ;

    private void Start()
    {
        if (!player) AutoFindPlayer();
        if (player) _startZ = player.position.z; // assumes forward is +Z
        UpdateUI();
    }

    private void Update()
    {
        if (!player)
        {
            AutoFindPlayer();              // try to recover if player was spawned late
            if (!player) return;
        }

        var distance = Mathf.Max(0f, player.position.z - _startZ);
        var newScore = Mathf.FloorToInt(distance * pointsPerMeter);

        if (newScore == _distanceScore) return;
        _distanceScore = newScore;
        UpdateUI();
    }

    private void AutoFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (!go) return;

        player = go.transform;
        _startZ = player.position.z; // re-baseline when we discover the player
    }

    // --- UI ---
    public void UpdateUI()
    {
        if (crystalDisplay) crystalDisplay.text = "" + CrystalCount;
        if (scoreDisplay)   scoreDisplay.text   = "DISTANCE: " + _distanceScore;
    }

    // --- Currency helpers ---
    /// <summary>Try to spend crystals. Returns true on success. Refreshes UI if present.</summary>
    public static bool TrySpendCrystals(int amount)
    {
        if (amount <= 0) return true;
        if (CrystalCount < amount) return false;

        CrystalCount -= amount;
        if (CrystalCount < 0) CrystalCount = 0; // safety clamp
        ForceRefreshUI();
        return true;
    }

    /// <summary>Add crystals and refresh UI.</summary>
    public static void AddCrystals(int amount)
    {
        if (amount <= 0) return;
        CrystalCount += amount;
        ForceRefreshUI();
    }

    /// <summary>Find a MasterInfo in scene and call UpdateUI (safe if none exists).</summary>
    public static void ForceRefreshUI()
    {
        var mi = FindObjectOfType<MasterInfo>();
        if (mi) mi.UpdateUI();
    }
}

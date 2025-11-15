using System.Collections.Generic;
using Features.UI;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [SerializeField] private List<Skin> allSkins;
    [SerializeField] private Skin defaultSkin;

    public IReadOnlyList<Skin> AllSkins => allSkins;
    public Skin CurrentSkin { get; private set; }

    public System.Action<Skin> OnSkinChanged;
    public System.Action<Skin> OnOwnershipChanged;

    const string OwnedKey = "SKIN_OWNED";
    const string CurrentKey = "SKIN_CURRENT";

    void Awake()
    {
        // Load currently equipped skin id (may be empty on first run)
        var curId = PlayerPrefs.GetString(CurrentKey, string.Empty);
        CurrentSkin = allSkins.Find(s => s.id == curId);

        // Do NOT auto-own anything on first run.
        // Ownership is only set by TryPurchase(...) or your editor tools.

        // If the saved current skin is missing or no longer owned, clear it.
        if (CurrentSkin != null && !IsOwned(CurrentSkin))
        {
            CurrentSkin = null;
            PlayerPrefs.DeleteKey(CurrentKey);
            PlayerPrefs.Save();
        }

        // Optional: log what’s owned (for sanity)
#if UNITY_EDITOR
        foreach (var s in allSkins)
            Debug.Log($"[SkinManager] Owned[{s.displayName}] = {IsOwned(s)}");
#endif
    }

    public bool IsOwned(Skin s) => PlayerPrefs.GetInt(OwnedKey + s.id, 0) == 1;

    void SetOwned(Skin s, bool owned)
    {
        PlayerPrefs.SetInt(OwnedKey + s.id, owned ? 1 : 0);
        PlayerPrefs.Save();
        OnOwnershipChanged?.Invoke(s);
    }

    public bool TryPurchase(Skin s)
    {
        if (s == null) return false;
        if (IsOwned(s)) return true;

        var price = Mathf.Max(0, s.price);
        if (!CrystalWallet.CanAfford(price))
            return false;

        if (!CrystalWallet.Spend(price))
            return false;

        SetOwned(s, true);
        OnOwnershipChanged?.Invoke(s);
        return true;
    }

    public void SetCurrentSkin(Skin s)
    {
        if (!IsOwned(s)) return;
        CurrentSkin = s;
        PlayerPrefs.SetString(CurrentKey, s.id);
        PlayerPrefs.Save();
        OnSkinChanged?.Invoke(s);
    }

    public List<Skin> GetOwnedSkins()
    {
        var list = new List<Skin>();
        foreach (var s in allSkins) if (IsOwned(s)) list.Add(s);
        return list;
    }
    
    [ContextMenu("Reset Ownership (Own None)")]
    public void ResetOwnershipOwnNone()
    {
        foreach (var s in allSkins)
            PlayerPrefs.DeleteKey(OwnedKey + s.id);
        PlayerPrefs.DeleteKey(CurrentKey);
        PlayerPrefs.Save();
        CurrentSkin = null;
        OnSkinChanged?.Invoke(null);
        Debug.Log("[SkinManager] Ownership reset: none owned, nothing equipped.");
    }
}
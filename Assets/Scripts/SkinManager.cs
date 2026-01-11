using System.Collections.Generic;
using Features.UI;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [SerializeField] public List<Skin> allSkins;
    [SerializeField] private Skin defaultSkin;

    public IReadOnlyList<Skin> AllSkins => allSkins;
    public Skin CurrentSkin { get; private set; }

    public System.Action<Skin> OnSkinChanged;
    public System.Action<Skin> OnOwnershipChanged;

    public const string OwnedKey = "SKIN_OWNED";
    public const string CurrentKey = "SKIN_CURRENT";

    void Awake()
    {
        var curId = PlayerPrefs.GetString(CurrentKey, string.Empty);
        CurrentSkin = allSkins.Find(s => s.id == curId);

        if (CurrentSkin != null && !IsOwned(CurrentSkin))
        {
            CurrentSkin = null;
            PlayerPrefs.DeleteKey(CurrentKey);
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR
        foreach (var s in allSkins)
            Debug.Log($"[SkinManager] Owned[{s.displayName}] = {IsOwned(s)}");
#endif
    }

    public bool IsOwned(Skin s) => PlayerPrefs.GetInt(OwnedKey + s.id, 0) == 1;

    public void SetOwned(Skin s, bool owned)
    {
        if (s == null) return;
        PlayerPrefs.SetInt(OwnedKey + s.id, owned ? 1 : 0);
        PlayerPrefs.Save();
        OnOwnershipChanged?.Invoke(s);
    }

    public void ClearCurrentSkin()
    {
        CurrentSkin = null;
        PlayerPrefs.DeleteKey(CurrentKey);
        PlayerPrefs.Save();
        OnSkinChanged?.Invoke(null);
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
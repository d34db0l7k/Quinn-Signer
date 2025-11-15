using UnityEngine;
using UnityEngine.UI;
using Features.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Data")]
    public Skin skin;

    [Header("UI")]
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject equippedBadge;
    [SerializeField] private RawImage icon; // used by ShopItemPreview

    private SkinManager _manager;

    void OnEnable()
    {
        CrystalWallet.OnChanged += HandleWalletChanged;
    }

    void OnDisable()
    {
        CrystalWallet.OnChanged -= HandleWalletChanged;
    }

    void HandleWalletChanged(int _)
    {
        Refresh(); // re-check affordability & states
    }
    
    public void Bind(Skin s, SkinManager manager)
    {
        skin = s;
        _manager = manager;

        if (nameText) nameText.text = s.displayName;
        if (priceText) priceText.text = s.price.ToString() + " Shards";

        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
        }
        if (selectButton)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(Select);
        }
        
        var preview = GetComponent<ShopItemPreview>();
        if (preview)
        {
            // Make sure the RawImage on the card is assigned in the preview inspector.
            preview.RebuildFor(skin);
        }

        Refresh();
        _manager.OnOwnershipChanged += HandleOwnedChanged;
        _manager.OnSkinChanged += HandleSkinChanged;
    }

    void OnDestroy()
    {
        if (_manager != null)
        {
            _manager.OnOwnershipChanged -= HandleOwnedChanged;
            _manager.OnSkinChanged -= HandleSkinChanged;
        }
    }

    void HandleOwnedChanged(Skin s) { if (s == skin) Refresh(); }
    void HandleSkinChanged(Skin s)  { Refresh(); }

    void Refresh()
    {
        if (!_manager || !skin) return;

        bool owned = _manager.IsOwned(skin);
        bool equipped = (_manager.CurrentSkin == skin);
        bool affordable = CrystalWallet.CanAfford(skin.price);

        if (buyButton)
        {
            buyButton.gameObject.SetActive(!owned);
            buyButton.interactable = !owned && affordable; // 🔒 disable when too poor
        }
        if (priceText)
        {
            priceText.gameObject.SetActive(!owned);
            // Optional: tint red if unaffordable
            priceText.color = affordable ? Color.white : new Color(1f, 0.4f, 0.4f);
        }
        if (selectButton) selectButton.gameObject.SetActive(owned && !equipped);
        if (equippedBadge) equippedBadge.SetActive(equipped);
    }

    void Buy()
    {
        if (_manager.TryPurchase(skin))
            Refresh();
    }

    void Select()
    {
        _manager.SetCurrentSkin(skin);
        Refresh();
    }
}

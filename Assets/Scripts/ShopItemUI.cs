using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Data")]
    public Skin skin;                 // assign the Skin asset for THIS item

    [Header("UI")]
    public Button actionButton;       // the button on the item
    public Text actionLabel;          // the Text inside that button

    private string DefaultId => SkinManager.Instance ? SkinManager.Instance.defaultId : "ship_default";

    void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClick);
        }
        Refresh();
    }

    void OnEnable() => Refresh();

    void OnClick()
    {
        var sm = SkinManager.Instance;
        if (sm == null || skin == null) return;

        // Default skin: just equip, never buy
        if (skin.id == DefaultId)
        {
            sm.Equip(skin);
            Refresh();
            return;
        }

        // Not owned → attempt to buy
        if (!sm.IsUnlocked(skin))
        {
            int price = Mathf.Max(0, skin.price);

            // If cannot afford, bail (even if button was somehow clickable)
            if (!MasterInfo.TrySpendCrystals(price))
            {
                // Optional: visual feedback here
                return;
            }

            // Purchase OK → unlock + equip
            sm.Equip(skin);
            Refresh();
            return;
        }

        // Owned
        if (sm.IsEquipped(skin))
        {
            // Unequip → back to default
            sm.Unequip(skin);
            var def = sm.knownSkins.Find(s => s && s.id == DefaultId);
            if (def) sm.Equip(def);
        }
        else
        {
            sm.Equip(skin);
        }

        Refresh();
    }

    void Refresh()
    {
        var sm = SkinManager.Instance;
        if (sm == null || skin == null || actionButton == null || actionLabel == null) return;

        // Default skin UX
        if (skin.id == DefaultId)
        {
            if (sm.IsEquipped(skin))
            {
                actionLabel.text = "EQUIPPED";
                actionButton.interactable = false;
            }
            else
            {
                actionLabel.text = "EQUIP";
                actionButton.interactable = true;
            }
            return;
        }

        // Non-default skins
        if (!sm.IsUnlocked(skin))
        {
            int price = Mathf.Max(0, skin.price);
            actionLabel.text = $"BUY ({price})";
            actionButton.interactable = (MasterInfo.CrystalCount >= price);
        }
        else if (sm.IsEquipped(skin))
        {
            actionLabel.text = "UNEQUIP";
            actionButton.interactable = true;
        }
        else
        {
            actionLabel.text = "EQUIP";
            actionButton.interactable = true;
        }
    }
}

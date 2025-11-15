using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinCarousel : MonoBehaviour
{
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private RawImage icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Button leftBtn, rightBtn, equipBtn;
    [SerializeField] private ShopItemPreview preview; // reuse our preview script (assign targetImage=icon)

    List<Skin> _owned;
    int _index;

    void Start()
    {
        _owned = skinManager.GetOwnedSkins();
        if (_owned.Count == 0) { gameObject.SetActive(false); return; }

        leftBtn.onClick.AddListener(() => { _index = (_index - 1 + _owned.Count) % _owned.Count; UpdateUI(); });
        rightBtn.onClick.AddListener(() => { _index = (_index + 1) % _owned.Count; UpdateUI(); });
        equipBtn.onClick.AddListener(() => { skinManager.SetCurrentSkin(_owned[_index]); UpdateUI(); });

        // Jump to current skin if owned
        var cur = skinManager.CurrentSkin;
        var idx = _owned.IndexOf(cur);
        _index = Mathf.Max(0, idx);
        UpdateUI();
    }

    void UpdateUI()
    {
        var s = _owned[_index];
        if (nameText) nameText.text = s.displayName;

        if (preview)
        {
            preview.enabled = false; // force rebuild
            preview.enabled = true;
            // preview reads ShopItemUI.skin OR we set it directly:
            var ui = preview.GetComponent<ShopItemUI>();
            if (ui == null) ui = preview.gameObject.AddComponent<ShopItemUI>();
            ui.Bind(s, skinManager);
        }

        // Show “Equipped” state by disabling Equip when already current (optional)
        if (equipBtn) equipBtn.interactable = (skinManager.CurrentSkin != s);
    }
}
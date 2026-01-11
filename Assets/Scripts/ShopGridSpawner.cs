using System.Collections.Generic;
using UnityEngine;

public class ShopGridSpawner : MonoBehaviour
{
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private GameObject shopCardPrefab;
    [SerializeField] private Transform contentParent;

    void Start()
    {
        if (!skinManager || !shopCardPrefab || !contentParent) { Debug.LogError("[ShopGridSpawner] Missing refs."); return; }

        foreach (var skin in skinManager.AllSkins)
        {
            var go = Instantiate(shopCardPrefab, contentParent, false);
            var ui = go.GetComponent<ShopItemUI>();
            if (ui) ui.Bind(skin, skinManager);
        }
        
        var rt = contentParent as RectTransform;
        if (rt)
        {
            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }
    }
}
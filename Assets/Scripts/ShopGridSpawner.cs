using System.Collections.Generic;
using UnityEngine;

public class ShopGridSpawner : MonoBehaviour
{
    [SerializeField] private SkinManager skinManager; // your existing manager
    [SerializeField] private GameObject shopCardPrefab; // the prefab you just built
    [SerializeField] private Transform contentParent; // ScrollView/Content

    void Start()
    {
        if (!skinManager || !shopCardPrefab || !contentParent) { Debug.LogError("[ShopGridSpawner] Missing refs."); return; }

        foreach (var skin in skinManager.AllSkins) // ensure SkinManager exposes this list
        {
            var go = Instantiate(shopCardPrefab, contentParent, false);
            var ui = go.GetComponent<ShopItemUI>();
            if (ui) ui.Bind(skin, skinManager);
        }
        
        // At the end of Start(), after looping through skins:
        var rt = contentParent as RectTransform;
        if (rt)
        {
            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }
    }
}
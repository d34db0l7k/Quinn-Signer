namespace Features
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerSkinApplier : MonoBehaviour
    {
        [Header("Skin Source")]
        [SerializeField] private SkinManager skinManager;

        [Header("Renderers to Re-skin")]
        [Tooltip("All MeshRenderer / SkinnedMeshRenderer components whose materials should be replaced.")]
        [SerializeField] private List<Renderer> targets = new List<Renderer>();

        [Header("Replace Mode")]
        [Tooltip("Replace every material slot with the skin material.")]
        [SerializeField] private bool replaceAllSlots = true;

        [Tooltip("If not replacing all, only this index will be replaced.")]
        [SerializeField] private int slotIndex = 0;

        [Header("Fallback")]
        [SerializeField] private Material fallbackMaterial;

        void OnEnable()
        {
            if (skinManager) skinManager.OnSkinChanged += HandleSkinChanged;
            Apply();
        }

        void OnDisable()
        {
            if (skinManager) skinManager.OnSkinChanged -= HandleSkinChanged;
        }

        [ContextMenu("Apply Now")]
        public void Apply()
        {
            var mat = skinManager && skinManager.CurrentSkin && skinManager.CurrentSkin.skinMaterial
                ? skinManager.CurrentSkin.skinMaterial
                : fallbackMaterial;

            if (!mat) return;

            foreach (var r in targets)
            {
                if (!r) continue;

                // Use .materials to get an instanced array we can modify per-object.
                var mats = r.materials;

                if (replaceAllSlots)
                {
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                }
                else
                {
                    if (slotIndex >= 0 && slotIndex < mats.Length) mats[slotIndex] = mat;
                }

                r.materials = mats;
            }
        }

        private void HandleSkinChanged(Skin _)
        {
            Apply();
        }
    }
}
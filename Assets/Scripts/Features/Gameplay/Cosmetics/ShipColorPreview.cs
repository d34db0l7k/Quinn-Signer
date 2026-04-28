namespace Features.Gameplay.Cosmetics
{
    using UnityEngine;

    public class ShipColorPreview : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;

        private void Start()
        {
            ApplyCurrentColor();
        }

        public void ApplyCurrentColor()
        {
            if (CosmeticManager.Instance == null || targetRenderer == null) return;

            targetRenderer.material.color = CosmeticManager.Instance.currentShipColor;
            Debug.Log("here is the current ship color -> " + CosmeticManager.Instance.currentShipColor);
        }
    }
}
namespace Features.Gameplay.Cosmetics
{
    using UnityEngine;

    public class CosmeticManager : MonoBehaviour
    {
        public static CosmeticManager Instance;

        public Color currentShipColor = Color.white;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetShipColor(Color newColor)
        {
            currentShipColor = newColor;
            
            ShipColorPreview preview = FindFirstObjectByType<ShipColorPreview>();
            if (preview != null)
            {
                preview.ApplyCurrentColor();
            }
        }

        public void SetShipColorWhite()
        {
            SetShipColor(Color.white);
        }

        public void SetShipColorRed()
        {
            SetShipColor(Color.red);
        }

        public void SetShipColorBlue()
        {
            SetShipColor(Color.blue);
        }

        public void SetShipColorGreen()
        {
            SetShipColor(Color.green);
        }

        public void SetShipColorYellow()
        {
            SetShipColor(Color.yellow);
        }
    }
}
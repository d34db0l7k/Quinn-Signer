using UnityEngine;
using UnityEngine.UI;

public class CosmeticsButton : MonoBehaviour
{
    public Color buttonColor; 
    public Renderer garageModelRenderer;

    public void OnButtonClick()
    {
        CosmeticManager.Instance.currentShipColor = buttonColor;

        if (garageModelRenderer != null)
        {
            garageModelRenderer.material.color = buttonColor;
        }
    }
}
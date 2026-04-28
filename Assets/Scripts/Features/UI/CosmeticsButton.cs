using UnityEngine;
using UnityEngine.UI;

public class CosmeticsButton : MonoBehaviour
{
    public Color buttonColor; 
    public Renderer garageModelRenderer;

    public void OnButtonClick()
    {
        if (garageModelRenderer != null)
        {
            garageModelRenderer.material.color = buttonColor;
        }
    }
}
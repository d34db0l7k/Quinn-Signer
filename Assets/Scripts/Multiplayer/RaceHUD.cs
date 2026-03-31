using TMPro;
using UnityEngine;

namespace Multiplayer
{
    public class RaceHUD : MonoBehaviour
    {
        public TMP_Text promptText;

        public void SetPrompt(int prompt)
        {
            if (promptText == null) return;

            promptText.text = prompt switch
            {
                0 => "↑",
                1 => "↓",
                2 => "←",
                3 => "→",
                _ => "?"
            };
        }
    }
}
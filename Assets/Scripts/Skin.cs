using UnityEngine;

[CreateAssetMenu(menuName = "Cosmetics/Skin")]
public class Skin : ScriptableObject
{
    public string id;                 // e.g. "ship_default", "ship_red"
    public string displayName;        // e.g. "Original", "Red Ship"
    public Sprite shopIcon;           // shop card image (optional to use)
    public GameObject runnerPrefab;   // the PLAYER prefab to spawn in the runner scene

    public int price = 0;             // <-- NEW: cost in crystals
}

using UnityEngine;

[CreateAssetMenu(menuName = "Cosmetics/Skin")]
public class Skin : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public int price = 0;
    
    [Header("Appearance")]
    public Material skinMaterial;
    public GameObject runnerPrefab;
}

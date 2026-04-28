using UnityEngine;

public class CosmeticManager : MonoBehaviour
{
    public static CosmeticManager Instance;
    public Color currentShipColor = Color.white;

    void Awake()
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
}
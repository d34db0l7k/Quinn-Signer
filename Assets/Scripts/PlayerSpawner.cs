using UnityEngine;

[DefaultExecutionOrder(-50)] // run before most Start()
public class PlayerSpawner : MonoBehaviour
{
    public GameObject defaultPlayerPrefab; // assign Original ship prefab here

    void Start()
    {
        // pick prefab: equipped or default
        var sm = SkinManager.Instance;
        var prefab = defaultPlayerPrefab;
        if (sm && sm.EquippedSkin && sm.EquippedSkin.runnerPrefab)
            prefab = sm.EquippedSkin.runnerPrefab;

        if (!prefab) { Debug.LogError("PlayerSpawner: no prefab assigned."); return; }

        var player = Instantiate(prefab, transform.position, transform.rotation);

        // make sure tag is set (helps auto-finders)
        if (player.tag != "Player") player.tag = "Player";
    }
}

using UnityEngine;
using Features.CameraManagement;
using System;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Skin / Prefab")]
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private GameObject fallbackPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("World-space offset applied to the spawned ship.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool dontDestroyOnLoad = false;

    [Header("Tag/Layer")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int playerLayer = 0;

    [Header("Optional Camera Hook")]
    [SerializeField] private bool attachCameraFollow = true;
    [Tooltip("Temporarily disable CameraFollow during spawn to avoid a one-frame look-down flicker.")]
    [SerializeField] private bool silenceCameraDuringSpawn = true;
    [Tooltip("After setting the new target, try to snap the camera on the next frame.")]
    [SerializeField] private bool snapCameraOnAssign = true;

    private static GameObject s_currentPlayer;
    private static PlayerSpawner s_instance;
    private Action<Skin> _onSkinChangedHandler;

    void Awake()
    {
        if (dontDestroyOnLoad)
        {
            if (s_instance != null && s_instance != this) { Destroy(gameObject); return; }
            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (spawnOnStart) EnsureSpawned();

        if (skinManager != null)
        {
            _onSkinChangedHandler = _ => Respawn();
            skinManager.OnSkinChanged += _onSkinChangedHandler;
        }
    }

    void OnDestroy()
    {
        if (skinManager != null && _onSkinChangedHandler != null)
            skinManager.OnSkinChanged -= _onSkinChangedHandler;

        if (s_instance == this) s_instance = null;
    }

    public void EnsureSpawned()
    {
        if (s_currentPlayer == null) Spawn();
        else
        {
            var p = spawnPoint ? spawnPoint : transform;
            s_currentPlayer.transform.SetPositionAndRotation(p.position + spawnOffset, p.rotation);
        }
    }

    public void Respawn()
    {
        if (s_currentPlayer) Destroy(s_currentPlayer);
        Spawn();
    }

    private void Spawn()
    {
        var prefab = (skinManager && skinManager.CurrentSkin && skinManager.CurrentSkin.runnerPrefab)
            ? skinManager.CurrentSkin.runnerPrefab
            : fallbackPrefab;

        var p = spawnPoint ? spawnPoint : transform;

        CameraFollow camFollow = null;
        if (attachCameraFollow)
        {
            camFollow = FindFirstObjectByType<CameraFollow>(FindObjectsInactive.Exclude);
            if (camFollow && silenceCameraDuringSpawn) camFollow.enabled = false;
        }

        s_currentPlayer = Instantiate(prefab, p.position + spawnOffset, p.rotation);

        if (!string.IsNullOrEmpty(playerTag)) s_currentPlayer.tag = playerTag;
        if (playerLayer >= 0) SetLayerRecursive(s_currentPlayer, playerLayer);

        var anchor = s_currentPlayer.GetComponentInChildren<CameraAnchor>(true);
        Transform camTarget = (anchor != null) ? anchor.transform : s_currentPlayer.transform;

        if (camFollow)
        {
            var t = camFollow.GetType();
            var m = t.GetMethod("SetTarget", new Type[] { typeof(Transform), typeof(bool) })
                     ?? t.GetMethod("SetTarget", new Type[] { typeof(Transform) });

            if (m != null)
            {
                if (m.GetParameters().Length == 2) m.Invoke(camFollow, new object[] { camTarget, false });
                else m.Invoke(camFollow, new object[] { camTarget });
            }
            else
            {
                var f = t.GetField("target");
                if (f != null) f.SetValue(camFollow, camTarget);
                else
                {
                    var prop = t.GetProperty("target");
                    if (prop != null && prop.CanWrite) prop.SetValue(camFollow, camTarget);
                }
            }

            if (snapCameraOnAssign) StartCoroutine(SnapCameraNextFrame(camFollow));
            else if (silenceCameraDuringSpawn) camFollow.enabled = true;
        }
    }

    private System.Collections.IEnumerator SnapCameraNextFrame(CameraFollow camFollow)
    {
        yield return null; // wait one frame so player/rig initialize
        if (!camFollow) yield break;

        var t = camFollow.GetType();
        var m = t.GetMethod("SnapToTarget") ?? t.GetMethod("SnapNow") ?? t.GetMethod("SyncImmediate");
        if (m != null)
        {
            try { m.Invoke(camFollow, null); } catch { /* ignore */ }
        }
        camFollow.enabled = true;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        if (!go) return;
        go.layer = layer;
        foreach (Transform t in go.transform) SetLayerRecursive(t.gameObject, layer);
    }
}

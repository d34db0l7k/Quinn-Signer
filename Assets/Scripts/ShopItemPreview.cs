using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ShopItemPreview : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Skin skin;

    [Header("UI")]
    [SerializeField] private RawImage targetImage;

    [Header("Skin Material Override (Preview Only)")]
    [SerializeField] private bool replaceAllSlots = true;
    [SerializeField] private int slotIndex = 0;
    [SerializeField] private Material fallbackMaterial;

    [Header("Preview")]
    [SerializeField] private Vector3 modelOffset = Vector3.zero;
    [SerializeField] private float modelScale = 1f;
    [SerializeField] private float camDistance = 6f;
    [SerializeField] private float camFov = 28f;
    [SerializeField] private Color  clearColor = new Color(0,0,0,0);
    [SerializeField] private Vector3 lightDir = new Vector3(0.3f, 0.8f, -0.6f);
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private float rotateSpeed = 25f;

    [Header("RT (leave null for auto)")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private int rtSize = 512;

    const string PreviewLayerName = "ShopPreview";
    int _previewLayer = -1;
    UnityEngine.Camera _cam;
    Transform _stage;
    GameObject _spawned;
    Light _light;

    void Reset() { TryAutoWire(); }
    void OnValidate() { TryAutoWire(); }
    
    void TryAutoWire()
    {
        if (!skin)
        {
            var ui = GetComponent<ShopItemUI>();
            if (ui) skin = ui.skin;
        }
        if (!targetImage)
            targetImage = GetComponentInChildren<RawImage>(true);
    }

    void OnEnable()
    {
        Build();
    }

    void OnDisable()
    {
        CleanupSpawn();
        if (_cam) { _cam.targetTexture = null; DestroyImmediate(_cam.gameObject); _cam = null; }
        if (_light) { DestroyImmediate(_light.gameObject); _light = null; }
        if (renderTexture) { targetImage.texture = null; /* keep shared RT if user assigned */ }
    }

    void Update()
    {
        if (autoRotate && _stage) _stage.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    void Build()
    {
        // Layer setup
        _previewLayer = LayerMask.NameToLayer(PreviewLayerName);

        // Stage
        if (!_stage)
        {
            var stageGO = new GameObject("PreviewStage");
            stageGO.transform.SetParent(transform, false);
            _stage = stageGO.transform;
            _stage.gameObject.layer = _previewLayer;
        }

        // Spawn model
        CleanupSpawn();
        if (!skin || !skin.runnerPrefab)
        {
            Debug.LogWarning("[ShopItemPreview] Missing Skin or runnerPrefab.");
            return;
        }
        _spawned = Instantiate(skin.runnerPrefab, _stage);
        _spawned.transform.localPosition = modelOffset;
        _spawned.transform.localRotation = Quaternion.identity;
        _spawned.transform.localScale    = Vector3.one * modelScale;
        SetLayerRecursive(_spawned, _previewLayer);
        FreezeForPreview(_spawned);
        
        var mat = (skin && skin.skinMaterial) ? skin.skinMaterial : fallbackMaterial;
        ApplySkinMaterialRecursive(_spawned, mat);
        
        // Camera
        if (!_cam)
        {
            var camGO = new GameObject("PreviewCamera");
            camGO.transform.SetParent(transform, false);
            _cam = camGO.AddComponent<UnityEngine.Camera>();
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = clearColor;
            _cam.allowHDR = true;
            _cam.cullingMask = 1 << _previewLayer;
            _cam.enabled = true;
        }
        // Frame the model
        _cam.fieldOfView = camFov;
        var center = _stage.position + modelOffset;
        _cam.transform.position = center + new Vector3(0, 0, -camDistance);
        _cam.transform.LookAt(center);

        // Light (simple key light)
        if (!_light)
        {
            var lgo = new GameObject("PreviewLight");
            lgo.transform.SetParent(transform, false);
            _light = lgo.AddComponent<Light>();
            _light.type = LightType.Directional;
            _light.intensity = 1.2f;
            _light.color = Color.white;
        }
        _light.transform.rotation = Quaternion.LookRotation(-lightDir.normalized);

        // RenderTexture hookup
        if (!renderTexture)
        {
            renderTexture = new RenderTexture(rtSize, rtSize, 16, RenderTextureFormat.ARGB32);
            renderTexture.name = $"ShopRT_{name}";
            renderTexture.Create();
        }
        _cam.targetTexture = renderTexture;
        if (targetImage) targetImage.texture = renderTexture;
    }

    void CleanupSpawn()
    {
        if (_spawned)
        {
            DestroyImmediate(_spawned);
            _spawned = null;
        }
    }
    
    public void RebuildFor(Skin s)
    {
        skin = s;
        CleanupSpawn();
        if (_cam) { _cam.targetTexture = null; DestroyImmediate(_cam.gameObject); _cam = null; }
        if (_light) { DestroyImmediate(_light.gameObject); _light = null; }
        Build();
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        if (!go) return;
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursive(t.gameObject, layer);
    }
    
    // Disable movement/AI/physics so previews don't run away
    void FreezeForPreview(GameObject root)
    {
        if (!root) return;
        string[] typesToDisable =
        {
            "InfinitePlayerMovement",
            "PlayerMovement",
            "EnemyController",
            "EnemyLock",
            "SpaceshipCollision",
            "CameraFollow"
        };
        DisableBehavioursByName(root, typesToDisable);

        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.Sleep();
        }

        foreach (var col in root.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        foreach (var au in root.GetComponentsInChildren<AudioSource>(true))
            au.mute = true;
    }

    void DisableBehavioursByName(GameObject root, string[] typeNames)
    {
        if (!root || typeNames == null || typeNames.Length == 0) return;

        var all = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < all.Length; i++)
        {
            var mb = all[i];
            if (!mb) continue;
            var t = mb.GetType();
            for (int j = 0; j < typeNames.Length; j++)
            {
                if (t.Name == typeNames[j])
                {
                    mb.enabled = false;
                    break;
                }
            }
        }
    }
    
    void ApplySkinMaterialRecursive(GameObject root, Material mat)
    {
        if (!root || !mat) return;

        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (!r) continue;
            var mats = r.materials;
            if (replaceAllSlots)
            {
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            }
            else
            {
                if (slotIndex >= 0 && slotIndex < mats.Length) mats[slotIndex] = mat;
            }
            r.materials = mats;
        }
    }
}

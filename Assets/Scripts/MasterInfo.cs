using Features.UI;
using UnityEngine;
using UnityEngine.UI;
using Features.Gameplay.Entities.Player;

public class MasterInfo : MonoBehaviour
{
    [SerializeField] private Text crystalDisplay;
    [SerializeField] private Text scoreDisplay;
    
    [SerializeField] private Transform player;
    [Tooltip("Points awarded per meter traveled (or per meter-equivalent if time-based).")]
    [SerializeField] private float pointsPerMeter = 1f;
    
    private float _startZ;
    private int   _distanceScore;
    private bool  _useZDistance = true;
    private float _timeBasedMeters; // integrates forward speed * dt

    void OnEnable()
    {
        CrystalWallet.OnChanged += OnWalletChanged;
    }

    void OnDisable()
    {
        CrystalWallet.OnChanged -= OnWalletChanged;
    }

    void Start()
    {
        if (!player) AutoFindPlayer();
        if (player) _startZ = player.position.z;
        
        UpdateUI(force: true);
    }

    void Update()
    {
        if (!player)
        {
            AutoFindPlayer();
            if (!player) return;
            _startZ = player.position.z;
        }

        float meters = Mathf.Max(0f, player.position.z - _startZ);

        if (meters < 0.01f)
        {
            float speed = 0f;
            var mover = player.GetComponent<InfinitePlayerMovement>();
            if (mover) speed = Mathf.Max(0f, mover.forwardSpeed);
            if (speed > 0f)
            {
                _useZDistance = false;
                _timeBasedMeters += speed * Time.deltaTime;
            }
        }
        else
        {
            _useZDistance = true;
            _timeBasedMeters = 0f; // reset if we switched back
        }

        float effectiveMeters = _useZDistance ? meters : _timeBasedMeters;
        int newScore = Mathf.FloorToInt(effectiveMeters * pointsPerMeter);
        if (newScore != _distanceScore)
        {
            _distanceScore = newScore;
            UpdateUI();
        }
    }

    private void AutoFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (!go) return;
        player = go.transform;
        _startZ = player.position.z;
    }

    private void OnWalletChanged(int _)
    {
        UpdateUI(force: true);
    }

    // --- UI ---
    private void UpdateUI(bool force = false)
    {
        if (crystalDisplay)
            crystalDisplay.text = CrystalWallet.Load().ToString(); // single source of truth

        if (scoreDisplay)
            scoreDisplay.text = "DISTANCE: " + _distanceScore;
    }
    
    public static bool TrySpendCrystals(int amount) => CrystalWallet.Spend(amount);
    public static void  AddCrystals(int amount)     => CrystalWallet.Add(amount);
    
    public static void ForceRefreshUI()
    {
        var list = FindObjectsOfType<MasterInfo>();
        foreach (var mi in list) mi.UpdateUI(force: true);
    }
}

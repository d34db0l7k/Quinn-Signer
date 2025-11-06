using UnityEngine;
using UnityEngine.UI;

public class MasterInfo : MonoBehaviour
{
    public static int CrystalCount = 0;
    private static int _distanceScore;

    [SerializeField]
        private Text crystalDisplay;
    [SerializeField]
        private Text scoreDisplay;
    [SerializeField]
        private Transform player;
    [SerializeField]
        private float pointsPerMeter = 1f;

    private float _startZ;

    private void Start()
    {
        if (player)
            _startZ = player.position.z; // assumes forward is +Z

        UpdateUI();
    }

    private void Update()
    {
        if (!player) return;

        // calculate distance since start
        var distance = Mathf.Max(0f, player.position.z - _startZ);
        var newScore = Mathf.FloorToInt(distance * pointsPerMeter);

        if (newScore == _distanceScore) return;
        _distanceScore = newScore;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (crystalDisplay)
            crystalDisplay.text = "CRYSTALS: " + CrystalCount;

        if (scoreDisplay)
            scoreDisplay.text = "DISTANCE: " + _distanceScore;
    }
}

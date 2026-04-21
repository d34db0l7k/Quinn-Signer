using UnityEngine;
using System.Collections;
using Features.Gameplay.Entities.Player;
using Features.Gameplay.Entities.Enemy;

public class EnemyControllerMissionZero : MonoBehaviour
{
    [HideInInspector] public SegmentManagerMissionZero segmentManager;
    [HideInInspector] public float lockLeadZ = 30f;
    [HideInInspector] public GameObject signingOverlay;

    [Header("Settings")]
    public bool isBoss = false;
    public float approachSpeed = 50f;

    [Header("Fly In")]
    public float flyInHeight = 20f;
    public float flyInDuration = 1.5f;

    private Transform _player;
    private bool _flyInComplete = false;
    private Vector3 _flyInStartPos;
    private Vector3 _flyInTargetPos;

    void Start()
    {
        var mover = FindFirstObjectByType<InfinitePlayerMovement>();
        if (mover)
        {
            _player = mover.transform;
            approachSpeed = 1.25f * mover.forwardSpeed;
        }

        _flyInStartPos = transform.position + Vector3.up * flyInHeight;
        _flyInTargetPos = transform.position;
        transform.position = _flyInStartPos;

        StartCoroutine(FlyIn());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TriggerDestroy();
            return;
        }

        if (!_flyInComplete || !_player) return;

        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y,
            _player.position.z + lockLeadZ
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            approachSpeed * Time.deltaTime
        );
    }

    private void TriggerDestroy()
    {
        var controller = GetComponent<Features.Gameplay.Entities.Enemy.EnemyController>();
        if (controller != null)
            controller.Explode();
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (signingOverlay != null)
            signingOverlay.SetActive(false);

        if (!isBoss && segmentManager != null)
            segmentManager.OnEnemyDestroyed();
    }

    private IEnumerator FlyIn()
    {
        float elapsed = 0f;
        while (elapsed < flyInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyInDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.Lerp(_flyInStartPos, _flyInTargetPos, ease);
            yield return null;
        }

        transform.position = _flyInTargetPos;
        _flyInComplete = true;

        if (signingOverlay != null)
            signingOverlay.SetActive(true);

        // Refresh word assignment for this enemy
        var signer = FindFirstObjectByType<Features.Signing.Signer>();
        if (signer != null) signer.RefreshEnemyLabels();
    }
}
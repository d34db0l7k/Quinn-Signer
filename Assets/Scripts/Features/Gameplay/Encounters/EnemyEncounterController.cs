namespace Features.Gameplay.Encounters
{
    using UnityEngine;
    using System.Collections;
    using Features.Gameplay.WorldGen;
    using Features.Signing;
    using Features.Gameplay.Entities.Player;   // for InfinitePlayerMovement
    using Features.Gameplay.Entities.Enemy;

    public class EnemyEncounterController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private SegmentGenerator segmentGen;
        [SerializeField] private SessionSelection sessionSelection;
        [SerializeField] private GameObject enemyPrefab;

        [Header("Timing")]
        [SerializeField] private float encounterIntervalSec = 15f; // time between encounters
        [SerializeField] private float preSafeLeadSec = 3f;        // desired safe runway before spawn
        [SerializeField] private float lifeDuration = 10f;

        [Header("Spawn placement (relative to PLAYER world +Z)")]
        [SerializeField] private float spawnAheadZ = 60f;
        [SerializeField] private Vector2 lateralX = new(-6f, 6f);
        [SerializeField] private Vector2 lateralY = new(-2f, 3f);

        [Header("Lock distance (ahead of player)")]
        [SerializeField] private float lockLeadZ = 30f;

        private GameObject _currentEnemy;
        private float _timer;
        private bool _safeArmed;

        private InfinitePlayerMovement _mover;

        void Reset()
        {
            if (!player)
            {
                var m = FindFirstObjectByType<InfinitePlayerMovement>();
                if (m) player = m.transform;
            }
            if (!mainCamera) mainCamera = Camera.main;
            if (!segmentGen) segmentGen = FindFirstObjectByType<SegmentGenerator>();
        }

        void Start()
        {
            if (!player)
            {
                var m = FindFirstObjectByType<InfinitePlayerMovement>();
                if (m) player = m.transform;
            }
            _mover = player ? player.GetComponent<InfinitePlayerMovement>() : null;

            if (!mainCamera) mainCamera = Camera.main;
            if (!segmentGen) segmentGen = FindFirstObjectByType<SegmentGenerator>();
            _timer = encounterIntervalSec;
            _safeArmed = false;
        }

        void Update()
        {
            // If an enemy is alive: pause the timer and keep safe mode ON
            if (_currentEnemy)
            {
                if (segmentGen) segmentGen.SetForceSafeSegments(true);
                return;
            }

            // count down toward next encounter
            _timer -= Time.deltaTime;

            // Compute how early we must arm safe mode so the prewarmed buffer also becomes safe
            float speed = (_mover && _mover.forwardSpeed > 0f) ? _mover.forwardSpeed : segmentGen.segmentLength; // fallback ~1 seg/s
            float prewarmedMeters = segmentGen.segmentsAhead * segmentGen.segmentLength;
            float bufferSeconds = prewarmedMeters / Mathf.Max(0.01f, speed);
            float armThreshold = preSafeLeadSec + bufferSeconds;

            // Arm safe mode early enough to cover the queued path
            if (!_safeArmed && _timer <= armThreshold)
            {
                _safeArmed = true;
                if (segmentGen) segmentGen.SetForceSafeSegments(true);
                // Optional: Debug.Log($"[Encounter] Arming safe mode {armThreshold:F1}s before spawn (buffer {bufferSeconds:F1}s).");
            }

            // Time to spawn?
            if (_timer <= 0f)
            {
                SpawnEnemy();
                // Timer stays paused while enemy exists
            }
        }

        void SpawnEnemy()
        {
            if (!enemyPrefab || !player || sessionSelection.words.Count == 0) return;

            var pos = player.position;
            pos.z += spawnAheadZ;
            pos.x += Random.Range(lateralX.x, lateralX.y);
            pos.y += Random.Range(lateralY.x, lateralY.y);

            _currentEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

            // Ensure label always faces camera
            var face = _currentEnemy.GetComponentInChildren<Features.CameraManagement.FaceCamera>();
            if (!face) _currentEnemy.AddComponent<Features.CameraManagement.FaceCamera>();

            // Assign a session word
            var label = _currentEnemy.GetComponentInChildren<EnemyLabel>(true);
            if (label)
            {
                if (sessionSelection && sessionSelection.HasWords && sessionSelection.TryPop(out var word))
                    label.SetWord(word);
                else
                    label.SetWord("sign");
            }
            
            var locker = _currentEnemy.GetComponent<EnemyLeadLock>();
            if (!locker) locker = _currentEnemy.AddComponent<EnemyLeadLock>();
            locker.target = player;
            locker.lockLeadZ = lockLeadZ;
            locker.approachSpeed = 1.25f * (player ? player.GetComponent<Features.Gameplay.Entities.Player.InfinitePlayerMovement>()?.forwardSpeed ?? 40f : 40f);

            StartCoroutine(WaitEnemyDefeatedThenResume());
            StartCoroutine(DespawnFromExpiration());
        }

        IEnumerator WaitEnemyDefeatedThenResume()
        {
            while (_currentEnemy) yield return null;

            // Enemy defeated → resume normal track and timer
            _safeArmed = false;
            if (segmentGen) segmentGen.SetForceSafeSegments(false);
            _timer = encounterIntervalSec;
        }

        IEnumerator DespawnFromExpiration()
        {
            Debug.Log($"Starting {lifeDuration} second countdown towards expiration of enemy.");
            EnemyController controller = _currentEnemy.GetComponent<EnemyController>();
            yield return new WaitForSeconds(lifeDuration);
            controller.Expire();
            _timer = encounterIntervalSec;
            _currentEnemy = null;
            Debug.Log("Successfully expired the enemy.");
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Gameplay.Entities.Player;
using Features.Signing;

public class SegmentManagerMissionZero : MonoBehaviour
{
    [Header("Segments")]
    public GameObject[] mountainSegments;
    public GameObject[] flowerSegments;
    public GameObject[] forestSegments;
    public GameObject[] bossSegments;

    [Header("Enemy")]
    public GameObject enemyPrefab;
    public float firstEnemySpawnDistance = 80f;
    public float enemySpawnDistanceAhead = 30f;
    public float lockLeadZ = 30f;
    public int enemiesPerSection = 3;
    public float enemySpawnDelay = 3f;

    [Header("Boss")]
    public GameObject bossPrefab;
    public float bossSpawnDistanceAhead = 80f;
    public float bossLockLeadZ = 50f;

    [Header("Layout")]
    public float segmentLength = 50f;
    public int segmentsAhead = 6;
    public float deleteBuffer = 100f;

    [Header("References")]
    public Transform player;
    public MissionHUDTracker missionHUD;
    public GameObject missionsSigningOverlay;

    [Header("Session")]
    public SessionSelection missionZeroSession;

    private enum Section { Mountain, Flower, Forest, Boss }
    private Section _currentSection = Section.Mountain;
    private readonly Queue<GameObject> _activeSegments = new Queue<GameObject>();
    private float _nextSpawnZ = 0f;
    private GameObject _currentEnemy;
    private bool _waitingForEnemyDestroy = false;
    private int _enemiesDestroyedInSection = 0;
    private bool _isFirstEnemy = true;
    private bool _bossSpawned = false;

    private void Start()
    {
        if (!player) AutoFindPlayer();
        if (segmentLength <= 0f) segmentLength = 50f;
        if (missionsSigningOverlay != null) missionsSigningOverlay.SetActive(false);

        PrewarmSegments();
        SetSectionWord();
        StartCoroutine(SpawnEnemyWhen(firstEnemySpawnDistance));
    }

    private void Update()
    {
        if (!player) { AutoFindPlayer(); return; }

        EnsureSegmentsAhead();
        CleanupOldSegments();

        if (_waitingForEnemyDestroy && _currentEnemy == null)
        {
            _waitingForEnemyDestroy = false;
            _enemiesDestroyedInSection++;

            if (_enemiesDestroyedInSection >= enemiesPerSection)
            {
                _enemiesDestroyedInSection = 0;
                AdvanceSection();

                if (_currentSection == Section.Boss)
                {
                    StartCoroutine(SpawnBossWhen());
                    return;
                }
            }

            StartCoroutine(SpawnEnemyWhen(enemySpawnDistanceAhead, enemySpawnDelay));
        }
    }

    private void PrewarmSegments()
    {
        _nextSpawnZ = Mathf.Floor((player ? player.position.z : 0f) / segmentLength + 1f) * segmentLength;
        EnsureSegmentsAhead();
    }

    private void EnsureSegmentsAhead()
    {
        var targetFrontZ = player.position.z + segmentsAhead * segmentLength;
        while (_nextSpawnZ < targetFrontZ)
            SpawnNextSegment();
    }

    private void SpawnNextSegment()
    {
        var pool = _currentSection switch
        {
            Section.Mountain => mountainSegments,
            Section.Flower   => flowerSegments,
            Section.Forest   => forestSegments,
            Section.Boss     => bossSegments,
            _                => forestSegments
        };

        if (pool == null || pool.Length == 0) return;

        var seg = Instantiate(
            pool[Random.Range(0, pool.Length)],
            new Vector3(0f, 0f, _nextSpawnZ),
            Quaternion.identity
        );

        _activeSegments.Enqueue(seg);
        _nextSpawnZ += segmentLength;
    }

    private void CleanupOldSegments()
    {
        while (_activeSegments.Count > 0)
        {
            var oldest = _activeSegments.Peek();
            if (!oldest) { _activeSegments.Dequeue(); continue; }
            if (player.position.z - (oldest.transform.position.z + segmentLength) > deleteBuffer)
            {
                _activeSegments.Dequeue();
                Destroy(oldest);
            }
            else break;
        }
    }

    private IEnumerator SpawnEnemyWhen(float distanceAhead, float delay = 0f)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float triggerZ = player.position.z + distanceAhead;
        while (player.position.z < triggerZ)
            yield return null;

        SpawnEnemy(distanceAhead);
    }

    private void SpawnEnemy(float distanceAhead)
    {
        if (enemyPrefab == null || !player) return;

        // Reset the section word before spawning
        SetSectionWord();

        var enemyPos = new Vector3(0f, player.position.y, player.position.z + distanceAhead);
        _currentEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        _isFirstEnemy = false;

        var enemy = _currentEnemy.GetComponent<EnemyControllerMissionZero>();
        if (enemy != null)
        {
            enemy.segmentManager = this;
            enemy.lockLeadZ = lockLeadZ;
            enemy.signingOverlay = missionsSigningOverlay;
            if (missionHUD != null) missionHUD.enemy = enemy;
        }

        _waitingForEnemyDestroy = true;
    }

    private IEnumerator SpawnBossWhen()
    {
        float triggerZ = player.position.z + bossSpawnDistanceAhead;
        while (player.position.z < triggerZ)
            yield return null;

        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || !player || _bossSpawned) return;

        var bossPos = new Vector3(0f, player.position.y, player.position.z + bossSpawnDistanceAhead);
        var boss = Instantiate(bossPrefab, bossPos, Quaternion.identity);
        _bossSpawned = true;

        var enemy = boss.GetComponent<EnemyControllerMissionZero>();
        if (enemy != null)
        {
            enemy.segmentManager = this;
            enemy.lockLeadZ = bossLockLeadZ;
            enemy.isBoss = true;
            enemy.signingOverlay = missionsSigningOverlay;
            if (missionHUD != null) missionHUD.enemy = enemy;
        }

        var bossController = boss.GetComponent<TutorialBossController>();
        if (bossController != null)
            bossController.InitSession(missionZeroSession);

        var bossMovement = boss.GetComponent<TutorialBossMovement>();
        if (bossMovement != null)
            bossMovement.InitMovementRefs(player);

        Debug.Log("[MissionZero] Boss spawned!");
    }

    private void SetSectionWord()
    {
        if (missionZeroSession == null) return;

        if (_currentSection == Section.Boss)
        {
            missionZeroSession.SetWords(new List<string> { "hello", "bye", "thankyou" });
            return;
        }

        string word = _currentSection switch
        {
            Section.Mountain => "hello",
            Section.Flower   => "bye",
            Section.Forest   => "thankyou",
            _                => "hello"
        };

        missionZeroSession.SetWords(new List<string> { word });
        Debug.Log($"[MissionZero] Session word set to: {word}");
    }

    private void AdvanceSection()
    {
        _currentSection = _currentSection switch
        {
            Section.Mountain => Section.Flower,
            Section.Flower   => Section.Forest,
            Section.Forest   => Section.Boss,
            _                => Section.Boss
        };
        Debug.Log($"[MissionZero] Section: {_currentSection}");
        SetSectionWord();
    }

    public void OnEnemyDestroyed() => _currentEnemy = null;

    private void AutoFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) { player = go.transform; return; }
        var mover = FindFirstObjectByType<InfinitePlayerMovement>();
        if (mover) player = mover.transform;
    }
}
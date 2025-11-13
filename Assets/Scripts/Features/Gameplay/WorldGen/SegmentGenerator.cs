using System.Collections.Generic;
using System.Linq;
using Features.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Features.Gameplay.WorldGen
{
    public class SegmentGenerator : MonoBehaviour
    {
        [Header("Segment Prefabs")]
        public GameObject[] segmentPrefabs;

        [Tooltip("Segment prefab with no obstacles, used for the first few seconds")]
        public GameObject safeIntroPrefab;
        [Header("Layout")]
        [Tooltip("Exact world length of each segment along +Z.")]
        public float segmentLength = 50f;

        [Tooltip("How many segments should always be ahead of the player.")]
        public int segmentsAhead = 6;

        [Header("Intro")]
        [Tooltip("Duration of safe/no-obstacle runway at the start")]
        public float safeIntroSeconds = 4f;
        
        [Header("Runtime Control")]
        [Tooltip("When true, only safe (no obstacles) segments will be spawned.")]
        public bool forceSafeSegments = false;
        
        public Transform player;
        public float deleteBuffer = 100f;
        [Tooltip("Parent object that holds all spawned segments. If empty, this.transform will be used.")]
        public Transform segmentsParent;

        // --- Internal state ---
        private readonly Queue<GameObject> _activeSegments = new Queue<GameObject>();
        private float _nextSpawnZ = 0f;
        private bool _initialized = false;
        
        private int _introSegmentsRemaining = 0;
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            InitializeIfNeeded();
        }

        private void Update()
        {
            if (!player)
            {
                // Try to recover player mid-play if it was recreated
                AutoFindPlayer();
                if (!player) return; // still nothing; skip this frame
            }
            EnsureSegmentsAhead();
            
            // Cleanup: remove segments far behind
            while (_activeSegments.Count > 0)
            {
                var oldest = _activeSegments.Peek();
                if (!oldest) { _activeSegments.Dequeue(); continue; }

                var startZ = oldest.transform.position.z;
                var endZ = startZ + segmentLength;

                if (player.position.z - endZ > deleteBuffer)
                {
                    _activeSegments.Dequeue();
                    Destroy(oldest);
                }
                else break;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Fresh scene: rebind and reset generator state
            _initialized = false;
            InitializeIfNeeded();
        }

        private void InitializeIfNeeded()
        {
            if (_initialized) return;

            if (segmentLength <= 0f) segmentLength = 50f;
            if (!segmentsParent) segmentsParent = transform;           // safe default parent
            if (!player) AutoFindPlayer();

            // Clear old state (in case this object survived a scene change)
            ClearQueueAndDestroyChildrenNotInScene();

            // Ingest any pre-placed children under segmentsParent
            var children = new List<Transform>();
            for (var i = 0; i < segmentsParent.childCount; i++)
                children.Add(segmentsParent.GetChild(i));

            _activeSegments.Clear();
            foreach (var t in children.OrderBy(c => c.position.z))
                _activeSegments.Enqueue(t.gameObject);

            if (children.Count > 0)
            {
                var lastStartZ = children.Max(c => c.position.z);
                _nextSpawnZ = lastStartZ + segmentLength;
            }
            else
            {
                var playerZ = player ? player.position.z : 0f;
                _nextSpawnZ = Mathf.Floor(playerZ / segmentLength + 1f) * segmentLength;
            }
            
            _introSegmentsRemaining = ComputeIntroSegments();
            
            EnsureSegmentsAhead();   // prewarm
            _initialized = true;
        }

        private int ComputeIntroSegments()
        {
            if (safeIntroSeconds <= 0f) return 0;

            var speed = 0f;
            if (player)
            {
                var mover = player.GetComponent<Features.Gameplay.Entities.Player.InfinitePlayerMovement>();
                if (mover) speed = Mathf.Max(0f, mover.forwardSpeed); // public in your mover
            }
            // Fallback if no player/mover found
            if (speed <= 0f) speed = segmentLength; // ~1 segment/sec as a safe default

            var meters = speed * safeIntroSeconds;
            return Mathf.Max(0, Mathf.CeilToInt(meters / segmentLength));
        }

        private void AutoFindPlayer()
        {
            // Try by tag first
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) { player = go.transform; return; }

            // Fallback: look for common movement script in the scene
            var mover = FindFirstObjectByType<InfinitePlayerMovement>();
            if (mover) player = mover.transform;
        }

        private void ClearQueueAndDestroyChildrenNotInScene()
        {
            _activeSegments.Clear();
        }

        private void EnsureSegmentsAhead()
        {
            if (!player) return;

            var targetFrontZ = player.position.z + segmentsAhead * segmentLength;
            while (_nextSpawnZ < targetFrontZ)
                SpawnNext();
        }

        public void SetForceSafeSegments(bool on) => forceSafeSegments = on;
        private void SpawnNext()
        {
            if ((segmentPrefabs == null || segmentPrefabs.Length == 0) && !safeIntroPrefab) return;

            GameObject prefabToUse;
            
            
            if (_introSegmentsRemaining > 0 && safeIntroPrefab || (forceSafeSegments && safeIntroPrefab))
            {
                prefabToUse = safeIntroPrefab;
                _introSegmentsRemaining--;
            }
            else
            {
                var idx = Random.Range(0, segmentPrefabs.Length);
                prefabToUse = segmentPrefabs[idx];
            }
            
            var pos = new Vector3(0f, 0f, _nextSpawnZ);
            var seg = Instantiate(prefabToUse, pos, Quaternion.identity, segmentsParent);
            _activeSegments.Enqueue(seg);
            _nextSpawnZ += segmentLength;
        }
    }
}

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

        [Header("Layout")]
        [Tooltip("Exact world length of each segment along +Z.")]
        public float segmentLength = 50f;

        [Tooltip("How many segments should always be ahead of the player.")]
        public int segmentsAhead = 6;

        [Header("Player / Cleanup")]
        public Transform player;
        [Tooltip("Delete a segment after player is this far past the segment END.")]
        public float deleteBuffer = 100f;

        [Header("Hierarchy (optional)")]
        [Tooltip("Parent object that holds all spawned segments. If empty, this.transform will be used.")]
        public Transform segmentsParent;

        // --- Internal state ---
        private readonly Queue<GameObject> _activeSegments = new Queue<GameObject>();
        private float _nextSpawnZ = 0f;
        private bool _initialized = false;

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
            if (!player || segmentPrefabs == null || segmentPrefabs.Length == 0)
            {
                // Try to recover player mid-play if it was recreated
                if (!player) AutoFindPlayer();
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

            EnsureSegmentsAhead();   // prewarm
            _initialized = true;
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

        private void SpawnNext()
        {
            if (segmentPrefabs == null || segmentPrefabs.Length == 0) return;

            var idx = Random.Range(0, segmentPrefabs.Length);
            var pos = new Vector3(0f, 0f, _nextSpawnZ);

            var seg = Instantiate(segmentPrefabs[idx], pos, Quaternion.identity, segmentsParent);
            _activeSegments.Enqueue(seg);
            _nextSpawnZ += segmentLength;
        }
    }
}

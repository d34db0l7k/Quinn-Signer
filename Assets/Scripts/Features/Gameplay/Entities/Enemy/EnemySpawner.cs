using System;
using System.Collections;
using System.Collections.Generic;
using Features.Signing;
using UnityEngine;

namespace Features.Gameplay.Entities.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Main Camera reference")]
        public GameObject mainCamera;

        [Header("GameObject to spawn")]
        public GameObject enemyPrefab;

        [Header("Words")]
        [SerializeField] private WordBank wordBank;

        [Header("Spawn Timing")]
        public float firstSpawnDelay = 1f;
        public float spawnInterval = 2f;

        [Header("Spawn Positions")]
        public float spawnDistance = 50f;
        public Vector2 lateralRangeX = new Vector2(-5, 5);
        public Vector2 lateralRangeY = new Vector2(-3, 3);

        [Header("Lock-In Slots (camera-local)")]
        public Vector3 slotTopLeft;
        public Vector3 slotCenter;
        public Vector3 slotTopRight;

        private readonly List<GameObject> _activeEnemies = new List<GameObject>();
        private Vector3[] _slotOffsets;

        private HintMode _hintMode;

        private void Start()
        {
            if (!wordBank)
            {
                wordBank = FindAnyObjectByType<WordBank>();
            }

            _slotOffsets = new[] { slotTopLeft, slotCenter, slotTopRight };

            if (enemyPrefab == null)
            {
                Debug.LogWarning("EnemySpawner needs an enemyPrefab");
                enabled = false;
                return;
            }

            _hintMode = FindAnyObjectByType<HintMode>();

            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(firstSpawnDelay);

            while (true)
            {
                _activeEnemies.RemoveAll(e => !e);

                if (_activeEnemies.Count < _slotOffsets.Length)
                {
                    var used = new bool[_slotOffsets.Length];

                    foreach (var go in _activeEnemies)
                    {
                        var lockComp = go.GetComponent<EnemyLock>();
                        if (lockComp && lockComp.slotIndex >= 0 && lockComp.slotIndex < used.Length)
                        {
                            used[lockComp.slotIndex] = true;
                        }
                    }

                    var slot = Array.FindIndex(used, taken => !taken);

                    if (slot >= 0)
                    {
                        var word = wordBank.PopRandomWord();

                        if (!string.IsNullOrEmpty(word) && word != "Out of Words!")
                        {
                            var enemy = SpawnEnemy();

                            var lockComp = enemy.GetComponent<EnemyLock>();
                            if (lockComp)
                            {
                                lockComp.slotIndex = slot;
                                lockComp.lockedLocalOffset = _slotOffsets[slot];
                            }

                            var label = enemy.GetComponentInChildren<EnemyLabel>(true);
                            if (label)
                            {
                                label.SetWord(word);
                            }

                            _activeEnemies.Add(enemy);

                            if (_hintMode != null)
                            {
                                _hintMode.TryActivateHintForEnemy(enemy);
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private GameObject SpawnEnemy()
        {
            var cam = mainCamera.transform;

            var basePos = cam.position + cam.forward * spawnDistance;

            basePos += cam.right * UnityEngine.Random.Range(lateralRangeX.x, lateralRangeX.y);
            basePos += cam.up * UnityEngine.Random.Range(lateralRangeY.x, lateralRangeY.y);

            return Instantiate(enemyPrefab, basePos, Quaternion.identity);
        }
    }
}
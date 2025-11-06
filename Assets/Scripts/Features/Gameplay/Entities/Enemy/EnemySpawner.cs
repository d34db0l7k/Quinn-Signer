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
        public float spawnDistance = 50f; // units in front of the camera
        public Vector2 lateralRangeX = new Vector2(-5, 5);
        public Vector2 lateralRangeY = new Vector2(-3, 3);

        [Header("Lock‑In Slots (camera‑local)")]
        public Vector3 slotTopLeft;
        public Vector3 slotCenter;
        public Vector3 slotTopRight;

        private readonly List<GameObject> _activeEnemies = new List<GameObject>();
        private Vector3[] _slotOffsets;

        private void Start()
        {

            // auto-find WordBank
            if (!wordBank) wordBank = FindAnyObjectByType<WordBank>();
            // pack offsets
            _slotOffsets = new[]{slotTopLeft, slotCenter, slotTopRight};
            if (enemyPrefab == null)
            {
                Debug.LogWarning("EnemySpawner needs an enemyPrefab");
                enabled = false;
                return;
            }
            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(firstSpawnDelay);

            while (true)
            {
                // clear defeated enemies
                _activeEnemies.RemoveAll(e => !e);


                // if we have fewer than 3, spawn into the first free slot
                if (_activeEnemies.Count < _slotOffsets.Length)
                {
                    // figure out which slots are taken
                    var used = new bool[_slotOffsets.Length];
                    foreach (var go in _activeEnemies)
                    {
                        var lockComp = go.GetComponent<EnemyLock>();
                        if (lockComp && lockComp.slotIndex >= 0 && lockComp.slotIndex < used.Length)
                            used[lockComp.slotIndex] = true;
                    }

                    // find first free slot index
                    var slot = Array.FindIndex(used, taken => !taken);
                    if (slot >= 0)
                    {
                        var word = wordBank.PopRandomWord();
                        if (!string.IsNullOrEmpty(word) && word != "Out of Words!")
                        {
                            var enemy = SpawnEnemy();
                            // assign its slot to lock into
                            var lockComp = enemy.GetComponent<EnemyLock>();
                            if (lockComp)
                            {
                                lockComp.slotIndex = slot;
                                lockComp.lockedLocalOffset = _slotOffsets[slot];
                            }
                            var label = enemy.GetComponentInChildren<EnemyLabel>(true);
                            if (label) label.SetWord(word);

                            _activeEnemies.Add(enemy);
                        }
                    }
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private GameObject SpawnEnemy()
        {
            var cam = mainCamera;
            var forward = cam.transform.forward;
            var basePos = cam.transform.position + forward * spawnDistance;

            // add random lateral jitter (so they fly in with variety)
            basePos += cam.transform.right * UnityEngine.Random.Range(lateralRangeX.x, lateralRangeX.y);
            basePos += cam.transform.up * UnityEngine.Random.Range(lateralRangeY.x, lateralRangeY.y);

            return Instantiate(enemyPrefab, basePos, Quaternion.identity);
        }
    }
}
using UnityEngine;

namespace Features.Gameplay.Entities.Enemy
{
    public class EnemyLock : MonoBehaviour
    {
        [Header("Dolly Camera reference")]
        public GameObject cam;

        [Tooltip("When this forward‑axis distance to the camera is ≤ lockDistance, we lock.")]
        public float lockDistance = 5f;

        [HideInInspector]
        public int slotIndex = -1;

        [HideInInspector]
        public Vector3 lockedLocalOffset;

        private bool _isLocked = false;

        private void Start()
        {
            if (!cam) Debug.LogError("EnemyLock: no Dolly Camera!");
        }

        private void Update()
        {
            if (_isLocked || !cam) return;

            // Compute vector from camera to this enemy
            var toEnemy = transform.position - cam.transform.position;

            // Project onto camera’s forward to get “depth” in front of cam
            var forwardDist = Vector3.Dot(cam.transform.forward, toEnemy);

            // When it’s close enough, snap and parent
            if (forwardDist <= lockDistance)
                LockIntoSlot();
        }

        private void LockIntoSlot()
        {
            _isLocked = true;

            // Reparent under the camera (so it moves with it)
            transform.SetParent(cam.transform, worldPositionStays: false);

            // Then place it at the local offset you chose
            transform.localPosition = lockedLocalOffset;

            // Optionally zero out rotation so it faces you squarely:
            transform.localRotation = Quaternion.identity;

            // If you have any Rigidbody or movement script, disable it here:
            var rb = GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;
        }
    }
}

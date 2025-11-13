namespace Features.Gameplay.Entities.Enemy
{
    using UnityEngine;

    /// Moves the enemy forward until it is `lockLeadZ` ahead of the player,
    /// then keeps it locked at exactly that lead. Prevents the player from ever overtaking it.
    [RequireComponent(typeof(Transform))]
    public class EnemyLeadLock : MonoBehaviour
    {
        public Transform target;              // player ship
        [Tooltip("Desired lead distance ahead of the player (world +Z).")]
        public float lockLeadZ = 30f;
        [Tooltip("Max forward approach speed while catching up to the lock lead (units/sec).")]
        public float approachSpeed = 40f;
        [Tooltip("If the enemy has a Rigidbody, use MovePosition for smoother physics.")]
        public bool useRigidbodyMove = true;

        Rigidbody _rb;

        void Awake() { _rb = GetComponent<Rigidbody>(); }

        void Update()
        {
            if (!target) return;

            // current and desired Z positions
            float playerZ = target.position.z;
            float currentZ = transform.position.z;
            float desiredZ = playerZ + lockLeadZ;

            // 1) Never let the enemy fall behind the player
            if (currentZ < playerZ) currentZ = playerZ;

            // 2) If we're short of the lock point, move forward toward it (at most approachSpeed)
            if (currentZ < desiredZ)
            {
                float step = approachSpeed * Time.deltaTime;
                float nextZ = Mathf.Min(currentZ + step, desiredZ);
                MoveToZ(nextZ);
            }
            else
            {
                // 3) Once we hit the lock point, stick there relative to the player
                MoveToZ(desiredZ);
            }
        }

        void MoveToZ(float z)
        {
            var p = transform.position; p.z = z;
            if (useRigidbodyMove && _rb)
                _rb.MovePosition(p);
            else
                transform.position = p;

            // If using a Rigidbody and it has positive forward velocity, zero it so we don't drift
            if (_rb && _rb.linearVelocity.z > 0f) {
                var v = _rb.linearVelocity;
                v.z = 0f;
                _rb.linearVelocity = v;
            }
        }
    }

}
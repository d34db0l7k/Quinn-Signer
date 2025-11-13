namespace Features.Gameplay.Entities.Enemy
{
// EnemyLockAhead.cs
    using UnityEngine;

    public class EnemyLockAhead : MonoBehaviour
    {
        public Transform target;
        [Tooltip("Max distance in front of target (world +Z) the enemy may be.")]
        public float maxLeadZ = 30f;
        [Tooltip("If true, zero out forward (positive Z) velocity when clamped.")]
        public bool zeroForwardVelocityOnClamp = true;

        Rigidbody _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void LateUpdate()
        {
            if (!target) return;

            var p = transform.position;
            var capZ = target.position.z + maxLeadZ;

            if (p.z > capZ)
            {
                p.z = capZ;
                transform.position = p;

                if (zeroForwardVelocityOnClamp && _rb && _rb.linearVelocity.z > 0f)
                {
                    var v = _rb.linearVelocity;
                    v.z = 0f;
                    _rb.linearVelocity = v;
                }
            }
        }
    }

}
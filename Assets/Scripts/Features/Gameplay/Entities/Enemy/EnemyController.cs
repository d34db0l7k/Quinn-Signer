namespace Features.Gameplay.Entities.Enemy
{
    using Features.Gameplay.Entities.Player;
    using System.Collections;
    using UnityEngine;

    public class EnemyController : MonoBehaviour
    {
        [Header("Death FX")]
        [SerializeField] private ParticleSystem explodeVfx;
        [SerializeField] private AudioClip explodeSfx;
        [SerializeField] private float despawnDelay = 2f;

        private bool _isDead;
        private EnemyLabel _labelCache;

        private void Awake()
        {
            _labelCache = GetComponentInChildren<EnemyLabel>(true);
        }

        private void Start()
        {
            var hintMode = FindFirstObjectByType<Features.Signing.HintMode>();
            if (hintMode != null)
            {
                hintMode.TryActivateHintForEnemy(gameObject);
            }
        }

        public void Explode()
        {
            if (!TryDeath()) return;
        }

        public void Expire()
        {
            if (!TryDeath()) return;
            DamagePlayer(1);
        }

        private bool TryDeath()
        {
            if (_isDead) return false;
            _isDead = true;

            StopBehaviors();
            StopPhysics();
            HideRenderer();
            DestroyLabel();
            TriggerDeathFX();

            Destroy(gameObject, despawnDelay);

            return true;
        }

        private void StopBehaviors()
        {
            foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb != this) mb.enabled = false;
            }
        }

        private void StopPhysics()
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.Sleep();
            }

            foreach (var col in GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }
        }

        private void HideRenderer()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                r.enabled = false;
            }
        }

        private void DestroyLabel()
        {
            var labels = GetComponentsInChildren<EnemyLabel>(true);
            foreach (var l in labels)
            {
                Destroy(l.gameObject);
            }
        }

        private void TriggerDeathFX()
        {
            if (explodeVfx)
            {
                var v = Instantiate(explodeVfx, transform.position, transform.rotation);
                v.Play();
                Destroy(v.gameObject, v.main.duration + 0.5f);
            }

            if (explodeSfx)
            {
                AudioSource.PlayClipAtPoint(explodeSfx, transform.position);
            }
        }

        private void DamagePlayer(int damage)
        {
            var health = FindFirstObjectByType<PlayerHealth>();
            if (health) health.Damage(damage);
        }
    }
}
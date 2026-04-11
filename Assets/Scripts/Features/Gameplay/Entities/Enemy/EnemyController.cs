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
        private bool _notifiedKill;

        private void Awake()
        {
            // cache once; safe even if it’s on a child
            _labelCache = GetComponentInChildren<EnemyLabel>(true);
        }

        private void NotifySignerAndPruneWord()
        {
            if (_notifiedKill) return;
            _notifiedKill = true;

            var signer = FindFirstObjectByType<Features.Signing.Signer>(FindObjectsInactive.Include);
            if (signer && _labelCache)
                signer.HandleEnemyKilled(_labelCache);
        }

        public void Explode()
        {
            if (_isDead) return;
            _isDead = true;

            // 1) stop behaviours
            StopBehaviors();

            // 2) stop physics/collisions
            StopPhysics();

            // 3) hide renderers (so it looks gone even if we keep the root for VFX timing)
            HideRenderer();

            NotifySignerAndPruneWord();
            var hintMode = FindFirstObjectByType<Features.Signing.HintMode>(FindObjectsInactive.Include);
            if (hintMode) hintMode.OnEnemyDestroyed();

            // 4) nuke labels IMMEDIATELY so win checks no longer see this enemy
            DestroyLabel();

            //// 5) FX/SFX
            TriggerDeathFX();

            // 6) finally remove the whole enemy object
            Destroy(gameObject, despawnDelay);
        }

        public void Expire()
        {
            if (_isDead) return;
            _isDead = true;

            StopBehaviors();
            StopPhysics();
            HideRenderer();
            DestroyLabel();
            TriggerDeathFX();
            DamagePlayer(1);
            var hintMode = FindFirstObjectByType<Features.Signing.HintMode>(FindObjectsInactive.Include);
            if (hintMode) hintMode.OnEnemyDestroyed();
            Destroy(gameObject, despawnDelay);
        }

        private void StopBehaviors()
        {
            foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == this) continue;
                mb.enabled = false;
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
                col.enabled = false;
        }

        private void HideRenderer()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }

        private void DestroyLabel()
        {
            var labels = GetComponentsInChildren<EnemyLabel>(true);
            foreach (var l in labels) Destroy(l.gameObject);
        }

        private void TriggerDeathFX()
        {
            if (explodeVfx)
            {
                var v = Instantiate(explodeVfx, transform.position, transform.rotation);
                v.Play();
                Destroy(v.gameObject, v.main.duration + v.main.startLifetime.constantMax + 0.25f);
            }
            if (explodeSfx) AudioSource.PlayClipAtPoint(explodeSfx, transform.position);
        }

        private void DamagePlayer(int damage)
        {
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            health.Damage(damage);
        }
    }
}

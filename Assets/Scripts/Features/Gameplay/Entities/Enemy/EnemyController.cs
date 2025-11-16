namespace Features.Gameplay.Entities.Enemy
{
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
            foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == this) continue;
                mb.enabled = false;
            }

            // 2) stop physics/collisions
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

            // 3) hide renderers (so it looks gone even if we keep the root for VFX timing)
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = false;

            NotifySignerAndPruneWord();
            
            // 4) nuke labels IMMEDIATELY so win checks no longer see this enemy
            var labels = GetComponentsInChildren<EnemyLabel>(true);
            foreach (var l in labels) Destroy(l.gameObject);

            // 5) FX/SFX
            if (explodeVfx)
            {
                var v = Instantiate(explodeVfx, transform.position, transform.rotation);
                v.Play();
                Destroy(v.gameObject, v.main.duration + v.main.startLifetime.constantMax + 0.25f);
            }
            if (explodeSfx) AudioSource.PlayClipAtPoint(explodeSfx, transform.position);

            // 6) finally remove the whole enemy object
            Destroy(gameObject, despawnDelay);
        }
    }
}

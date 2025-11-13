namespace Features.Gameplay.Entities.Player
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class PlayerDeathHook : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerHealth health;               // Player's PlayerHealth
        [SerializeField] private SpaceshipCollision shipCollision;  // optional
        [SerializeField] private GameObject playerRoot;             // <-- set this to your SHIP ROOT (important)

        [Header("Fallback if SpaceshipCollision isn't used")]
        [SerializeField] private GameObject explosionPrefab;        // optional VFX
        [SerializeField] private string defeatSceneName = "DefeatScene";
        [SerializeField] private float defeatDelay = 1.25f;

        void Reset()
        {
            if (!health) health = FindFirstObjectByType<PlayerHealth>();
            if (!shipCollision) shipCollision = FindFirstObjectByType<SpaceshipCollision>();
            if (!playerRoot)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged) playerRoot = tagged;
                else if (shipCollision) playerRoot = shipCollision.gameObject;
            }
        }

        void OnEnable()
        {
            if (!health) health = FindFirstObjectByType<PlayerHealth>();
            if (health != null) health.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            if (health != null) health.OnDeath -= HandleDeath;
        }

        void HandleDeath()
        {
            StartCoroutine(DieRoutine());
        }

        IEnumerator DieRoutine()
        {
            // 1) Trigger explosion if available (spawns VFX/SFX)
            if (shipCollision)
            {
                bool called = false;
                try { shipCollision.SendMessage("Explode", SendMessageOptions.DontRequireReceiver); called = true; } catch {}
                if (!called)
                {
                    var mi = typeof(SpaceshipCollision).GetMethod(
                        "ExplodePlayer",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (mi != null) { mi.Invoke(shipCollision, null); called = true; }
                }
                // give 1 frame so VFX instantiate
                yield return null;
            }
            else
            {
                if (explosionPrefab && playerRoot)
                    Instantiate(explosionPrefab, playerRoot.transform.position, Quaternion.identity);
            }

            // 2) HARD-disable control/physics/rendering — BUT DO NOT SetActive(false) yet
            HardDisablePlayerRig_NoDeactivate(playerRoot ?? TryGuessPlayerRoot());

            // 3) Load defeat after a beat
            if (!string.IsNullOrEmpty(defeatSceneName))
                yield return new WaitForSeconds(defeatDelay);

            if (!string.IsNullOrEmpty(defeatSceneName))
                SceneManager.LoadScene(defeatSceneName, LoadSceneMode.Single);
        }

        // Same as HardDisablePlayerRig but WITHOUT SetActive(false)
        static void HardDisablePlayerRig_NoDeactivate(GameObject root)
        {
            if (!root) return;
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; rb.useGravity = false;
            }
            foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;

            // Keep SpaceshipCollision enabled so any internal coroutines can finish if needed
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!mb) continue;
                if (mb is SpaceshipCollision) continue; // don't disable this component
                mb.enabled = false;
            }

            foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            foreach (var a in root.GetComponentsInChildren<AudioSource>(true)) a.Stop();
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


        GameObject TryGuessPlayerRoot()
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged) return tagged;
            if (shipCollision) return shipCollision.gameObject;
            if (health) return health.gameObject;
            return null;
        }

        static void HardDisablePlayerRig(GameObject root)
        {
            if (!root) return;

            // Stop physics completely
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; rb.useGravity = false;
            }

            // Kill movement/camera/any scripts
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!mb) continue;
                mb.enabled = false;
            }

            // Remove collisions
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            // Hide visuals (mesh, skinned, trail, line, etc.)
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;

            // Silence audio & particles
            foreach (var a in root.GetComponentsInChildren<AudioSource>(true))
                a.Stop();
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Finally, deactivate the whole rig on next frame (ensures VFX instantiated above remain)
            root.SetActive(false);
        }
    }
}
using Core.SceneManagement;
using Features.Gameplay.Entities.Player;
using UnityEngine;

namespace Features.Gameplay.Entities
{
    public class SpaceshipCollision : MonoBehaviour
    {
        [Header("Explosion FX Prefabs")] public GameObject explosionEffect; // prefab with particle effect
        public GameObject bigExplosionEffect; // prefab as a big explosion
        public AudioClip explosionSound;

        [Header("End the Game")] public int nextSceneIndex; // game over scene
        public float sceneDelay;

        private bool _hasCollided = false;
        [SerializeField] private bool deactivateShipOnExplode = false;
        [SerializeField] private float deactivateDelay = 0.0f;

        private void OnCollisionEnter(Collision collision)
        {
            if (!_hasCollided && collision.gameObject.CompareTag("Enemy"))
            {
                ExplodePlayer();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_hasCollided && other.CompareTag("Wall"))
            {
                ExplodePlayer();
            }
        }

        private void HideShipVisualsAndPhysics()
        {
            // 1) Stop movement and physics
            var rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // 2) Disable player controls / movers
            var mover = GetComponent<Features.Gameplay.Entities.Player.InfinitePlayerMovement>();
            if (mover) mover.enabled = false;

            // 3) Disable colliders so we can’t hit anything post-explode
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var c in colliders) c.enabled = false;

            // 4) Turn off visible renderers (mesh + skinned + trail)
            var rends = GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends) r.enabled = false;

            // (optional) stop any looping particle systems / audios on the ship itself
            var ps = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var p in ps) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var audios = GetComponentsInChildren<AudioSource>(true);
            foreach (var a in audios) a.Stop();

            // 5) Optionally deactivate the whole ship object
            if (deactivateShipOnExplode)
                StartCoroutine(DeactivateAfterDelay());
        }

        private System.Collections.IEnumerator DeactivateAfterDelay()
        {
            if (deactivateDelay > 0f) yield return new WaitForSeconds(deactivateDelay);
            gameObject.SetActive(false);
        }
        
        public void Explode()
        {
            ExplodePlayer();
        }
        private void ExplodePlayer()
        {
            if (_hasCollided) return;
            _hasCollided = true; //runs once

            // 1) initial big short explosion
            if (bigExplosionEffect != null)
                Instantiate(bigExplosionEffect, transform.position, Quaternion.identity);

            // 2) debris or smoke effect
            if (explosionEffect != null)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            var shutdown = GetComponent<PlayerShutdown>();
            if (shutdown) shutdown.Execute();
            else PlayerShutdown.Kill(gameObject);
            
            SceneSwitcher.Instance?.SwitchSceneAfterDelay(nextSceneIndex, sceneDelay);
            Destroy(gameObject);
        }
    }
}
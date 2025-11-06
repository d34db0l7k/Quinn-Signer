using Core.SceneManagement;
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

        private void ExplodePlayer()
        {
            _hasCollided = true; //runs once

            // 1) initial big short explosion
            if (bigExplosionEffect != null)
                Instantiate(bigExplosionEffect, transform.position, Quaternion.identity);

            // 2) debris or smoke effect
            if (explosionEffect != null)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            SceneSwitcher.Instance?.SwitchSceneAfterDelay(nextSceneIndex, sceneDelay);
            Destroy(gameObject);
        }
    }
}
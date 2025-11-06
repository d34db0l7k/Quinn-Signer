using UnityEngine;

namespace Features.Gameplay.Entities.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        public GameObject explosionEffect;
        private AudioSource _audio;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
        }
        public void Explode()
        {
            if (_audio)
                _audio.Play(); // sounding

            if (explosionEffect)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}

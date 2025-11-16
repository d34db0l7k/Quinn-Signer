using Features.UI;
using UnityEngine;

namespace Features.Gameplay.Collectibles
{
    public class CollectCrystal : MonoBehaviour
    {
        [SerializeField]
        private AudioClip crystalClip;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
        
            // play the crystal collection sound if assigned
            if (crystalClip != null)
                AudioSource.PlayClipAtPoint(crystalClip, transform.position);

            CrystalWallet.Add(1);
            gameObject.SetActive(false); // maybe use Destroy(game object)?
        }
    }
}

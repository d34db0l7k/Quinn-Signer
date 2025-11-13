using Features.Gameplay.Entities.Enemy;
using Features.Signing;
using UnityEngine;

namespace DevTools
{
    public class KillKey : MonoBehaviour
    {
        [Header("Settings")]
        public KeyCode killKey = KeyCode.K;
        public bool addScore = true;

        [Tooltip("Optional: assign your Signer to award points & win check; will auto-find if null.")]
        public Signer signer;

        private void Awake()
        {
            if (!signer) signer = FindFirstObjectByType<Signer>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            if (Input.GetKeyDown(killKey))
                KillOneEnemy();
        }

        private void KillOneEnemy()
        {
            // Grab all enemy labels
            var labels = FindObjectsByType<EnemyLabel>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
            if (labels == null || labels.Length == 0)
            {
                return;
            }

            // Pick the first non-null label
            EnemyLabel target = null;
            foreach (var l in labels)
            {
                if (l) { target = l; break; }
            }
            if (!target)
            {
                return;
            }

            // scoring using same rule as signing
            if (addScore && signer && !string.IsNullOrEmpty(target.targetWord))
            {
                int pts = Mathf.Max(1, (target.targetWord.Length / 3) + 1);
            }

            // Explode / destroy
            var controller = target.GetComponentInParent<EnemyController>() ?? target.GetComponent<EnemyController>();
            if (controller) controller.Explode();
            else Destroy(target.gameObject);

            // remove word from filters and check win
            if (signer)
                signer.HandleEnemyKilled(target);
        }
    }
}

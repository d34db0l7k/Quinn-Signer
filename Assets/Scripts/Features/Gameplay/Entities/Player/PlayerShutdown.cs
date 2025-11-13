namespace Features.Gameplay.Entities.Player
{
    using System.Collections;
    using System.Linq;
    using UnityEngine;

    /// One-button "kill the player rig" switch:
    /// - stops physics
    /// - disables ALL behaviours on the player (except this component)
    /// - disables all colliders & renderers
    /// - optionally SetActive(false) after a short delay
    public class PlayerShutdown : MonoBehaviour
    {
        [Header("Deactivate whole GameObject after cleanup?")]
        public bool deactivateGameObject = true;
        public float deactivateDelay = 0f;

        [Header("Optional: also disable the attached camera(s)")]
        public bool disableChildCameras = true;

        bool _executed;

        public void Execute()
        {
            if (_executed) return;
            _executed = true;

            // 1) Stop physics on this rig
            foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // 2) Disable all colliders
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            // 3) Disable ALL behaviours (inputs, movement, scripts)
            //    except this shutdown component so coroutines can run
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in behaviours)
            {
                if (!mb || mb == this) continue;
                mb.enabled = false;
            }

            // 4) Turn off visuals
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = false;

            // 5) Optional: mute/stop audio + particles
            foreach (var a in GetComponentsInChildren<AudioSource>(true))
                a.Stop();
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // 6) Optional: disable cameras on the rig (if your main camera is a child)
            if (disableChildCameras)
            {
                foreach (var cam in GetComponentsInChildren<Camera>(true))
                    cam.enabled = false;
            }

            // 7) Optionally deactivate whole object on next frame (lets VFX spawn safely)
            if (deactivateGameObject) StartCoroutine(DeactivateNextFrame());
        }

        IEnumerator DeactivateNextFrame()
        {
            if (deactivateDelay > 0f) yield return new WaitForSeconds(deactivateDelay);
            else yield return null; // next frame
            gameObject.SetActive(false);
        }

        // Static helper if you need to kill from anywhere
        public static void Kill(GameObject playerRoot)
        {
            if (!playerRoot) return;
            var shut = playerRoot.GetComponent<PlayerShutdown>();
            if (!shut) shut = playerRoot.AddComponent<PlayerShutdown>();
            shut.Execute();
        }
    }
}
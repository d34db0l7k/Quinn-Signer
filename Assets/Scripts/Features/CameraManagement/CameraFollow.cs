using UnityEngine;

namespace Features.CameraManagement
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        public Transform target;
        public Vector3 offset;

        [Header("Smoothing")]
        [Tooltip("Time it takes to catch up (position).")]
        public float followSmoothTime;
        [Tooltip("How quickly you slerp rotation (0–1).")]
        [Range(0,1)] public float rotateSmoothSpeed;

        private Vector3 _velocity;
    
        private void LateUpdate()
        {
            if (!target) return;

            // 1) Build a quaternion from the ship's pitch (X) and yaw (Y), ignore roll:
            var rot = Quaternion.Euler(
                target.eulerAngles.x,
                target.eulerAngles.y,
                0f
            );

            // 2) Rotate your offset by that full pitch‑and‑yaw rotation:
            var desiredPos = target.position + rot * offset;

            // 3) Smoothly move to that position:
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPos,
                ref _velocity,
                followSmoothTime
            );

            // 4) Smoothly rotate to look at the ship, using world‑up so no roll:
            var wantRot = Quaternion.LookRotation(
                target.position - transform.position,
                Vector3.up
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                wantRot,
                rotateSmoothSpeed
            );
        }
    }
}

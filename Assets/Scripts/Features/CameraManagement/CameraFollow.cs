using UnityEngine;

namespace Features.CameraManagement
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 5f, -10f);

        [Header("Smoothing")]
        [Tooltip("Time it takes to catch up (position).")]
        public float followSmoothTime = 0.2f;
        [Tooltip("How quickly you slerp rotation (0–1).")]
        [Range(0,1)] public float rotateSmoothSpeed = 0.1f;

        private Vector3 _velocity;
        private bool _lostTarget;

        private void LateUpdate()
        {
            // If we have no target, freeze and try to pick up the next Player
            if (!target)
            {
                if (!_lostTarget) { _lostTarget = true; _velocity = Vector3.zero; }

                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) { target = p.transform; _lostTarget = false; }
                else return; // nothing to follow yet
            }

            // Normal follow
            var rot = Quaternion.Euler(target.eulerAngles.x, target.eulerAngles.y, 0f);
            var desiredPos = target.position + rot * offset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPos,
                ref _velocity,
                followSmoothTime
            );

            var wantRot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, wantRot, rotateSmoothSpeed);
        }
    }
}

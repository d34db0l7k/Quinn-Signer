using UnityEngine;

namespace Multiplayer
{
    public class LocalPlayerCameraFollow : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0f, 4f, -8f);
        public float positionLerp = 8f;
        public float rotationLerp = 8f;

        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                FindTarget();
                return;
            }

            Vector3 desiredPosition = _target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionLerp);

            Quaternion desiredRotation = Quaternion.LookRotation(_target.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerp);
        }

        private void FindTarget()
        {
            NetworkRacePlayer[] players = FindObjectsOfType<NetworkRacePlayer>();

            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    _target = player.CameraFollowTarget != null ? player.CameraFollowTarget : player.transform;
                    break;
                }
            }
        }
    }
}
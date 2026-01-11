using UnityEngine;

namespace Features.CameraManagement
{
    public class FaceCamera : MonoBehaviour
    {
        private UnityEngine.Camera _cam;

        private void Start()
        {
            _cam = UnityEngine.Camera.main;
        }

        private void LateUpdate()
        {
            if (!_cam) return;
            transform.rotation = Quaternion.LookRotation(
                transform.position - _cam.transform.position,
                Vector3.up
            );
        }
    }
}
using UnityEngine;

namespace Multiplayer
{
    public class BillboardToCamera : MonoBehaviour
    {
        private UnityEngine.Camera _cam;

        private void LateUpdate()
        {
            if (!_cam)
                _cam = UnityEngine.Camera.main;

            if (!_cam)
                return;

            var toCam = transform.position - _cam.transform.position;
            transform.rotation = Quaternion.LookRotation(toCam);
        }
    }
}
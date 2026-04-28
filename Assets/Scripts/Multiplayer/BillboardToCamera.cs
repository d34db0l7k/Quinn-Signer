using UnityEngine;

namespace Multiplayer
{
    public class BillboardToCamera : MonoBehaviour
    {
        private UnityEngine.Camera _cam;

        private void LateUpdate()
        {
            if (_cam == null)
                _cam = UnityEngine.Camera.main;

            if (_cam == null)
                return;

            transform.forward = _cam.transform.forward;
        }
    }
}
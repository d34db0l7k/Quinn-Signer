using UnityEngine;

namespace Features.Gameplay.Movement
{
    public class RotateShip : MonoBehaviour
    {
        [Header("Rotation Speed (degrees per second)")]
        public float rotationSpeed = 50f;

        private void Update()
        {
            // Rotate smoothly around the Y axis
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}

using UnityEngine;

namespace Features.Gameplay.Collectibles
{
    public class CollectableRotate : MonoBehaviour
    {
        [SerializeField]
        private float xRot;
        [SerializeField]
        private float yRot;
        [SerializeField]
        private float zRot;

        private void Update()
        {
            transform.Rotate(xRot, yRot, zRot, Space.World);
        
        }
    }
}

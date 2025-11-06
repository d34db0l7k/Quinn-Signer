using DG.Tweening;
using UnityEngine;

namespace Features.Gameplay.Movement
{
    public class RingShip : MonoBehaviour
    {
        public float speed;
        public bool activated;

        // Update is called once per frame
        private void Update()
        {
            if(!activated)
                transform.eulerAngles += new Vector3(0, speed, 0) * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            activated = true;
            transform.parent = other.transform.parent;

            var s = DOTween.Sequence();

            s.Append(transform.DORotate(Vector3.zero, .2f));
            s.Append(transform.DORotate(new Vector3(0, 0, -900), 3, RotateMode.LocalAxisAdd));
            s.Join(transform.DOScale(0, .5f).SetDelay(1f));
            s.AppendCallback(() => Destroy(gameObject));
        }
    }
}

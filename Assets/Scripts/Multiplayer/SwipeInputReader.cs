using System;
using UnityEngine;

namespace Multiplayer
{

    public class SwipeInputReader : MonoBehaviour
    {
        [Header("Swipe")]
        public float touchDeadZonePx = 4f;
        public float touchDamp = 14f;
        public float swipeThresholdPx = 60f;

        private int _activeTouchId = -1;
        private Vector2 _lastTouchPos;
        private Vector2 _touchDeltaSmoothed;
        private Vector2 _accumulated;

        public event Action<SwipeDirection> OnSwipeDetected;

        private void Update()
        {
            ReadKeyboardForTesting();
            ReadMobileSwipe();
        }

        private void ReadKeyboardForTesting()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) OnSwipeDetected?.Invoke(SwipeDirection.Up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) OnSwipeDetected?.Invoke(SwipeDirection.Down);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) OnSwipeDetected?.Invoke(SwipeDirection.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) OnSwipeDetected?.Invoke(SwipeDirection.Right);
        }

        private void ReadMobileSwipe()
        {
            _touchDeltaSmoothed = Vector2.Lerp(
                _touchDeltaSmoothed,
                Vector2.zero,
                1f - Mathf.Exp(-touchDamp * Time.deltaTime)
            );

            if (Input.touchCount == 0)
            {
                _activeTouchId = -1;
                _accumulated = Vector2.zero;
                return;
            }

            if (_activeTouchId == -1)
            {
                Touch first = Input.GetTouch(0);
                _activeTouchId = first.fingerId;
                _lastTouchPos = first.position;
                _accumulated = Vector2.zero;
            }

            Touch? primary = null;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == _activeTouchId)
                {
                    primary = Input.GetTouch(i);
                    break;
                }
            }

            if (primary == null)
            {
                Touch first = Input.GetTouch(0);
                _activeTouchId = first.fingerId;
                _lastTouchPos = first.position;
                _accumulated = Vector2.zero;
                return;
            }

            Touch t = primary.Value;

            if (t.phase == TouchPhase.Began)
            {
                _lastTouchPos = t.position;
                _touchDeltaSmoothed = Vector2.zero;
                _accumulated = Vector2.zero;
                return;
            }

            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                Vector2 delta = t.position - _lastTouchPos;

                if (delta.magnitude < touchDeadZonePx)
                    delta = Vector2.zero;

                _touchDeltaSmoothed = Vector2.Lerp(
                    _touchDeltaSmoothed,
                    delta,
                    1f - Mathf.Exp(-touchDamp * Time.deltaTime)
                );

                _lastTouchPos = t.position;
                _accumulated += _touchDeltaSmoothed;

                if (_accumulated.magnitude >= swipeThresholdPx)
                {
                    SwipeDirection dir;

                    if (Mathf.Abs(_accumulated.x) > Mathf.Abs(_accumulated.y))
                        dir = _accumulated.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
                    else
                        dir = _accumulated.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;

                    OnSwipeDetected?.Invoke(dir);
                    _accumulated = Vector2.zero;
                }
            }

            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                _activeTouchId = -1;
                _accumulated = Vector2.zero;
            }
        }
    }
}
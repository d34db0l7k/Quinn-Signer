using UnityEngine;

namespace Features.Gameplay.Entities.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [Tooltip("Speed when holding Space // two-finger hold")]
        public float forwardSpeed = 8f;
        [Tooltip("Speed when holding Shift // three-finger hold")]
        public float reverseSpeed = 6f;

        [Header("Rotation Speeds")]
        [Tooltip("Yaw (turn) speed, degrees per second")]
        public float yawSpeed = 120f;
        [Tooltip("Pitch (look up/down) speed, degrees per second")]
        public float pitchSpeed = 90f;

        [Header("Rotation Smoothing")]
        [Tooltip("How fast the ship turns toward its target rotation")]
        public float rotationSmoothSpeed = 12f;

        [Header("Lean / Tilt")]
        [Tooltip("Max roll angle when turning")]
        public float maxLeanAngle = 25f;
        [Tooltip("How quickly the lean settles (0 <–> 1)")]
        [Range(0.01f, 1f)]
        public float leanSmooth = 0.2f;

        [Header("Mobile Controls")]
        [Tooltip("Pixels of drag ignored at the start (dead zone)")]
        public float touchDeadZonePx = 4f;
        [Tooltip("Drag -> input sensitivity (bigger = more responsive)")]
        public float touchSensitivity = 0.045f;
        [Tooltip("How quickly drag influence decays when you stop moving")]
        public float touchDamp = 14f;

        // Reference to your ship’s visible model (for roll)
        private Transform _model;

        // Internal rotation state
        private float _yaw;
        private float _pitch;

        // Mobile input state
        private int _activeTouchId = -1;
        private Vector2 _lastTouchPos;
        private Vector2 _touchDeltaSmoothed; // damped delta for stable control

        private void Start()
        {
            // assume the first child is the mesh/model
            if (transform.childCount > 0)
                _model = transform.GetChild(0);
            else
                Debug.LogWarning("PlayerMovement: no child model found!");

            // initialize from current rotation
            var e = transform.eulerAngles;
            _yaw   = e.y;
            _pitch = e.x;
        }

        private void Update()
        {
            var yawInput = 0f;
            var pitchInput = 0f;
            var forwardHeld = false;
            var reverseHeld = false;

            // keyboard controls
            if (Input.GetKey(KeyCode.A))        yawInput   = -1f;
            else if (Input.GetKey(KeyCode.D))   yawInput   = +1f;

            if (Input.GetKey(KeyCode.W))        pitchInput = -1f;
            else if (Input.GetKey(KeyCode.S))   pitchInput = +1f;

            if (Input.GetKey(KeyCode.Space))                    forwardHeld = true;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) reverseHeld = true;

            // mobile controls
            ReadMobileLook(ref yawInput, ref pitchInput);
            ReadMobileThrust(ref forwardHeld, ref reverseHeld);
            // rotation
            _yaw   += yawInput   * yawSpeed   * Time.deltaTime;
            _pitch += pitchInput * pitchSpeed * Time.deltaTime;
            _pitch  = Mathf.Clamp(_pitch, -45f, 45f);

            var targetRot = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmoothSpeed * Time.deltaTime
            );

            // apply movement
            var move = Vector3.zero;
            if (forwardHeld) move += transform.forward * forwardSpeed;
            if (reverseHeld) move -= transform.forward * reverseSpeed;
            transform.position += move * Time.deltaTime;

            // visual lean
            if (_model != null)
            {
                var targetRoll = -yawInput * maxLeanAngle;
                var le = _model.localEulerAngles;
                var currentRoll = le.z > 180f ? le.z - 360f : le.z; // map 0..360 to -180..180
                var newRoll = Mathf.Lerp(currentRoll, targetRoll, leanSmooth);
                _model.localEulerAngles = new Vector3(le.x, le.y, newRoll);
            }
        }

        // One-finger drag to aim (yaw/pitch)
        private void ReadMobileLook(ref float yawInput, ref float pitchInput)
        {
            // Decay smoothed delta every frame
            _touchDeltaSmoothed = Vector2.Lerp(_touchDeltaSmoothed, Vector2.zero, 1f - Mathf.Exp(-touchDamp * Time.deltaTime));

            if (Input.touchCount == 0)
            {
                _activeTouchId = -1;
                return;
            }

            // find primary finger
            if (_activeTouchId == -1)
            {
                // non-UI touch as primary
                _activeTouchId = Input.GetTouch(0).fingerId;
                _lastTouchPos = Input.GetTouch(0).position;
            }

            // get touch matching id
            Touch? primary = null;
            for (var i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == _activeTouchId)
                {
                    primary = Input.GetTouch(i);
                    break;
                }
            }

            if (primary == null)
            {
                // primary finger picked up -> pick another
                _activeTouchId = Input.GetTouch(0).fingerId;
                _lastTouchPos = Input.GetTouch(0).position;
                return;
            }

            var t = primary.Value;

            if (t.phase == TouchPhase.Began)
            {
                _lastTouchPos = t.position;
                _touchDeltaSmoothed = Vector2.zero;
                return;
            }

            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                var delta = t.position - _lastTouchPos;

                // Dead zone to ignore micro jitter
                if (delta.magnitude < touchDeadZonePx)
                    delta = Vector2.zero;

                // Smooth the delta for stable control
                _touchDeltaSmoothed = Vector2.Lerp(_touchDeltaSmoothed, delta, 1f - Mathf.Exp(-touchDamp * Time.deltaTime));
                _lastTouchPos = t.position;

                // Convert to normalized inputs (-1..1-ish), horizontal = yaw, vertical = pitch
                var x = _touchDeltaSmoothed.x * touchSensitivity; // right drag → +yaw
                var y = _touchDeltaSmoothed.y * touchSensitivity; // up drag    → +pitch (invert if you prefer flight)
                // Flight-style invert (drag up = pitch down). Comment out if unwanted:
                y = -y;

                // Combine with keyboard values (mobile adds on top)
                yawInput = Mathf.Clamp(yawInput + x, -1f, 1f);
                pitchInput = Mathf.Clamp(pitchInput + y, -1f, 1f);
            }

            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                _activeTouchId = -1;
            }
        }

        // Two fingers = forward thrust, three fingers = reverse
        private static void ReadMobileThrust(ref bool forwardHeld, ref bool reverseHeld)
        {
            var tc = Input.touchCount;
            switch (tc)
            {
                case >= 2 and < 3:
                    forwardHeld = true;
                    break;
                case >= 3:
                    reverseHeld = true;
                    break;
            }
        }
    }
}

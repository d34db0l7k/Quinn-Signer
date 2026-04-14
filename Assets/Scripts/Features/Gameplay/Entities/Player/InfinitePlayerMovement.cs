using UnityEngine;

namespace Features.Gameplay.Entities.Player
{
    public class InfinitePlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float forwardSpeed = 20f;
        public float laneChangeSpeed = 12f;

        [Header("Lanes")]
        public float laneWidth = 5f;              // Distance between lane centers
        public int minLane = -1;                  // -1 = left, 0 = middle, 1 = right
        public int maxLane =  1;
        public float middleLaneWorldX = 0f;       // World-X of the middle lane center

        [Header("Barrel Roll When Switching Lanes")]
        public Transform visual;                  // Optional: roll only the visual model (child). If null, uses this transform.
        public int   spinRevolutions = 1;         // 1 = 360°, 2 = 720°, etc.
        public float spinDuration    = 0.5f;      // Seconds for one lane-change spin
        public bool  spinMatchesInput = true;     // Left input = CCW, Right input = CW (if false, always same direction)

        [Header("Edge Tumble")]
        public float tumbleDuration  = 1.0f;      // Seconds to wobble on the edge
        public float tumbleAmplitude = 0.4f;      // How far outward it bumps during tumble
        public float tumbleTilt      = 20f;       // Visual tilt (degrees) during tumble
        public bool  lockInputDuringTumble = true;

        [Header("Mobile Controls (Swipe to change lanes)")]
        [Tooltip("Pixels of drag ignored at the start (dead zone)")]
        public float touchDeadZonePx = 4f;

        [Tooltip("Drag -> input smoothing (bigger damp = quicker decay)")]
        public float touchDamp = 14f;

        [Tooltip("How many pixels of horizontal drag count as a lane swipe")]
        public float swipeThresholdPx = 60f;

        [Tooltip("Minimum time between 2 lane changes (sec)")]
        public float laneChangeCooldown = 0.15f;

        [Header("Mobile Thrust (optional)")]
        [Tooltip("Two-finger = boost, three-finger = slow")]
        public bool enableTouchThrust = true;

        [Tooltip("Forward speed multiplier when 2 fingers held")]
        public float boostMultiplier = 1.35f;

        [Tooltip("Forward speed multiplier when 3+ fingers held")]
        public float slowMultiplier = 0.6f;

        public int currentLane => _currentLane;

        // internal state
        private int _currentLane = 0;              // Start in the middle
        private bool _isTumbling = false;          // In edge penalty animation?
        private bool _isSpinning = false;          // In a barrel-roll spin?
        private float _lastLaneChangeTime = -999f; // cooldown timer

        // Touch tracking (free-move logic adapted for swipes)
        private int _activeTouchId = -1;
        private Vector2 _lastTouchPos;
        private Vector2 _touchDeltaSmoothed;
        private float _accumulatedX;               // accumulate horizontal drag to detect a swipe

        private Transform TiltTarget => visual != null ? visual : transform;

        private void Start()
        {
            if (laneWidth <= 0f) laneWidth = 5f;
            laneChangeSpeed = Mathf.Max(0.01f, laneChangeSpeed);
            spinDuration    = Mathf.Max(0.05f, spinDuration);
            spinRevolutions = Mathf.Max(1, spinRevolutions);
            swipeThresholdPx = Mathf.Max(8f, swipeThresholdPx);
        }

        private void Update()
        {
            // --- Forward move with optional touch thrust multipliers ---
            var speedMul = 1f;
            if (enableTouchThrust)
            {
                var tc = Input.touchCount;

                speedMul = tc switch
                {
                    >= 2 and < 3 => boostMultiplier,
                    >= 3         => slowMultiplier,
                    _            => 1f
                };
            }

            transform.Translate(Vector3.forward * (forwardSpeed * speedMul) * Time.deltaTime, Space.World);

            // keyboard and mobile swipe
            if (!_isTumbling || !lockInputDuringTumble)
            {
                if (Input.GetKeyDown(KeyCode.A)) TryChangeLane(-1);
                if (Input.GetKeyDown(KeyCode.D)) TryChangeLane(+1);
                
                ReadMobileSwipe();
            }

            // slide toward lane center (keep flying while spinning)
            var targetX = LaneCenterX(_currentLane);
            var pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneChangeSpeed);
            transform.position = pos;

            if (_isSpinning || _isTumbling) return;
            // if not spinning AND not tumbling
            var e = TiltTarget.localEulerAngles;
            e.z = Mathf.LerpAngle(e.z, 0f, Time.deltaTime * laneChangeSpeed);
            TiltTarget.localEulerAngles = e;
        }

        // one-finger horizontal drag triggers lane changes
        private void ReadMobileSwipe()
        {
            // exponential decay of smoothed delta (prevents stuck input)
            _touchDeltaSmoothed = Vector2.Lerp(_touchDeltaSmoothed, Vector2.zero,
                1f - Mathf.Exp(-touchDamp * Time.deltaTime));

            if (Input.touchCount == 0)
            {
                _activeTouchId = -1;
                _accumulatedX = 0f;
                return;
            }

            // choose a primary finger
            if (_activeTouchId == -1)
            {
                var first = Input.GetTouch(0);
                _activeTouchId = first.fingerId;
                _lastTouchPos = first.position;
            }

            // grab the primary touch
            Touch? primary = null;
            for (var i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId != _activeTouchId) return;
                // if same finger as initial
                primary = Input.GetTouch(i);
                break;
            }

            if (primary == null)
            {
                // if lost, pick a new one
                var first = Input.GetTouch(0);
                _activeTouchId = first.fingerId;
                _lastTouchPos = first.position;
                _accumulatedX = 0f;
                return;
            }

            var t = primary.Value;

            switch (t.phase)
            {
                case TouchPhase.Began:
                    _lastTouchPos = t.position;
                    _touchDeltaSmoothed = Vector2.zero;
                    _accumulatedX = 0f;
                    return;
            }

            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                var delta = t.position - _lastTouchPos;

                // dead zone to ignore micro jitter
                if (delta.magnitude < touchDeadZonePx)
                    delta = Vector2.zero;

                // smooth it
                _touchDeltaSmoothed = Vector2.Lerp(
                    _touchDeltaSmoothed,
                    delta,
                    1f - Mathf.Exp(-touchDamp * Time.deltaTime)
                );

                _lastTouchPos = t.position;

                // accumulate horizontal movement; when past threshold, trigger lane change
                _accumulatedX += _touchDeltaSmoothed.x;

                if (Mathf.Abs(_accumulatedX) >= swipeThresholdPx)
                {
                    int dir = _accumulatedX > 0f ? +1 : -1;

                    // respect a short cooldown so one long swipe doesn't spam lanes
                    if (Time.time - _lastLaneChangeTime >= laneChangeCooldown)
                    {
                        TryChangeLane(dir);
                        _lastLaneChangeTime = Time.time;
                    }

                    // reset accumulator so you can chain swipes within the same drag
                    _accumulatedX = 0f;
                }
            }

            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                _activeTouchId = -1;
                _accumulatedX = 0f;
            }
        }

        private float LaneCenterX(int laneIndex) => middleLaneWorldX + laneIndex * laneWidth;

        private void TryChangeLane(int dir)
        {
            var desired = _currentLane + dir;

            // Inside bounds -> move lanes normally and trigger spin
            if (desired >= minLane && desired <= maxLane)
            {
                _currentLane = desired;

                // Start a 360° spin matching input direction (or fixed direction)
                if (!_isSpinning && !_isTumbling)
                    StartCoroutine(Spin360(dir));

                return;
            }

            // Outside bounds -> trigger edge tumble (dir shows which edge we hit)
            if (!_isTumbling)
                StartCoroutine(EdgeTumble(dir));
        }

        private System.Collections.IEnumerator Spin360(int dir)
        {
            _isSpinning = true;

            // Determine spin sign (CCW = +, CW = - around local Z)
            var sign = (spinMatchesInput ? Mathf.Sign(dir) : 1f);

            // Cache start rotation
            var startEuler = TiltTarget.localEulerAngles;

            // We add N * 360 degrees to Z over spinDuration
            var totalDegrees = 360f * spinRevolutions * sign;

            var elapsed = 0f;
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / spinDuration);

                // Ease in/out (S-curve): 3t^2 - 2t^3
                var ease = (3f * t * t) - (2f * t * t * t);

                var z = startEuler.z + totalDegrees * ease;

                var e = TiltTarget.localEulerAngles;
                e.z = z;
                TiltTarget.localEulerAngles = e;

                yield return null;
            }

            // Land cleanly aligned (mod 360)
            var finalE = TiltTarget.localEulerAngles;
            finalE.z = Mathf.Repeat(startEuler.z + totalDegrees, 360f);
            TiltTarget.localEulerAngles = finalE;

            _isSpinning = false;
        }

        private System.Collections.IEnumerator EdgeTumble(int dir)
        {
            _isTumbling = true;

            var elapsed = 0f;
            var centerX = LaneCenterX(_currentLane);       // edge lane center we’re on
            var outward = Mathf.Sign(dir);                // -1 if trying left at left edge, +1 if right at right edge

            while (elapsed < tumbleDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / tumbleDuration);

                // Sine pulse for bump 0→1→0
                var pulse = Mathf.Sin(t * Mathf.PI);

                // Bump position outward (small) then back
                var pos = transform.position;
                pos.x = centerX + outward * tumbleAmplitude * pulse;
                transform.position = pos;

                // Visual tilt during the bump
                var e = TiltTarget.eulerAngles;
                e.z = -outward * tumbleTilt * pulse;
                TiltTarget.eulerAngles = e;

                yield return null;
            }

            // Snap back to lane center and clear tilt
            var finalPos = transform.position;
            finalPos.x = centerX;
            transform.position = finalPos;

            var finalE = TiltTarget.eulerAngles;
            finalE.z = 0f;
            TiltTarget.eulerAngles = finalE;
            _isTumbling = false;
        }
    }
}
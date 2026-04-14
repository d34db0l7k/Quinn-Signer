using Features.Gameplay.Entities.Player;
using System.Collections;
using UnityEngine;

public class TutorialBossMovement : MonoBehaviour
{
    [Header("Boss Timers")]
    [SerializeField] private float moveCooldown = 1.5f;
    [SerializeField] private float attackDelay = 5f;

    [Header("Battle Precondition")]
    [SerializeField] private bool _preconditionFulfilled = true; // Dialogues, cutscene, etc.

    //private float _partitionWidth;
    //private float _leftBound;
    //private float _rightBound;

    private bool _patternStarted = false;
    private int[] _movementPattern = { 0, -1, 0, 1 }; // { Middle, Left, Middle, Right }

    private int _currentLane = 0;
    private int _currentLaneIndex = 0;
    private Transform _player;
    private InfinitePlayerMovement _playerMovement;
    private TutorialBossController _controller;

    //private UnityEngine.Camera _cam;

    //private void Start()
    //{
    //    StartCoroutine(AssignCameraAndPartitionWidthOnReady());
    //}

    private void Update()
    {
        if (_preconditionFulfilled && !_patternStarted)
        {
            _patternStarted = true;
            StartCoroutine(MoveAndAttack());
        }
    }

    public void InitMovementRefs(Transform player)
    {
        _controller = GetComponent<TutorialBossController>();
        _player = player;
        _playerMovement = player.GetComponent<InfinitePlayerMovement>();
    }

    // Used camera to determine partitions. May need in future.
    //private IEnumerator AssignCameraAndPartitionWidthOnReady()
    //{
    //    while (!_cam)
    //    {
    //        _cam = UnityEngine.Camera.main;
    //        yield return null;
    //    }

    //    float dist = transform.position.z - _cam.transform.position.z;
    //    _leftBound = _cam.ScreenToWorldPoint(new Vector3(0, 0, dist)).x;
    //    _rightBound = _cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, dist)).x;
    //    _partitionWidth = (_rightBound - _leftBound) / 3f;
    //}

    private IEnumerator MoveAndAttack()
    {
        while (_controller && _controller.currentHealth > 0)
        {
            int nextLane = _movementPattern[_currentLaneIndex];
            _currentLaneIndex = (_currentLaneIndex + 1) % _movementPattern.Length;
            _currentLane = nextLane;

            float targetX = GetLaneCenter(nextLane);

            float timeElapsed = 0f;
            float movementTimer = 0.5f;
            Vector3 start = transform.position;
            while (timeElapsed < movementTimer)
            {
                timeElapsed += Time.deltaTime;
                Vector3 curPos = transform.position;
                curPos.x = Mathf.Lerp(start.x, targetX, timeElapsed / movementTimer);
                transform.position = curPos;
                yield return null;
            }

            string laneValToStr = _currentLane switch
            {
                -1 => "left",
                0 => "middle",
                1 => "right",
                _ => "illegal"
            };
            Toast.Instance.ShowToast(
                $"{_controller.bossName} will attack the {laneValToStr} lane in {attackDelay} seconds!",
                1.5f,
                new Vector2(0f, 0f),
                new Vector2((Screen.width * 1.5f), 0f)
                );
            yield return new WaitForSeconds(attackDelay);

            int playerLane = GetPlayerLane();
            if (playerLane == _currentLane) _controller.Attack();

            yield return new WaitForSeconds(moveCooldown);
        }
    }

    private float GetLaneCenter(int lane)
    {
        //return _leftBound + _partitionWidth * (lane + 1.5f); // Was used for camera based tracking. May need in future.
        return _playerMovement.middleLaneWorldX + lane * _playerMovement.laneWidth;
    }

    private int GetPlayerLane()
    {
        return _playerMovement.currentLane;
    }
}

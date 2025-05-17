using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraMovement : MonoBehaviour
{
    [System.Serializable]
    public class CameraPoint
    {
        public Transform pointTransform;
        public float moveDuration = 2f;
        public float waitDuration = 0.5f;
        [Tooltip("Включить сглаживание при движении к следующей точке")]
        public bool useSmoothing = false;
        [Tooltip("Сила сглаживания (0.1-1)")]
        [Range(0.1f, 1f)]
        public float smoothness = 0.5f;
    }

    public enum LockedAxis { None, X, Y, Z }

    [Header("Path Settings")]
    [SerializeField] private CameraPoint[] cameraPath;
    [SerializeField] private float followTransitionDuration = 2f;
    [SerializeField] private float playerStartDelay = 1f;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 2, -5);
    [SerializeField] private LockedAxis lockedAxis = LockedAxis.None;
    [SerializeField] private float cameraTiltAngle = 10f;
    [SerializeField] private float positionSmoothness = 5f;
    [SerializeField][Range(0.1f, 10f)] private float focusSmoothness = 5f;
    [SerializeField][Range(0f, 1f)] private float focusStrength = 1f;

    [Header("Toggle Settings")]
    [SerializeField] private float toggleTransitionDuration = 0.5f;

    [Header("Events")]
    public UnityEvent OnPathComplete;
    public UnityEvent PlayerStartedMovement;

    private Transform _target;
    private PlayerController _playerController;
    private bool _isFollowingPlayer;
    private bool _isTransitioning;
    private bool _isInTransition;
    private bool _isCameraInFront;
    private Vector3 _lockedWorldPosition;
    private bool _isInDialog;
    private bool _wasCameraMovedForDialog;

    public static CameraMovement Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        PlayerManager.PlayerChanged.AddListener((GameObject newPlayer) =>
        {
            _target = newPlayer?.transform;
            _playerController = newPlayer?.GetComponent<PlayerController>();
            Debug.Log($"CameraMovement: Player set. _target: {_target}, _playerController: {_playerController}");
        });
    }

    private void Start()
    {
        if (_target == null)
        {
            Debug.LogWarning("CameraMovement: _target is null at start, waiting for PlayerManager.");
        }

        if (cameraPath.Length > 0)
        {
            StartCoroutine(FollowCameraPath());
        }
        else
        {
            StartFollowingPlayer();
            StartCoroutine(DelayedPlayerStart());
        }
    }

    public void MoveToDialogPosition(Transform targetPosition)
    {
        if (_isInDialog || targetPosition == null)
        {
            Debug.LogWarning($"CameraMovement: Cannot move to dialog position. _isInDialog: {_isInDialog}, targetPosition: {targetPosition}");
            return;
        }

        Debug.Log($"CameraMovement: Moving to dialog position: {targetPosition.position}");
        _isInDialog = true;
        _wasCameraMovedForDialog = true;
        _isFollowingPlayer = false;

        StopAllCoroutines();
        StartCoroutine(SmoothMoveToPosition(targetPosition.position, targetPosition.rotation, toggleTransitionDuration));
    }

    public void OnDialogEnd()
    {
        Debug.Log($"CameraMovement: OnDialogEnd called. _isInDialog: {_isInDialog}, _wasCameraMovedForDialog: {_wasCameraMovedForDialog}");
        if (!_isInDialog) return;

        _isInDialog = false;
        StartCoroutine(ReturnToPlayerAndResume());
    }

    private IEnumerator ReturnToPlayerAndResume()
    {
        Debug.Log($"CameraMovement: ReturnToPlayerAndResume started. _wasCameraMovedForDialog: {_wasCameraMovedForDialog}");

        if (_target == null)
        {
            Debug.LogError("CameraMovement: _target is null, cannot return to player.");
            ResumePlayerMovement();
            yield break;
        }

        if (!_wasCameraMovedForDialog)
        {
            Debug.Log("CameraMovement: Camera was not moved for dialog, resuming player movement.");
            StartFollowingPlayer();
            ResumePlayerMovement();
            yield break;
        }

        Vector3 targetPos = CalculateFollowPosition();
        Quaternion targetRot = CalculateFollowRotation();
        Debug.Log($"CameraMovement: Transitioning to player position: {targetPos}, rotation: {targetRot.eulerAngles}");

        yield return StartCoroutine(SmoothMoveToPosition(targetPos, targetRot, followTransitionDuration));

        StartFollowingPlayer();
        Debug.Log($"CameraMovement: Returned to player. Position: {transform.position}, _isFollowingPlayer: {_isFollowingPlayer}");

        ResumePlayerMovement();
    }

    private void ResumePlayerMovement()
    {
        if (_playerController != null)
        {
            Debug.Log("CameraMovement: Resuming player movement.");
            _playerController.RestoreStateAfterDialog();
        }
        else
        {
            Debug.LogWarning("CameraMovement: _playerController is null, cannot resume player movement.");
        }
    }

    private IEnumerator SmoothMoveToPosition(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Debug.Log($"CameraMovement: Moving to position: {targetPos}, rotation: {targetRot.eulerAngles}, duration: {duration}s");
        float t = 0;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smoothedT = Mathf.SmoothStep(0, 1, t);

            transform.position = Vector3.Lerp(startPos, targetPos, smoothedT);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothedT);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        Debug.Log($"CameraMovement: Reached position: {transform.position}, rotation: {transform.rotation.eulerAngles}");
    }

    private IEnumerator FollowCameraPath()
    {
        if (cameraPath.Length == 0) yield break;

        transform.SetPositionAndRotation(
            cameraPath[0].pointTransform.position,
            cameraPath[0].pointTransform.rotation
        );
        yield return new WaitForSeconds(cameraPath[0].waitDuration);

        for (int i = 1; i < cameraPath.Length; i++)
        {
            CameraPoint prevPoint = cameraPath[i - 1];
            CameraPoint currentPoint = cameraPath[i];

            float t = 0;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            while (t < 1f)
            {
                t += Time.deltaTime / prevPoint.moveDuration;
                float smoothedT = prevPoint.useSmoothing ? Mathf.SmoothStep(0, 1, t) : t;

                transform.position = Vector3.Lerp(
                    startPos,
                    currentPoint.pointTransform.position,
                    prevPoint.useSmoothing ? smoothedT * prevPoint.smoothness : t
                );

                transform.rotation = Quaternion.Slerp(
                    startRot,
                    currentPoint.pointTransform.rotation,
                    prevPoint.useSmoothing ? smoothedT * prevPoint.smoothness : t
                );

                yield return null;
            }
            yield return new WaitForSeconds(currentPoint.waitDuration);
        }

        _isTransitioning = true;
        yield return StartCoroutine(SmoothMoveToPosition(
            CalculateFollowPosition(),
            CalculateFollowRotation(),
            followTransitionDuration
        ));
        _isTransitioning = false;

        StartFollowingPlayer();
        OnPathComplete?.Invoke();

        yield return new WaitForSeconds(playerStartDelay);
        StartPlayerMovement();
    }

    private Vector3 CalculateFollowPosition()
    {
        if (_target == null)
        {
            Debug.LogWarning("CameraMovement: _target is null in CalculateFollowPosition, returning current position.");
            return transform.position;
        }

        Vector3 targetPosition = _target.position;
        Vector3 offset = _isCameraInFront ? new Vector3(followOffset.x, followOffset.y, -followOffset.z) : followOffset;
        Vector3 finalPosition;

        float targetY = targetPosition.y + offset.y;

        switch (lockedAxis)
        {
            case LockedAxis.X:
                finalPosition = new Vector3(
                    _lockedWorldPosition.x,
                    targetY,
                    targetPosition.z + offset.z
                );
                break;

            case LockedAxis.Y:
                finalPosition = new Vector3(
                    targetPosition.x + offset.x,
                    _lockedWorldPosition.y,
                    targetPosition.z + offset.z
                );
                break;

            case LockedAxis.Z:
                finalPosition = new Vector3(
                    targetPosition.x + offset.x,
                    targetY,
                    _lockedWorldPosition.z
                );
                break;

            default:
                finalPosition = new Vector3(
                    targetPosition.x + offset.x,
                    targetY,
                    targetPosition.z + offset.z
                );
                break;
        }

        return finalPosition;
    }

    private Quaternion CalculateFollowRotation()
    {
        if (_target == null)
        {
            Debug.LogWarning("CameraMovement: _target is null in CalculateFollowRotation, returning current rotation.");
            return transform.rotation;
        }

        Vector3 baseDirection = _isCameraInFront ? -Vector3.forward : Vector3.forward;
        Quaternion baseRotation = Quaternion.LookRotation(baseDirection);

        Vector3 lookDirection = _target.position - transform.position;
        lookDirection.y = 0;
        lookDirection = lookDirection.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        bool isLeaningAgainstWall = _playerController != null &&
                                    (_playerController.currentState == PlayerController.PlayerMovementState.left ||
                                     _playerController.currentState == PlayerController.PlayerMovementState.right);

        Quaternion resultRotation;
        if (isLeaningAgainstWall && focusStrength > 0)
        {
            resultRotation = Quaternion.Slerp(baseRotation, targetRotation, focusStrength);
        }
        else
        {
            resultRotation = baseRotation;
        }

        resultRotation *= Quaternion.Euler(cameraTiltAngle, 0, 0);
        return resultRotation;
    }

    private void StartFollowingPlayer()
    {
        if (_target == null)
        {
            Debug.LogWarning("CameraMovement: _target is null in StartFollowingPlayer, cannot start following.");
            return;
        }

        _isFollowingPlayer = true;

        Vector3 targetPos = CalculateFollowPosition();
        switch (lockedAxis)
        {
            case LockedAxis.X:
                _lockedWorldPosition = new Vector3(
                    transform.position.x,
                    targetPos.y,
                    targetPos.z
                );
                break;
            case LockedAxis.Y:
                _lockedWorldPosition = new Vector3(
                    targetPos.x,
                    transform.position.y,
                    targetPos.z
                );
                break;
            case LockedAxis.Z:
                _lockedWorldPosition = new Vector3(
                    targetPos.x,
                    targetPos.y,
                    transform.position.z
                );
                break;
            default:
                _lockedWorldPosition = targetPos;
                break;
        }

        // Устанавливаем начальную позицию камеры
        transform.position = targetPos;
        transform.rotation = CalculateFollowRotation();
        Debug.Log($"CameraMovement: Started following player. Position: {transform.position}, _isFollowingPlayer: {_isFollowingPlayer}");
    }

    private void StartPlayerMovement()
    {
        if (_playerController != null)
        {
            _playerController.StartMovingForward();
            PlayerStartedMovement?.Invoke();
            Debug.Log("CameraMovement: Player movement started.");
        }
        else
        {
            Debug.LogWarning("CameraMovement: _playerController is null in StartPlayerMovement.");
        }
    }

    public void ToggleCameraToFront()
    {
        if (_target == null) return;

        Debug.Log("CameraMovement: Toggling camera to front.");
        StopAllCoroutines();
        _isInTransition = true;
        _isFollowingPlayer = false;
        StartCoroutine(TransitionToFront());
    }

    public void ResetCameraToBack()
    {
        if (_target == null) return;

        Debug.Log("CameraMovement: Resetting camera to back.");
        StopAllCoroutines();
        _isInTransition = true;
        _isFollowingPlayer = false;
        StartCoroutine(TransitionToBack());
    }

    private IEnumerator TransitionToFront()
    {
        float t = 0;
        Vector3 startPos = transform.position;
        float startYaw = transform.eulerAngles.y;

        bool originalIsCameraInFront = _isCameraInFront;
        _isCameraInFront = true;
        Vector3 targetPos = CalculateFollowPosition();
        Quaternion targetRot = CalculateFollowRotation();
        float targetYaw = targetRot.eulerAngles.y;
        _isCameraInFront = originalIsCameraInFront;

        while (t < 1f)
        {
            t += Time.deltaTime / toggleTransitionDuration;
            float smoothedT = Mathf.SmoothStep(0, 1, t);

            _isCameraInFront = true;
            transform.position = Vector3.Lerp(startPos, CalculateFollowPosition(), smoothedT);
            _isCameraInFront = originalIsCameraInFront;

            _isCameraInFront = true;
            Quaternion currentTargetRot = CalculateFollowRotation();
            float currentTargetYaw = currentTargetRot.eulerAngles.y;
            float currentYaw = Mathf.LerpAngle(startYaw, currentTargetYaw, smoothedT);
            transform.rotation = Quaternion.Euler(cameraTiltAngle, currentYaw, 0);
            _isCameraInFront = originalIsCameraInFront;

            yield return null;
        }

        _isCameraInFront = true;
        _isInTransition = false;
        _isFollowingPlayer = true;
        transform.position = CalculateFollowPosition();
        transform.rotation = CalculateFollowRotation();
        Debug.Log($"CameraMovement: Transition to front complete. Position: {transform.position}");
    }

    private IEnumerator TransitionToBack()
    {
        float t = 0;
        Vector3 startPos = transform.position;
        float startYaw = transform.eulerAngles.y;

        bool originalIsCameraInFront = _isCameraInFront;
        _isCameraInFront = false;
        Vector3 targetPos = CalculateFollowPosition();
        Quaternion targetRot = CalculateFollowRotation();
        float targetYaw = targetRot.eulerAngles.y;
        _isCameraInFront = originalIsCameraInFront;

        while (t < 1f)
        {
            t += Time.deltaTime / toggleTransitionDuration;
            float smoothedT = Mathf.SmoothStep(0, 1, t);

            _isCameraInFront = false;
            transform.position = Vector3.Lerp(startPos, CalculateFollowPosition(), smoothedT);
            _isCameraInFront = originalIsCameraInFront;

            _isCameraInFront = false;
            Quaternion currentTargetRot = CalculateFollowRotation();
            float currentTargetYaw = currentTargetRot.eulerAngles.y;
            float currentYaw = Mathf.LerpAngle(startYaw, currentTargetYaw, smoothedT);
            transform.rotation = Quaternion.Euler(cameraTiltAngle, currentYaw, 0);
            _isCameraInFront = originalIsCameraInFront;

            yield return null;
        }

        _isCameraInFront = false;
        _isInTransition = false;
        _isFollowingPlayer = true;
        transform.position = CalculateFollowPosition();
        transform.rotation = CalculateFollowRotation();
        Debug.Log($"CameraMovement: Transition to back complete. Position: {transform.position}");
    }

    private void LateUpdate()
    {
        if (!_isFollowingPlayer || _isTransitioning || _isInTransition || _target == null) return;

        switch (lockedAxis)
        {
            case LockedAxis.X:
                _lockedWorldPosition.y = transform.position.y;
                _lockedWorldPosition.z = transform.position.z;
                break;

            case LockedAxis.Y:
                _lockedWorldPosition.x = transform.position.x;
                _lockedWorldPosition.z = transform.position.z;
                break;

            case LockedAxis.Z:
                _lockedWorldPosition.x = transform.position.x;
                _lockedWorldPosition.y = transform.position.y;
                break;
        }

        Vector3 targetPosition = CalculateFollowPosition();
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmoothness * Time.deltaTime
        );

        bool isLeaningAgainstWall = _playerController != null &&
                                    (_playerController.currentState == PlayerController.PlayerMovementState.left ||
                                     _playerController.currentState == PlayerController.PlayerMovementState.right);
        Quaternion targetRotation = CalculateFollowRotation();
        if (isLeaningAgainstWall && focusSmoothness >= 10f)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                (isLeaningAgainstWall ? focusSmoothness : positionSmoothness) * Time.deltaTime
            );
        }
    }

    private IEnumerator DelayedPlayerStart()
    {
        yield return new WaitForSeconds(playerStartDelay);
        StartPlayerMovement();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (cameraPath == null || cameraPath.Length < 1) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < cameraPath.Length; i++)
        {
            if (cameraPath[i].pointTransform == null) continue;

            Gizmos.DrawSphere(cameraPath[i].pointTransform.position, 0.3f);

            if (i < cameraPath.Length - 1 && cameraPath[i + 1].pointTransform != null)
            {
                Gizmos.DrawLine(
                    cameraPath[i].pointTransform.position,
                    cameraPath[i + 1].pointTransform.position
                );
            }
        }
    }
#endif
}
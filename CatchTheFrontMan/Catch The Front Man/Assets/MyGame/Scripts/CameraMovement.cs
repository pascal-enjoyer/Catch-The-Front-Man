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

    [Header("Path Settings")]
    [SerializeField] private CameraPoint[] cameraPath;
    [SerializeField] private float followTransitionDuration = 2f;
    [SerializeField] private float playerStartDelay = 1f;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 2, -5);
    [SerializeField] private float cameraTiltAngle = 10f;
    [SerializeField] private float followSmoothness = 5f;

    [Header("Events")]
    public UnityEvent OnPathComplete;

    private Transform _target;
    private PlayerController _playerController;
    private bool _isFollowingPlayer;
    private bool _isTransitioning;

    private void Awake()
    {
        PlayerManager.PlayerChanged.AddListener((GameObject newPlayer) =>
        {
            _target = newPlayer.transform;
            _playerController = newPlayer.GetComponent<PlayerController>();
        });
    }

    private void Start()
    {
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

                // Применяем сглаживание если включено
                float smoothedT = prevPoint.useSmoothing ?
                    Mathf.SmoothStep(0, 1, t) :
                    t;

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
        yield return TransitionToPlayer(cameraPath[^1].useSmoothing, cameraPath[^1].smoothness);
        _isTransitioning = false;

        OnPathComplete?.Invoke();

        yield return new WaitForSeconds(playerStartDelay);
        StartPlayerMovement();
    }
    private IEnumerator TransitionToPlayer(bool useSmoothing = false, float smoothness = 0.5f)
    {
        float t = 0;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 targetPos = CalculateFollowPosition();
        Quaternion targetRot = CalculateFollowRotation();

        while (t < 1f)
        {
            t += Time.deltaTime / followTransitionDuration;

            // Применяем сглаживание, если включено
            float smoothedT = useSmoothing ? Mathf.SmoothStep(0, 1, t) : t;

            transform.position = Vector3.Lerp(startPos, targetPos, smoothedT);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothedT);

            yield return null;
        }

        StartFollowingPlayer(); // Активируем режим следования после завершения перехода
    }

    private Vector3 CalculateFollowPosition()
    {
        return _target.position
            + _target.forward * followOffset.z // Исправлено: теперь + для отрицательных значений Z
            + _target.up * followOffset.y
            + _target.right * followOffset.x;
    }

    private Quaternion CalculateFollowRotation()
    {
        // Основное направление - куда смотрит игрок
        Quaternion baseRotation = _target.rotation;

        // Добавляем наклон камеры
        return baseRotation * Quaternion.Euler(cameraTiltAngle, 0, 0);
    }

    private void StartFollowingPlayer()
    {
        _isFollowingPlayer = true;
    }

    private void StartPlayerMovement()
    {
        if (_playerController != null)
        {
            _playerController.StartMovingForward();
        }
    }

    private void LateUpdate()
    {
        if (!_isFollowingPlayer || _isTransitioning || _target == null) return;

        transform.position = Vector3.Lerp(
            transform.position,
            CalculateFollowPosition(),
            followSmoothness * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            CalculateFollowRotation(),
            followSmoothness * Time.deltaTime
        );
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
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public GameObject player => PlayerManager.Instance.currentPlayer;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 10f;
    public float rotationSpeed = 5f;
    public bool GizmosOn = true;
    public PlayerAnimationManager animator;
    public float shootDelay = 0.3f;

    [Header("Prediction Settings")]
    [SerializeField] private float verticalOffset = 0.5f;
    [SerializeField] private bool debugPrediction = false;

    private bool isDelaying = true;
    private float delayTimer = 0f;
    private float fireTimer = 0f;
    public bool IsPlayerVisible;
    private bool wasPlayerVisible = false;
    public bool isDead = false;
    public bool enemyTouchesPlayer = false;

    private Collider _playerCollider => player.GetComponent<Collider>();
    private PlayerController _playerController => player.GetComponent<PlayerController>();
    private Vector3 _lastPlayerPosition;
    private Vector3 _calculatedVelocity;

    void Start()
    {

        if (player != null)
        {
            _lastPlayerPosition = player.transform.position;
        }
    }

    void Update()
    {
        if (player == null || _playerCollider == null || isDead) return;

        // Calculate custom velocity
        _calculatedVelocity = (player.transform.position - _lastPlayerPosition) / Time.deltaTime;
        _lastPlayerPosition = player.transform.position;

        CheckPlayerVisibility();

        if (IsPlayerVisible)
        {
            RotateTowardsPlayer();
            animator.ChangeAnimation("Firing");

            if (!wasPlayerVisible)
            {
                delayTimer = shootDelay;
                isDelaying = true;
            }

            if (isDelaying)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0)
                {
                    isDelaying = false;
                    fireTimer = 1f / fireRate;
                }
            }
            else
            {
                fireTimer += Time.deltaTime;
                if (fireTimer >= 1f / fireRate)
                {
                    Shoot();
                    fireTimer = 0f;
                }
            }
        }
        else
        {
            if (wasPlayerVisible)
            {
                animator.ChangeAnimation("Idle");
            }
            fireTimer = 0f;
            isDelaying = true;
        }

        wasPlayerVisible = IsPlayerVisible;
    }

    void RotateTowardsPlayer()
    {
        Vector3 playerCenter = GetPlayerAimPoint();
        Vector3 direction = (playerCenter - transform.position).normalized;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void Shoot()
    {
        if (bulletPrefab && firePoint && player != null)
        {
            Vector3 aimPoint = GetPredictedAimPoint();
            Vector3 direction = (aimPoint - firePoint.position).normalized;
            if (_playerController.currentState != PlayerController.PlayerMovementState.center)
            {
                aimPoint = GetPlayerAimPoint();
                direction = (aimPoint - firePoint.position).normalized;
            }
            // Визуализация
            if (debugPrediction)
            {
                Debug.DrawLine(firePoint.position, aimPoint, Color.cyan, 1f);
                Debug.DrawRay(aimPoint, Vector3.up * 2f, Color.red, 1f);
            }

            // Создание пули
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            Bullet bulletComponent = bullet.GetComponent<Bullet>();

            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction * bulletSpeed);
            }
            else
            {
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb) bulletRb.linearVelocity = direction * bulletSpeed;
            }
            if (debugPrediction)
                Debug.Log($"Bullet speed: {bulletSpeed} | Direction: {direction}");
        }
    }

    Vector3 GetPredictedAimPoint()
    {
        Vector3 targetPos = GetPlayerAimPoint();
        Vector3 shooterPos = firePoint.position;
        Vector3 targetVelocity = GetPredictionVelocity();

        // Добавляем логирование для диагностики
        if (debugPrediction)
        {
            Debug.Log($"PlayerState: {_playerController.currentState}");
            Debug.Log($"Calculated Velocity: {_calculatedVelocity}");
            Debug.Log($"Predicted Velocity: {targetVelocity}");
        }

        // Упрощенный физический расчет с приоритетом реальной скорости
        float timeToTarget = Vector3.Distance(shooterPos, targetPos) / bulletSpeed;
        return targetPos + targetVelocity * timeToTarget;
    }


    Vector3 GetPredictionVelocity()
    {
        // Всегда используем реальную скорость, если игрок движется
        if (!_playerController.isStopped) return _calculatedVelocity;

        // Только для специальных состояний
        switch (_playerController.currentState)
        {
            case PlayerController.PlayerMovementState.left:
                return Vector3.left * _playerController.moveSpeed;
            case PlayerController.PlayerMovementState.right:
                return Vector3.right * _playerController.moveSpeed;
            case PlayerController.PlayerMovementState.down:
                return Vector3.back * _playerController.moveSpeed;
            default:
                return _calculatedVelocity;
        }
    }

    Vector3 GetPlayerAimPoint()
    {
        if (_playerController.currentState == PlayerController.PlayerMovementState.down)
        {
            return _playerCollider.bounds.center + Vector3.up * verticalOffset;
        }
        return _playerCollider.bounds.center;
    }

    void CheckPlayerVisibility()
    {
        IsPlayerVisible = false;
        if (player == null || isDead || _playerController.isDead) return;

        Vector3 aimPoint = GetPlayerAimPoint();
        Vector3 toPlayer = aimPoint - transform.position;

        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance > viewRadius * viewRadius) return;

        Vector3 directionToPlayer = toPlayer.normalized;
        if (Vector3.Dot(transform.forward, directionToPlayer) < Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad) && !enemyTouchesPlayer)
            return;

        if (!Physics.Raycast(firePoint.position, directionToPlayer,
            out RaycastHit hit, Mathf.Sqrt(sqrDistance), obstacleMask) || enemyTouchesPlayer)
        {
            IsPlayerVisible = true;
        }
    }

    void OnDrawGizmos()
    {
        if (!GizmosOn) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        if (IsPlayerVisible && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, GetPlayerAimPoint());
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0,
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
        );
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            enemyTouchesPlayer = true;
        }
    }
}
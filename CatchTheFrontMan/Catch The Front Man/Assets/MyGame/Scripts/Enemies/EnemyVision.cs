using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public GameObject player;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 10f;
    public float rotationSpeed = 5f;
    public bool GizmosOn = true;
    public PlayerAnimationManager animator;

    public float shootDelay = 0.3f; // Задержка перед стрельбой

    private bool isDelaying = true;
    private float delayTimer = 0f;
    private float fireTimer = 0f;
    public bool IsPlayerVisible;
    private bool wasPlayerVisible = false;

    public bool isDead = false;
    public bool enemyTouchesPlayer = false;

    private Collider _playerCollider; // Кэшируем коллайдер игрока

    void Start()
    {
        if (player != null)
        {
            _playerCollider = player.GetComponent<Collider>(); // Кэшируем коллайдер игрока
        }
    }

    void Update()
    {
        if (player == null || _playerCollider == null) return;

        CheckPlayerVisibility();

        if (IsPlayerVisible)
        {
            RotateTowardsPlayer();
            animator.ChangeAnimation("Firing");

            // Если игрок только что стал видимым — запускаем задержку
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
            isDelaying = true; // Сбрасываем задержку при потере видимости
        }

        wasPlayerVisible = IsPlayerVisible;
    }

    void RotateTowardsPlayer()
    {
        Vector3 playerCenter = GetPlayerAimPoint(); // Получаем точку прицеливания
        Vector3 direction = (playerCenter - transform.position).normalized;

        // Плавный поворот только по горизонтали
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
            Vector3 aimPoint = GetPlayerAimPoint();
            Vector3 direction = (aimPoint - firePoint.position).normalized;

            Quaternion bulletRotation = Quaternion.LookRotation(direction);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);

            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent) bulletComponent.speed = bulletSpeed;
        }
    }

    Vector3 GetPlayerAimPoint()
    {
        // Используем центр коллайдера игрока
        if (_playerCollider != null)
        {
            return _playerCollider.bounds.center;
        }
        return player.transform.position; // На случай, если коллайдер отсутствует
    }

    void CheckPlayerVisibility()
    {
        IsPlayerVisible = false;
        if (player == null || isDead || player.GetComponent<PlayerController>().isDead) return;

        Vector3 aimPoint = GetPlayerAimPoint();
        Vector3 toPlayer = aimPoint - transform.position;

        // 1. Проверка расстояния
        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance > viewRadius * viewRadius) return;

        // 2. Проверка угла обзора
        Vector3 directionToPlayer = toPlayer.normalized;
        if (Vector3.Dot(transform.forward, directionToPlayer) < Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad) && !enemyTouchesPlayer)
            return;

        // 3. Проверка препятствий
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
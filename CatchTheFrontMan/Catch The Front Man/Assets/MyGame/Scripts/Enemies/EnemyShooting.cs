using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletPrefab;

    public float bulletSpeed = 20f;
    public float fireRate = 1f;

    [Header("Prediction Settings")]
    [SerializeField] private bool debugPrediction = false;
    public float rotationSpeed = 5f;

    private bool isDelaying = true;
    private float delayTimer = 0f;
    private float fireTimer = 0f;
    public float shootDelay = 0.3f;
    public PlayerAnimationManager animator;

    public Transform firePoint;

    private void Start()
    {
        if (TryGetComponent<EnemyVision>(out EnemyVision enemyVision))
        {
            enemyVision.PlayerInViewZone.AddListener(OnPlayerInViewZone);
            enemyVision.PlayerOutViewZone.AddListener(OnPlayerOutViewZone);
        }
    }

    private void OnPlayerInViewZone(bool wasPlayerVisible, GameObject player, Vector3 aimPoint, Vector3 calculatedVelocity)
    {
        RotateTowardsPlayer(player.transform.position);
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
                Shoot(player, aimPoint, calculatedVelocity);
                fireTimer = 0f;
            }
        }
    }

    public void OnPlayerOutViewZone(bool wasPlayerVisible)
    {
        if (wasPlayerVisible)
        {
            animator.ChangeAnimation("Idle");
        }
        fireTimer = 0f;
        isDelaying = true;
    }

    void RotateTowardsPlayer(Vector3 playerPosition)
    {
        Vector3 playerCenter = playerPosition;
        Vector3 direction = (playerCenter - transform.position).normalized;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );
    }



    public void Shoot(GameObject player, Vector3 playerAimPoint, Vector3 calculatedVelocity)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        Vector3 aimPoint = GetPredictedAimPoint(playerAimPoint, playerController, calculatedVelocity);
        Vector3 direction = (aimPoint - firePoint.position).normalized;

        if (playerController.currentState != PlayerController.PlayerMovementState.center)
        {
            aimPoint = playerAimPoint;
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

    private Vector3 GetPredictedAimPoint(Vector3 targetPos, PlayerController _playerController, Vector3 calculatedVelocity)
    {
        Vector3 shooterPos = firePoint.position;
        Vector3 targetVelocity = GetPredictionVelocity(_playerController, calculatedVelocity);

        // Добавляем логирование для диагностики
        if (debugPrediction)
        {
            Debug.Log($"PlayerState: {_playerController.currentState}");
            Debug.Log($"Calculated Velocity: {calculatedVelocity}");
            Debug.Log($"Predicted Velocity: {targetVelocity}");
        }

        // Упрощенный физический расчет с приоритетом реальной скорости
        float timeToTarget = Vector3.Distance(shooterPos, targetPos) / bulletSpeed;
        return targetPos + targetVelocity * timeToTarget;
    }



    private Vector3 GetPredictionVelocity(PlayerController playerController, Vector3 calculatedVelocity)
    {
        // Всегда используем реальную скорость, если игрок движется
        if (!playerController.isStopped) return calculatedVelocity;

        // Только для специальных состояний
        switch (playerController.currentState)
        {
            case PlayerController.PlayerMovementState.left:
                return Vector3.left * playerController.moveSpeed;
            case PlayerController.PlayerMovementState.right:
                return Vector3.right * playerController.moveSpeed;
            case PlayerController.PlayerMovementState.down:
                return Vector3.back * playerController.moveSpeed;
            default:
                return calculatedVelocity;
        }
    }
}

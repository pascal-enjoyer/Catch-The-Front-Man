using UnityEngine;

public class EnemyShooting : MonoBehaviour, IShootingComponent
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float fireRate = 1f;
    [SerializeField] private bool debugPrediction = false;
    public float rotationSpeed = 5f;
    public float shootDelay = 0.3f;
    public Transform firePoint;
    [SerializeField] private PlayerAnimationManager animator;

    private bool isAttacking;
    private bool isDelaying = true;
    private float delayTimer = 0f;
    private float fireTimer = 0f;
    private Enemy enemy;
    private IVisionComponent vision;

    public bool IsAttacking => isAttacking;

    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;
        vision = GetComponent<IVisionComponent>();
    }

    public void StartAttacking()
    {
        if (!IsEnemyAbleToDoSomething()) return;
        isAttacking = true;
        if (!isDelaying)
        {
            delayTimer = shootDelay;
            isDelaying = true;
        }
    }

    public void StopAttacking()
    {
        isAttacking = false;
        isDelaying = true;
        fireTimer = 0f;
        animator.ChangeAnimation("Idle");
    }

    private bool IsEnemyAbleToDoSomething()
    {
        return enemy.IsActive && !enemy.IsDead;
    }

    private void Update()
    {
        if (!isAttacking || !IsEnemyAbleToDoSomething() || !vision.IsPlayerVisible) return;

        RotateTowardsPlayer(vision.Player.transform.position);

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
                Shoot(vision.Player);
                fireTimer = 0f;
            }
        }
    }

    private void RotateTowardsPlayer(Vector3 playerPosition)
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

    private void Shoot(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        Vector3 playerAimPoint = playerController.GetComponent<Collider>().bounds.center;
        Vector3 calculatedVelocity = (player.transform.position - player.transform.position) / Time.deltaTime; // Simplified
        Vector3 aimPoint = GetPredictedAimPoint(playerAimPoint, playerController, calculatedVelocity);
        Vector3 direction = (aimPoint - firePoint.position).normalized;

        if (playerController.currentState != PlayerController.PlayerMovementState.center)
        {
            aimPoint = playerAimPoint;
            direction = (aimPoint - firePoint.position).normalized;
        }

        if (debugPrediction)
        {
            Debug.DrawLine(firePoint.position, aimPoint, Color.cyan, 1f);
            Debug.DrawRay(aimPoint, Vector3.up * 2f, Color.red, 1f);
        }

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

    private Vector3 GetPredictedAimPoint(Vector3 targetPos, PlayerController playerController, Vector3 calculatedVelocity)
    {
        Vector3 shooterPos = firePoint.position;
        Vector3 targetVelocity = GetPredictionVelocity(playerController, calculatedVelocity);

        if (debugPrediction)
        {
            Debug.Log($"PlayerState: {playerController.currentState}");
            Debug.Log($"Calculated Velocity: {calculatedVelocity}");
            Debug.Log($"Predicted Velocity: {targetVelocity}");
        }

        float timeToTarget = Vector3.Distance(shooterPos, targetPos) / bulletSpeed;
        return targetPos + targetVelocity * timeToTarget;
    }

    private Vector3 GetPredictionVelocity(PlayerController playerController, Vector3 calculatedVelocity)
    {
        if (!playerController.isStopped) return calculatedVelocity;

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
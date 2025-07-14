using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float shootDelay = 0.3f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private bool debugPrediction = false;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private PlayerAnimationManager animator;

    private float _delayTimer = 0f;
    private float _fireTimer = 0f;
    private bool _isDelaying = true;
    private Vector3 _lastPlayerPosition;
    private Vector3 _calculatedVelocity;
    private EnemyVision _enemyVision;
    private Enemy _enemy;

    public GameObject BulletPrefab => bulletPrefab;
    public float BulletSpeed => bulletSpeed;
    public float FireRate => fireRate;
    public float ShootDelay => shootDelay;
    public Transform FirePoint => firePoint;

    private void Awake()
    {
        _enemyVision = GetComponent<EnemyVision>();
        _enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        if (_enemyVision.player != null)
        {
            _lastPlayerPosition = _enemyVision.player.transform.position;
        }
    }

    public void ResetShooting()
    {
        _delayTimer = shootDelay;
        _isDelaying = true;
        _fireTimer = 0f;
    }

    public void ShootIfPossible()
    {
        if (!_enemy.IsActive || !_enemyVision.IsPlayerVisible) return;

        GameObject player = _enemyVision.player;
        _calculatedVelocity = (player.transform.position - _lastPlayerPosition) / Time.deltaTime;
        _lastPlayerPosition = player.transform.position;

        RotateTowardsPlayer(player.transform.position);

        if (_isDelaying)
        {
            _delayTimer -= Time.deltaTime;
            if (_delayTimer <= 0)
            {
                _isDelaying = false;
                _fireTimer = 1f / fireRate;
            }
        }
        else
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1f / fireRate)
            {
                Shoot(player);
                _fireTimer = 0f;
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
        Vector3 aimPoint = GetPredictedAimPoint(playerController);
        Vector3 direction = (aimPoint - firePoint.position).normalized;

        if (playerController.currentState != PlayerController.PlayerMovementState.center)
        {
            aimPoint = player.GetComponent<Collider>().bounds.center;
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
    }

    private Vector3 GetPredictedAimPoint(PlayerController playerController)
    {
        Vector3 targetPos = playerController.GetComponent<Collider>().bounds.center;
        Vector3 targetVelocity = _calculatedVelocity;

        if (!playerController.isStopped)
        {
            switch (playerController.currentState)
            {
                case PlayerController.PlayerMovementState.left:
                    targetVelocity = Vector3.left * playerController.moveSpeed;
                    break;
                case PlayerController.PlayerMovementState.right:
                    targetVelocity = Vector3.right * playerController.moveSpeed;
                    break;
                case PlayerController.PlayerMovementState.down:
                    targetVelocity = Vector3.back * playerController.moveSpeed;
                    break;
            }
        }

        float timeToTarget = Vector3.Distance(firePoint.position, targetPos) / bulletSpeed;
        return targetPos + targetVelocity * timeToTarget;
    }
}
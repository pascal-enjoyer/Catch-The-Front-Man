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



    public float shootDelay = 0.3f; // Новая переменная для задержки

    private bool isDelaying = true;
    private float delayTimer = 0f;
    private float fireTimer = 0f;
    public bool IsPlayerVisible { get; private set; }
    private bool wasPlayerVisible = false; // Добавляем флаг предыдущего состояния

    public bool isDead = false;

    void Update()
    {
        if (isDead) return;
        CheckPlayerVisibility();

        if (IsPlayerVisible)
        {
            RotateTowardsPlayer();
            animator.ChangeAnimation("Firing");

            // Если игрок только что стал видимым - начинаем задержку
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
                    fireTimer = 1f / fireRate; // Готовы к первому выстрелу
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
            isDelaying = true; // Сбрасываем задержку при потере цели
        }

        wasPlayerVisible = IsPlayerVisible;
    }


    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
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
        if (bulletPrefab && firePoint)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent) bulletComponent.speed = bulletSpeed;
        }
    }

    void CheckPlayerVisibility()
    {
        IsPlayerVisible = false;
        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach (Collider target in targets)
        {
            if (target.gameObject != player) continue;

            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) >= viewAngle / 2) continue;

            float dstToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
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

        if (IsPlayerVisible)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
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
}
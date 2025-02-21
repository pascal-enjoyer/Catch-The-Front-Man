using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public GameObject player;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 10f;

    private float fireTimer = 0f;

    private bool playerVisible = false;

    public bool GizmosOn = true;
    void Update()
    {
        CheckPlayerVisibility();
        if (playerVisible)
        {

            Debug.Log("niga");
            fireTimer += Time.deltaTime;
            if (fireTimer >= 1f / fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
        else
        {
            fireTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab && firePoint)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent)
            {
                bulletComponent.speed = bulletSpeed;
            }
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

        if (playerVisible)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
    void CheckPlayerVisibility()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(
            transform.position,
            viewRadius,
            targetMask
        );

        playerVisible = false;
        foreach (Collider target in targetsInViewRadius)
        {
            if (target.gameObject != player) continue;

            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (!Physics.Raycast(
                    transform.position,
                    dirToTarget,
                    dstToTarget,
                    obstacleMask
                ))
                {
                    playerVisible = true;
                }
            }
        }
    }

    void KillPlayer()
    {
        // Реализация убийства игрока
        Debug.Log("Player Killed!");
        Destroy(player);
        // Или перезагрузка уровня: SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0,
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
        );
    }
}
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    public Transform point;
    public float minWaitTime = 0f;
    public float maxWaitTime = 0f;
    public Transform lookTarget;

    public float GetWaitTime()
    {
        return Random.Range(minWaitTime, maxWaitTime);
    }
}

public class EnemyPatrol : MonoBehaviour
{
    public Waypoint[] waypoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public PatrolType patrolType = PatrolType.PingPong;
    public bool GizmosOn = true;
    public PlayerAnimationManager animator;

    private int currentWaypointIndex = 0;
    private Vector3 targetPosition;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private Quaternion targetRotation;
    private bool isMovingForward = true;
    private EnemyVision enemyVision;

    public bool isDead = false;


    public enum PatrolType { Loop, PingPong }

    void Start()
    {
        enemyVision = GetComponent<EnemyVision>();
        if (waypoints.Length > 0)
        {
            if (waypoints[currentWaypointIndex] != null)
                targetPosition = waypoints[currentWaypointIndex].point.position;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (enemyVision != null && enemyVision.IsPlayerVisible)
        {
            // При обнаружении игрока сбрасываем таймер ожидания
            if (isWaiting)
            {
                waitTimer = waypoints[currentWaypointIndex].GetWaitTime();
            }
            return;
        }

        if (waypoints.Length == 0) return;

        if (isWaiting)
        {
            HandleWaiting();
        }
        else
        {
            MoveToWaypoint();
            RotateTowardsTarget();
        }
    }

    void MoveToWaypoint()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (enemyVision == null || !enemyVision.IsPlayerVisible)
            animator.ChangeAnimation("Walk");

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (waypoints[currentWaypointIndex].GetWaitTime() > 0)
            {
                if (enemyVision == null || !enemyVision.IsPlayerVisible)
                    animator.ChangeAnimation("Idle");
                StartWaiting();
            }
            else
            {
                SetNextWaypoint();
            }
        }
    }

    void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waypoints[currentWaypointIndex].GetWaitTime();

        if (waypoints[currentWaypointIndex].lookTarget != null)
        {
            Vector3 lookDirection = waypoints[currentWaypointIndex].lookTarget.position - transform.position;
            targetRotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            targetRotation = transform.rotation;
        }
    }

    void HandleWaiting()
    {
        // Добавляем проверку на видимость игрока
        if (enemyVision != null && enemyVision.IsPlayerVisible)
        {
            waitTimer = waypoints[currentWaypointIndex].GetWaitTime();
            return;
        }

        waitTimer -= Time.deltaTime;
        RotateDuringWait();

        if (waitTimer <= 0)
        {
            isWaiting = false;
            SetNextWaypoint();
        }
    }
    void SetNextWaypoint()
    {
        if (waypoints.Length <= 1) return;

        if (patrolType == PatrolType.PingPong)
        {
            if (isMovingForward)
            {
                currentWaypointIndex = (currentWaypointIndex < waypoints.Length - 1) ? currentWaypointIndex + 1 : currentWaypointIndex - 1;
                isMovingForward = currentWaypointIndex < waypoints.Length - 1;
            }
            else
            {
                currentWaypointIndex = (currentWaypointIndex > 0) ? currentWaypointIndex - 1 : currentWaypointIndex + 1;
                isMovingForward = currentWaypointIndex == 0;
            }
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        targetPosition = waypoints[currentWaypointIndex].point.position;
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void RotateDuringWait()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void OnDrawGizmos()
    {
        if (!GizmosOn || waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].point != null)
            {
                Gizmos.DrawSphere(waypoints[i].point.position, 0.2f);
                if (i < waypoints.Length - 1 && waypoints[i + 1].point != null)
                    Gizmos.DrawLine(waypoints[i].point.position, waypoints[i + 1].point.position);
            }
        }
    }

    public void Die()
    {
        isDead = true;
        enemyVision.isDead = true;
        animator.ChangeAnimation("Die");
    }
}
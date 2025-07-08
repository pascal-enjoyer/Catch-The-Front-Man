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

public class EnemyPatrol : MonoBehaviour, IPatrolComponent
{
    public Waypoint[] waypoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public PatrolType patrolType = PatrolType.PingPong;
    public bool GizmosOn = true;
    [SerializeField] private PlayerAnimationManager animator;

    private int currentWaypointIndex = 0;
    private Vector3 targetPosition;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private Quaternion targetRotation;
    private bool isMovingForward = true;
    private Enemy enemy;
    private Transform chaseTarget;
    private bool isPatrolling;
    private bool isChasing;

    public bool IsPatrolling => isPatrolling;
    public bool IsChasing => isChasing;

    public enum PatrolType { Loop, PingPong }

    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;
        if (waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
        {
            targetPosition = waypoints[currentWaypointIndex].point.position;
            transform.position = targetPosition;
            RotateImmediatelyToTarget();
        }
    }

    private bool IsEnemyAbleToDoSomething()
    {
        return enemy.IsActive && !enemy.IsDead;
    }

    public void StartPatrolling()
    {
        if (waypoints.Length == 0) return;
        isPatrolling = true;
        isChasing = false;
        if (waypoints.Length == 1)
        {
            StartWaiting();
            animator.ChangeAnimation("Idle");
        }
    }

    public void StopPatrolling()
    {
        isPatrolling = false;
        isWaiting = false;
        animator.ChangeAnimation("Idle");
    }

    public void StartChasing(Transform target)
    {
        isChasing = true;
        isPatrolling = false;
        chaseTarget = target;
        animator.ChangeAnimation("Walk");
    }

    public void StopChasing()
    {
        isChasing = false;
        chaseTarget = null;
        animator.ChangeAnimation("Idle");
    }

    void Update()
    {
        if (!IsEnemyAbleToDoSomething()) return;

        if (isChasing && chaseTarget != null)
        {
            targetPosition = chaseTarget.position;
            MoveToWaypoint();
            RotateTowardsTarget();
        }
        else if (isPatrolling)
        {
            if (waypoints.Length == 1)
            {
                HandleSingleWaypoint();
            }
            else if (waypoints.Length > 1)
            {
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
        }
    }

    void HandleSingleWaypoint()
    {
        RotateTowardsTarget();
        if (isWaiting)
        {
            HandleWaiting();
        }
        else if (waypoints[0].GetWaitTime() > 0)
        {
            StartWaiting();
        }
    }

    void RotateImmediatelyToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void MoveToWaypoint()
    {
        if (waypoints.Length <= 1 && !isChasing) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (!isChasing)
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                if (waypoints[currentWaypointIndex].GetWaitTime() > 0)
                {
                    animator.ChangeAnimation("Idle");
                    StartWaiting();
                }
                else
                {
                    SetNextWaypoint();
                }
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
        if (direction == Vector3.zero) return;

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
}
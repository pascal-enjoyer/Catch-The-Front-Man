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

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] public Waypoint[] waypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool gizmosOn = true;
    [SerializeField] private PlayerAnimationManager animator;

    private int _currentWaypointIndex = 0;
    private Vector3 _targetPosition;
    private bool _isWaiting = false;
    private float _waitTimer = 0f;
    private Quaternion _targetRotation;
    private bool _isMovingForward = true;
    private Enemy _enemy;

    public enum PatrolType { Loop, PingPong }

    public PatrolType patrolType;

    public Waypoint[] Waypoints => waypoints;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            _targetPosition = waypoints[_currentWaypointIndex].point.position;
            transform.position = _targetPosition;
            RotateImmediatelyToTarget();
            animator.ChangeAnimation("Idle");
        }
    }

    public void Patrol()
    {
        if (!_enemy.IsActive) return;

        if (waypoints.Length == 0) return;

        if (waypoints.Length == 1)
        {
            HandleSingleWaypoint();
            return;
        }

        if (_isWaiting)
        {
            HandleWaiting();
        }
        else
        {
            MoveToWaypoint();
            RotateTowardsTarget();
        }
    }

    private void HandleSingleWaypoint()
    {
        RotateTowardsTarget();
        if (_isWaiting)
        {
            HandleWaiting();
        }
        else if (waypoints[0].GetWaitTime() > 0)
        {
            StartWaiting();
        }
    }

    private void MoveToWaypoint()
    {
        if (waypoints.Length <= 1 || _currentWaypointIndex >= waypoints.Length) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPosition,
            moveSpeed * Time.deltaTime
        );

        animator.ChangeAnimation("Walk");

        if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
        {
            if (waypoints[_currentWaypointIndex].GetWaitTime() > 0)
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

    private void StartWaiting()
    {
        _isWaiting = true;
        _waitTimer = waypoints[_currentWaypointIndex].GetWaitTime();

        if (waypoints[_currentWaypointIndex].lookTarget != null)
        {
            Vector3 lookDirection = waypoints[_currentWaypointIndex].lookTarget.position - transform.position;
            _targetRotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            _targetRotation = transform.rotation;
        }
    }

    private void HandleWaiting()
    {
        _waitTimer -= Time.deltaTime;
        RotateDuringWait();

        if (_waitTimer <= 0)
        {
            _isWaiting = false;
            SetNextWaypoint();
        }
    }

    private void SetNextWaypoint()
    {
        if (waypoints.Length <= 1) return;

        if (patrolType == PatrolType.PingPong)
        {
            if (_isMovingForward)
            {
                _currentWaypointIndex = (_currentWaypointIndex < waypoints.Length - 1) ? _currentWaypointIndex + 1 : _currentWaypointIndex - 1;
                _isMovingForward = _currentWaypointIndex < waypoints.Length - 1;
            }
            else
            {
                _currentWaypointIndex = (_currentWaypointIndex > 0) ? _currentWaypointIndex - 1 : _currentWaypointIndex + 1;
                _isMovingForward = _currentWaypointIndex == 0;
            }
        }
        else
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
        }

        _targetPosition = waypoints[_currentWaypointIndex].point.position;
    }

    private void RotateImmediatelyToTarget()
    {
        Vector3 direction = (_targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (_targetPosition - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void RotateDuringWait()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmos()
    {
        if (!gizmosOn || waypoints == null || waypoints.Length == 0) return;

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
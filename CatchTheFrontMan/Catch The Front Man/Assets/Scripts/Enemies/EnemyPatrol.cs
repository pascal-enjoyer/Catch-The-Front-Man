using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    private int currentWaypointIndex = 0;
    private Vector3 targetPosition;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            targetPosition = waypoints[currentWaypointIndex].position;
        }
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        MoveToWaypoint();
        RotateTowardsTarget();
    }

    void MoveToWaypoint()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            targetPosition = waypoints[currentWaypointIndex].position;
        }
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
}
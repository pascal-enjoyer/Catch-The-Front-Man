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
    public Waypoint[] waypoints; // Массив точек патрулирования
    public float moveSpeed = 3f; // Скорость движения
    public float rotationSpeed = 5f; // Скорость поворота

    private int currentWaypointIndex = 0; // Текущая точка
    private Vector3 targetPosition; // Целевая позиция
    private bool isWaiting = false; // Ожидает ли враг
    private float waitTimer = 0f; // Таймер ожидания
    private Quaternion targetRotation; // Целевое вращение

    private bool isMovingForward = true; // Флаг направления движения

    public enum PatrolType { Loop, PingPong }
    public PatrolType patrolType = PatrolType.PingPong;

    public bool GizmosOn = true;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            if (waypoints[currentWaypointIndex] != null)
                targetPosition = waypoints[currentWaypointIndex].point.position;
        }
    }

    void Update()
    {
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

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (waypoints[currentWaypointIndex].GetWaitTime() > 0)
            {
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

        // Устанавливаем цель для поворота
        if (waypoints[currentWaypointIndex].lookTarget != null)
        {
            Vector3 lookDirection = waypoints[currentWaypointIndex].lookTarget.position - transform.position;
            targetRotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            // Если цель не указана, сохраняем текущее вращение
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
            // Определяем следующую точку с учетом "туда-обратно"
            if (isMovingForward)
            {
                if (currentWaypointIndex < waypoints.Length - 1)
                {
                    currentWaypointIndex++;
                }
                else
                {
                    isMovingForward = false;
                    currentWaypointIndex--;
                }
            }
            else
            {
                if (currentWaypointIndex > 0)
                {
                    currentWaypointIndex--;
                }
                else
                {
                    isMovingForward = true;
                    currentWaypointIndex++;
                }
            }
        }
        else if (patrolType == PatrolType.Loop)
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
        if (waypoints == null || waypoints.Length == 0 || !GizmosOn) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].point != null)
            {
                Gizmos.DrawSphere(waypoints[i].point.position, 0.2f);

                // Рисуем линии для всего пути
                if (i < waypoints.Length - 1 && waypoints[i + 1].point != null)
                {
                    Gizmos.DrawLine(waypoints[i].point.position, waypoints[i + 1].point.position);
                }
                // Соединяем последнюю и первую точки красной линией
                else if (i == waypoints.Length - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(waypoints[i].point.position, waypoints[0].point.position);
                }
            }
        }
    }
}
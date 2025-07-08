using UnityEngine;

public class EnemyVision : MonoBehaviour, IVisionComponent
{
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public GameObject Player => PlayerManager.Instance.currentPlayer;
    public bool IsPlayerVisible { get; private set; }
    public bool EnemyTouchesPlayer { get; private set; }

    public Transform visionPoint;
    public bool GizmosOn = true;
    [SerializeField] private float verticalOffset = 0.5f;

    private Enemy enemy;
    private Collider playerCollider => Player.GetComponent<Collider>();
    private PlayerController playerController => Player.GetComponent<PlayerController>();

    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;
        if (visionPoint == null)
        {
            visionPoint = transform;
        }
    }

    private void Update()
    {
        if (!IsEnemyAbleToDoSomething()) return;
        CheckPlayerVisibility();
    }

    private bool IsEnemyAbleToDoSomething()
    {
        return enemy.IsActive && !enemy.IsDead;
    }

    private void CheckPlayerVisibility()
    {
        IsPlayerVisible = false;
        if (!IsEnemyAbleToDoSomething() || playerController.isDead) return;

        Vector3 playerCenter = GetPlayerColliderCenter();
        Vector3 toPlayer = playerCenter - visionPoint.position;
        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance > viewRadius * viewRadius) return;

        Vector3 directionToPlayer = toPlayer.normalized;
        if (Vector3.Dot(transform.forward, directionToPlayer) < Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad) && !EnemyTouchesPlayer)
        {
            return;
        }

        if (Physics.Raycast(visionPoint.position, directionToPlayer, out RaycastHit hit, Mathf.Sqrt(sqrDistance), obstacleMask))
        {
            if (hit.collider.gameObject.CompareTag("Player") || hit.collider.transform.IsChildOf(Player.transform))
            {
                IsPlayerVisible = true;
            }
        }
        else
        {
            IsPlayerVisible = true;
        }
    }

    private Vector3 GetPlayerColliderCenter()
    {
        if (playerController.currentState == PlayerController.PlayerMovementState.down)
        {
            return playerCollider.bounds.center + Vector3.up * verticalOffset;
        }
        return playerCollider.bounds.center;
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            EnemyTouchesPlayer = true;
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            EnemyTouchesPlayer = false;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!GizmosOn) return;

        Vector3 gizmoCenter = visionPoint != null ? visionPoint.position : transform.position;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(gizmoCenter, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gizmoCenter, gizmoCenter + viewAngleA * viewRadius);
        Gizmos.DrawLine(gizmoCenter, gizmoCenter + viewAngleB * viewRadius);

        if (Player != null)
        {
            Gizmos.color = IsPlayerVisible ? Color.red : Color.blue;
            Gizmos.DrawLine(visionPoint.position, GetPlayerColliderCenter());
        }

        if (visionPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(visionPoint.position, visionPoint.forward * 0.5f);
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
#endif
}
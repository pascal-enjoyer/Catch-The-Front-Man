using UnityEngine;
using UnityEngine.Events;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public GameObject player => PlayerManager.Instance.currentPlayer;
    public Transform visionPoint;

    public bool GizmosOn = true;

    public PlayerAnimationManager animator;

    public bool IsPlayerVisible = false;

    private bool wasPlayerVisible = false;

    public bool enemyTouchesPlayer = false;

    private Collider _playerCollider => player.GetComponent<Collider>();
    private PlayerController _playerController => player.GetComponent<PlayerController>();
    private Vector3 _lastPlayerPosition;
    private Vector3 _calculatedVelocity;
    private Enemy enemy;

    public UnityEvent<bool, GameObject, Vector3, Vector3> PlayerInViewZone;
    public UnityEvent<bool> PlayerOutViewZone;

    [SerializeField] private float verticalOffset = 0.5f;

    private bool IsEnemyAbleToDoSomething()
    {
        return enemy.IsActive;
    }

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (player != null)
        {
            _lastPlayerPosition = player.transform.position;
        }

        if (visionPoint == null)
        {
            //Debug.LogWarning("EnemyVision: visionPoint not assigned, using transform.");
            visionPoint = transform;
        }
    }

    void Update()
    {
        if (!IsEnemyAbleToDoSomething()) return;

        CheckPlayerVisibility();

        if (IsPlayerVisible)
        {
            _calculatedVelocity = (player.transform.position - _lastPlayerPosition) / Time.deltaTime;
            _lastPlayerPosition = player.transform.position;
            PlayerInViewZone?.Invoke(wasPlayerVisible, player, GetPlayerColliderCenter(), _calculatedVelocity);
        }
        else
        {
            PlayerOutViewZone?.Invoke(wasPlayerVisible);
        }

        wasPlayerVisible = IsPlayerVisible;
    }

    Vector3 GetPlayerColliderCenter()
    {
        if (_playerController.currentState == PlayerController.PlayerMovementState.down)
        {
            return _playerCollider.bounds.center + Vector3.up * verticalOffset;
        }
        return _playerCollider.bounds.center;
    }

    void CheckPlayerVisibility()
    {
        IsPlayerVisible = false;
        if (!IsEnemyAbleToDoSomething() || _playerController.isDead) { return; }

        Vector3 playerCenter = GetPlayerColliderCenter();
        Vector3 toPlayer = playerCenter - visionPoint.position;
        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance > viewRadius * viewRadius) { return; }

        Vector3 directionToPlayer = toPlayer.normalized;
        if (Vector3.Dot(transform.forward, directionToPlayer) < Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad) && !enemyTouchesPlayer)
        {
            //Debug.Log($"EnemyVision: Player out of view angle. Dot: {Vector3.Dot(transform.forward, directionToPlayer)}");
            return;
        }

        if (Physics.Raycast(visionPoint.position, directionToPlayer, out RaycastHit hit, Mathf.Sqrt(sqrDistance), obstacleMask))
        {
            if (hit.collider.gameObject.CompareTag("Player") || hit.collider.transform.IsChildOf(player.transform))
            {
                IsPlayerVisible = true;
                //Debug.Log($"EnemyVision: Player visible. Raycast hit player's collider: {hit.collider.name}, Distance: {hit.distance}");
            }
            else
            {
                //Debug.Log($"EnemyVision: Player invisible. Hit obstacle: {hit.collider.name}, Distance: {hit.distance}");
            }
        }
        else
        {
            IsPlayerVisible = true;
            //Debug.Log($"EnemyVision: Player visible. No obstacles. Distance: {Mathf.Sqrt(sqrDistance)}");
        }

        //Debug.Log($"EnemyVision: Forward: {transform.forward}, VisionPoint Pos: {visionPoint.position}, VisionPoint Forward: {visionPoint.forward}");
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
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

        if (player != null)
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
    #endif

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0,
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
        );
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            enemyTouchesPlayer = true;
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            enemyTouchesPlayer = false;
        }
    }
}
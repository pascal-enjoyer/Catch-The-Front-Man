using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private IEnemyState _currentState;
    private Enemy _enemy;
    private EnemyVision _enemyVision;
    private EnemyMovement _movement;
    private EnemyShooting _shooting;
    private PlayerAnimationManager _animator;

    public Enemy Enemy => _enemy;
    public EnemyVision EnemyVision => _enemyVision;
    public EnemyMovement Movement => _movement;
    public EnemyShooting Shooting => _shooting;
    public PlayerAnimationManager Animator => _animator;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _enemyVision = GetComponent<EnemyVision>();
        _movement = GetComponent<EnemyMovement>();
        _shooting = GetComponent<EnemyShooting>();
        _animator = GetComponent<PlayerAnimationManager>();
    }

    private void Start()
    {
        TransitionToState(new PatrolState());
    }

    private void Update()
    {
        if (!_enemy.IsActive) return;

        IEnemyState nextState = _currentState?.Update(this);
        if (nextState != null)
        {
            TransitionToState(nextState);
        }
    }

    public void TransitionToState(IEnemyState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState?.Enter(this);
    }

    public void WatchPoint(Vector3 point)
    {
        if (_enemy.IsDead || !_enemy.IsActive)
        {
            Debug.Log($"Enemy {name} cannot be distracted: IsDead={_enemy.IsDead}, IsActive={_enemy.IsActive}");
            return;
        }

        // ѕереходим в DistractedState только если не в ChaseState
        if (!(_currentState is ChaseState))
        {
            TransitionToState(new DistractedState(point));
        }
    }
}
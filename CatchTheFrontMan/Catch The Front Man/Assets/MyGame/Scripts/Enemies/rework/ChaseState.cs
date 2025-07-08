using UnityEngine;

public class ChaseState : IEnemyState
{
    public int Priority => 5;

    public bool CanEnter(Enemy enemy)
    {
        var vision = enemy.GetEnemyComponent<IVisionComponent>();
        return vision != null && vision.IsPlayerVisible && !vision.EnemyTouchesPlayer;
    }

    public void Enter(Enemy enemy)
    {
        var patrol = enemy.GetEnemyComponent<IPatrolComponent>();
        var vision = enemy.GetEnemyComponent<IVisionComponent>();
        if (patrol != null && vision != null)
        {
            patrol.StartChasing(vision.Player.transform);
            enemy.Animator.ChangeAnimation("Walk");
        }
    }

    public void Execute(Enemy enemy)
    {
        var patrol = enemy.GetEnemyComponent<IPatrolComponent>();
        var vision = enemy.GetEnemyComponent<IVisionComponent>();
        if (patrol != null && vision != null && !patrol.IsChasing)
            patrol.StartChasing(vision.Player.transform);
    }

    public void Exit(Enemy enemy)
    {
        var patrol = enemy.GetEnemyComponent<IPatrolComponent>();
        if (patrol != null)
        {
            patrol.StopChasing();
            enemy.Animator.ChangeAnimation("Idle");
        }
    }
}
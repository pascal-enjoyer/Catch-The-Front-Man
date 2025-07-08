using UnityEngine;

public class PatrolState : IEnemyState
{
    public int Priority => 1;

    public bool CanEnter(Enemy enemy)
    {
        return true; // ѕатрулирование Ч состо€ние по умолчанию
    }

    public void Enter(Enemy enemy)
    {
        var patrol = enemy.GetEnemyComponent<IPatrolComponent>();
        if (patrol != null)
        {
            patrol.StartPatrolling();
            enemy.Animator.ChangeAnimation("Walk");
        }
    }

    public void Execute(Enemy enemy)
    {
        var patrol = enemy.GetEnemyComponent<IPatrolComponent>();
        if (patrol != null && !patrol.IsPatrolling)
            patrol.StartPatrolling();
    }

    public void Exit(Enemy enemy)
    {
        var patrol = enemy.GetEnemyComponent<IPatrolComponent>();
        if (patrol != null)
        {
            patrol.StopPatrolling();
            enemy.Animator.ChangeAnimation("Idle");
        }
    }
}
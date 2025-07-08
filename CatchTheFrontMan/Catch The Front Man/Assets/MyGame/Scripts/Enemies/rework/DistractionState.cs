using UnityEngine;

public class DistractionState : IEnemyState
{
    public int Priority => 3;
    private Vector3 distractionPoint;

    public bool CanEnter(Enemy enemy)
    {
        var ears = enemy.GetEnemyComponent<IEarsComponent>();
        return ears != null && ears.IsDistracted(out distractionPoint);
    }

    public void Enter(Enemy enemy)
    {
        var ears = enemy.GetEnemyComponent<IEarsComponent>();
        if (ears != null)
        {
            ears.StartDistraction(distractionPoint);
            enemy.Animator.ChangeAnimation("Idle");
        }
    }

    public void Execute(Enemy enemy)
    {
        var ears = enemy.GetEnemyComponent<IEarsComponent>();
        if (ears == null || !ears.IsDistracted(out _))
            enemy.StateMachine.ChangeState(new PatrolState());
    }

    public void Exit(Enemy enemy)
    {
        var ears = enemy.GetEnemyComponent<IEarsComponent>();
        if (ears != null)
        {
            ears.StopDistraction();
            enemy.Animator.ChangeAnimation("Idle");
        }
    }
}
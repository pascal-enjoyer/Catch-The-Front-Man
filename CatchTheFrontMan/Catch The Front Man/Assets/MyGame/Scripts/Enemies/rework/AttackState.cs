using UnityEngine;

public class AttackState : IEnemyState
{
    public int Priority => 10;

    public bool CanEnter(Enemy enemy)
    {
        var vision = enemy.GetEnemyComponent<IVisionComponent>();
        return vision != null && vision.IsPlayerVisible && vision.EnemyTouchesPlayer;
    }

    public void Enter(Enemy enemy)
    {
        var shooting = enemy.GetEnemyComponent<IShootingComponent>();
        if (shooting != null)
        {
            shooting.StartAttacking();
            enemy.Animator.ChangeAnimation("Firing");
        }
    }

    public void Execute(Enemy enemy)
    {
        var shooting = enemy.GetEnemyComponent<IShootingComponent>();
        if (shooting != null && !shooting.IsAttacking)
            shooting.StartAttacking();
    }

    public void Exit(Enemy enemy)
    {
        var shooting = enemy.GetEnemyComponent<IShootingComponent>();
        if (shooting != null)
        {
            shooting.StopAttacking();
            enemy.Animator.ChangeAnimation("Idle");
        }
    }
}
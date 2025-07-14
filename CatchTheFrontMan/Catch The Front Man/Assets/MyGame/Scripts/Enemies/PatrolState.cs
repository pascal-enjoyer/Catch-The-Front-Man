using UnityEngine;

public class PatrolState : IEnemyState
{
    public void Enter(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Idle");
    }

    public IEnemyState Update(EnemyStateMachine context)
    {
        // Проверяем, видит ли враг игрока
        if (context.EnemyVision.IsPlayerVisible)
        {
            return new ChaseState(); // Переход в Погоню
        }

        // Выполняем патрулирование
        context.Movement.Patrol();

        return null; // Остаемся в текущем состоянии
    }

    public void Exit(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Idle");
    }
}
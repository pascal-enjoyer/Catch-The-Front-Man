using UnityEngine;

public class ChaseState : IEnemyState
{
    public void Enter(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Firing");
        context.Shooting.ResetShooting(); // Сбрасываем таймеры стрельбы
    }

    public IEnemyState Update(EnemyStateMachine context)
    {
        // Проверяем, видит ли враг игрока
        if (!context.EnemyVision.IsPlayerVisible)
        {
            return new PatrolState(); // Возврат к Патрулю
        }

        // Выполняем стрельбу
        context.Shooting.ShootIfPossible();

        return null; // Остаемся в текущем состоянии
    }

    public void Exit(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Idle");
    }
}
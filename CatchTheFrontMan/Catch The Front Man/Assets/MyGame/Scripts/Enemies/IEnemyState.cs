public interface IEnemyState
{
    void Enter(EnemyStateMachine context);
    IEnemyState Update(EnemyStateMachine context); // Возвращает следующее состояние или null, если переход не нужен
    void Exit(EnemyStateMachine context);
}
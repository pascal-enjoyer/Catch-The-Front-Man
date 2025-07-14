public interface IEnemyState
{
    void Enter(EnemyStateMachine context);
    IEnemyState Update(EnemyStateMachine context); // ���������� ��������� ��������� ��� null, ���� ������� �� �����
    void Exit(EnemyStateMachine context);
}
using UnityEngine;

public class PatrolState : IEnemyState
{
    public void Enter(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Idle");
    }

    public IEnemyState Update(EnemyStateMachine context)
    {
        // ���������, ����� �� ���� ������
        if (context.EnemyVision.IsPlayerVisible)
        {
            return new ChaseState(); // ������� � ������
        }

        // ��������� ��������������
        context.Movement.Patrol();

        return null; // �������� � ������� ���������
    }

    public void Exit(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Idle");
    }
}
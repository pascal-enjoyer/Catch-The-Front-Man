using UnityEngine;

public class ChaseState : IEnemyState
{
    public void Enter(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Firing");
        context.Shooting.ResetShooting(); // ���������� ������� ��������
    }

    public IEnemyState Update(EnemyStateMachine context)
    {
        // ���������, ����� �� ���� ������
        if (!context.EnemyVision.IsPlayerVisible)
        {
            return new PatrolState(); // ������� � �������
        }

        // ��������� ��������
        context.Shooting.ShootIfPossible();

        return null; // �������� � ������� ���������
    }

    public void Exit(EnemyStateMachine context)
    {
        context.Animator.ChangeAnimation("Idle");
    }
}
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    private bool isDead = false;
    private bool isActive = true; // ���� ���������� �����

    public bool IsDead => isDead;
    public bool IsActive => isActive; // ��������� ������ � ����� ����������

    public PlayerAnimationManager animator;

    public UnityEvent EnemyDie;

    private void Start()
    {
        // ������������� �� ������� DeathTimer
        DeathTimer.OnTimerStarted += OnDeathTimerStarted;
        DeathTimer.OnTimerEnded += OnDeathTimerEnded;
    }

    private void OnDestroy()
    {
        // ������������ �� �������
        DeathTimer.OnTimerStarted -= OnDeathTimerStarted;
        DeathTimer.OnTimerEnded -= OnDeathTimerEnded;
    }

    private void Update()
    {
        // ��������� ��������� ����������
        UpdateActiveState();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        UpdateActiveState();
        EnemyDie.Invoke();
        animator.ChangeAnimation("Die");
    }

    private void UpdateActiveState()
    {
        bool wasActive = isActive;
        isActive = !isDead && !DeathTimer.IsTimerActive;
        if (wasActive != isActive)
        {
            Debug.Log($"Enemy {gameObject.name} active state changed: IsActive={isActive}, IsDead={isDead}, DeathTimer.IsTimerActive={DeathTimer.IsTimerActive}");
        }
    }

    private void OnDeathTimerStarted()
    {
        UpdateActiveState();
    }

    private void OnDeathTimerEnded()
    {
        UpdateActiveState();
    }
}
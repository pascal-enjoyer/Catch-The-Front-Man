using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    private bool isDead = false;
    private bool isActive = true; // Флаг активности врага

    public bool IsDead => isDead;
    public bool IsActive => isActive; // Публичный доступ к флагу активности

    public PlayerAnimationManager animator;

    public UnityEvent EnemyDie;

    private void Start()
    {
        // Подписываемся на события DeathTimer
        DeathTimer.OnTimerStarted += OnDeathTimerStarted;
        DeathTimer.OnTimerEnded += OnDeathTimerEnded;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        DeathTimer.OnTimerStarted -= OnDeathTimerStarted;
        DeathTimer.OnTimerEnded -= OnDeathTimerEnded;
    }

    private void Update()
    {
        // Обновляем состояние активности
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
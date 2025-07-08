using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private bool isDead = false; private bool isActive = true;
    [SerializeField] private Dictionary<Type, IEnemyComponent> components = new Dictionary<Type, IEnemyComponent>();

    public bool IsDead => isDead;
    public bool IsActive => isActive;
    public PlayerAnimationManager Animator => animator;
    public EnemyStateMachine StateMachine { get; private set; }

    [SerializeField] private PlayerAnimationManager animator;
    public UnityEvent EnemyDie;

    private void Awake()
    {
        StateMachine = new EnemyStateMachine(this);
        RegisterComponents();
        InitializeStates();
    }

    private void RegisterComponents()
    {
        var enemyComponents = GetComponents<IEnemyComponent>();
        foreach (var component in enemyComponents)
        {
            components[component.GetType()] = component;
            component.Initialize(this);
        }
    }

    private void InitializeStates()
    {
        StateMachine.AddState(new PatrolState());
        StateMachine.AddState(new ChaseState());
        StateMachine.AddState(new AttackState());
        StateMachine.AddState(new DistractionState());
        StateMachine.ChangeState(new PatrolState());
    }

    public T GetEnemyComponent<T>() where T : class, IEnemyComponent
    {
        if (components.TryGetValue(typeof(T), out var component))
            return component as T;
        return null;
    }

    private void Start()
    {
        DeathTimer.OnTimerStarted += OnDeathTimerStarted;
        DeathTimer.OnTimerEnded += OnDeathTimerEnded;
    }

    private void OnDestroy()
    {
        DeathTimer.OnTimerStarted -= OnDeathTimerStarted;
        DeathTimer.OnTimerEnded -= OnDeathTimerEnded;
    }

    private void Update()
    {
        UpdateActiveState();
        StateMachine.UpdateState();
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
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine 
{
    private IEnemyState currentState; 
    private List<IEnemyState> possibleStates = new List<IEnemyState>(); 
    private Enemy enemy;

    public EnemyStateMachine(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void AddState(IEnemyState state)
    {
        possibleStates.Add(state);
    }

    public void ChangeState(IEnemyState newState)
    {
        if (currentState != null)
            currentState.Exit(enemy);
        currentState = newState;
        currentState.Enter(enemy);
        Debug.Log($"Enemy {enemy.gameObject.name} changed state to {newState.GetType().Name}");
    }

    public void UpdateState()
    {
        if (currentState == null || enemy.IsDead || !enemy.IsActive)
            return;

        IEnemyState highestPriorityState = currentState;
        int highestPriority = currentState.Priority;

        foreach (var state in possibleStates)
        {
            if (state.CanEnter(enemy) && state.Priority > highestPriority)
            {
                highestPriorityState = state;
                highestPriority = state.Priority;
            }
        }

        if (highestPriorityState != currentState)
        {
            ChangeState(highestPriorityState);
        }

        currentState.Execute(enemy);
    }

}
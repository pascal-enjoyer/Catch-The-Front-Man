using UnityEngine;

public interface IEnemyState 
{ 
    int Priority { get; } 
    bool CanEnter(Enemy enemy); 
    void Enter(Enemy enemy); 
    void Execute(Enemy enemy); 
    void Exit(Enemy enemy); 
}
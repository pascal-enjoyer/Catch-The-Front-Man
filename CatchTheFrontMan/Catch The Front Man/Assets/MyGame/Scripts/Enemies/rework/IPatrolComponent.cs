using UnityEngine;

public interface IPatrolComponent : IEnemyComponent
{
    bool IsPatrolling { get; }
    bool IsChasing { get; }
    void StartPatrolling();
    void StopPatrolling();
    void StartChasing(Transform target);
    void StopChasing();
}
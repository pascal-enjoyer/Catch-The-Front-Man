using UnityEngine;

public interface IEarsComponent : IEnemyComponent
{
    bool IsDistracted(out Vector3 distractionPoint);
    void StartDistraction(Vector3 point);
    void StopDistraction();
}
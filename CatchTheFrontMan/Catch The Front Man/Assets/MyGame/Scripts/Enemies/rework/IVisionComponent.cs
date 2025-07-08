using UnityEngine;

public interface IVisionComponent : IEnemyComponent
{
    bool IsPlayerVisible { get; }
    bool EnemyTouchesPlayer { get; }
    GameObject Player { get; }
}
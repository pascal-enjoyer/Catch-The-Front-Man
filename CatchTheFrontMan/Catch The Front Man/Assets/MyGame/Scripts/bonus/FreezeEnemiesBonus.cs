using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FreezeEnemiesBonus : Bonus
{

    protected override void ApplyEffect(bool activate)
    {
        foreach (GameObject enemy in EnemySpawner.Instance.spawnedEnemies)
        {
            enemy.GetComponent<EnemyPatrol>().isDead = activate;
            enemy.GetComponent<EnemyVision>().isDead = activate;
        }
    }
}
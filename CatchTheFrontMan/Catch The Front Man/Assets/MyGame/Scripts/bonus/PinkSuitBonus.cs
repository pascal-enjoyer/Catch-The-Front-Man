using UnityEngine;

public class PinkSuitBonus : Bonus
{
    protected override void ApplyEffect(bool isActive)
    {
        foreach (GameObject enemy in EnemySpawner.Instance.spawnedEnemies)
        {
            enemy.GetComponent<EnemyVision>().isDead = isActive;
        }
    }
}

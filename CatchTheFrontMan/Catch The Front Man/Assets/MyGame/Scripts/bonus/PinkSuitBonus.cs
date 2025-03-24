using UnityEngine;

public class PinkSuitBonus : Bonus
{
    protected override void ApplyEffect(bool isActive)
    {
        foreach (GameObject enemy in EnemyManager.Instance.SpawnedEnemies)
        {
            enemy.GetComponent<EnemyVision>().isDead = isActive;
        }
    }
}

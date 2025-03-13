using UnityEngine;
using System.Collections.Generic;

public class BonusManager : MonoBehaviour
{


    [System.Serializable]
    public class SpawnPointPair
    {
        public Transform spawnPoint;
        public BonusData bonusPrefab;
    }

    public List<SpawnPointPair> spawnPointPairs = new List<SpawnPointPair>();

    public void SpawnBonuses()
    {
        // Спавним бонусы на всех точках
        foreach (SpawnPointPair pair in spawnPointPairs)
        {
            if (pair.spawnPoint != null)
            {

                GameObject spawnedBonus = Instantiate(pair.bonusPrefab.bonusPrefab, pair.spawnPoint.position, pair.spawnPoint.rotation, transform);
                spawnedBonus.GetComponent<Bonus>().SetData(pair.bonusPrefab);
            }
        }
    }

    // Вызов спавна бонусов (например, при старте игры)
    private void Start()
    {
        SpawnBonuses();
    }
}
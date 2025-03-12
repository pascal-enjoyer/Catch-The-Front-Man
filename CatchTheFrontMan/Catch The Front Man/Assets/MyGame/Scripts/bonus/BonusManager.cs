using UnityEngine;
using System.Collections.Generic;

public class BonusManager : MonoBehaviour
{


    [System.Serializable]
    public class SpawnPointPair
    {
        public Transform spawnPoint;
        public GameObject bonusPrefab;
    }

    public List<SpawnPointPair> spawnPointPairs = new List<SpawnPointPair>();

    public void SpawnBonuses()
    {
        // Спавним бонусы на всех точках
        foreach (SpawnPointPair pair in spawnPointPairs)
        {
            if (pair.spawnPoint != null)
            {
                Instantiate(pair.bonusPrefab, pair.spawnPoint.position, pair.spawnPoint.rotation, transform);
            }
        }
    }

    // Вызов спавна бонусов (например, при старте игры)
    private void Start()
    {
        SpawnBonuses();
    }
}
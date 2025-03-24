using UnityEngine;
using System.Collections.Generic;
using static EnemyPatrol;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public GameObject player;

    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;      // Префаб врага
        public Waypoint[] patrolWaypoints; // Набор точек пути для патрулирования
        public Transform spawnPoint;       // Точка спавна (опционально)
        public PatrolType patrolType;
    }

    [Header("Enemy Spawn Settings")]
    public EnemySpawnData[] enemiesData;   // Массив данных для спавна врагов

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PlayerManager.PlayerChanged.AddListener((GameObject newPlayer) =>
        {
            player = newPlayer;
        });
    }

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    public List<GameObject>  SpawnedEnemies => spawnedEnemies;


    void Start()
    {
        SpawnAllEnemies();
    }

    void SpawnAllEnemies()
    {
        foreach (EnemySpawnData data in enemiesData)
        {
            if (data.enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is missing in EnemyManager!");
                continue;
            }

            // Определяем позицию спавна
            Vector3 spawnPosition = data.spawnPoint != null ?
                data.spawnPoint.position :
                GetFirstWaypointPosition(data.patrolWaypoints);

            // Создаем экземпляр врага
            GameObject enemy = Instantiate(
                data.enemyPrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

            // Настраиваем патрулирование
            SetupEnemyPatrol(enemy, data.patrolWaypoints, data.patrolType);

            spawnedEnemies.Add(enemy);
        }
    }

    Vector3 GetFirstWaypointPosition(Waypoint[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned!");
            return Vector3.zero;
        }
        return waypoints[0].point.position;
    }

    void SetupEnemyPatrol(GameObject enemy, Waypoint[] waypoints, PatrolType patrolType)
    {
        EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
        if (patrol == null)
        {
            Debug.LogError("Enemy prefab is missing EnemyPatrol component!");
            return;
        }

        patrol.waypoints = waypoints;
        patrol.patrolType = patrolType;
    }

    public void ResetAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
        SpawnAllEnemies();
    }
}
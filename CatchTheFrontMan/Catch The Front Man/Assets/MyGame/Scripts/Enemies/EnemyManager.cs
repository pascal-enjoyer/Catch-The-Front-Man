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
        public GameObject enemyPrefab;
        public Waypoint[] patrolWaypoints;
        public Transform spawnPoint;
        public PatrolType patrolType;
    }

    [Header("Enemy Spawn Settings")]
    public EnemySpawnData[] enemiesData;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    public List<GameObject> SpawnedEnemies => spawnedEnemies;

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

        // Подписываемся на события DeathTimer
        DeathTimer.OnTimerStarted += OnDeathTimerStarted;
        DeathTimer.OnTimerEnded += OnDeathTimerEnded;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий DeathTimer
        DeathTimer.OnTimerStarted -= OnDeathTimerStarted;
        DeathTimer.OnTimerEnded -= OnDeathTimerEnded;
    }

    private void Start()
    {
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        foreach (EnemySpawnData data in enemiesData)
        {
            if (data.enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is missing in EnemyManager!");
                continue;
            }

            Vector3 spawnPosition = data.spawnPoint != null ?
                data.spawnPoint.position :
                GetFirstWaypointPosition(data.patrolWaypoints);

            GameObject enemy = Instantiate(
                data.enemyPrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

            SetupEnemyPatrol(enemy, data.patrolWaypoints, data.patrolType);

            spawnedEnemies.Add(enemy);
        }
    }

    private Vector3 GetFirstWaypointPosition(Waypoint[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned!");
            return Vector3.zero;
        }
        return waypoints[0].point.position;
    }

    private void SetupEnemyPatrol(GameObject enemy, Waypoint[] waypoints, PatrolType patrolType)
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

    private void OnDeathTimerStarted()
    {
        Debug.Log("EnemyManager: DeathTimer started. All enemies paused.");
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    Debug.Log($"Enemy {enemy.name} paused: IsActive={enemyComponent.IsActive}");
                }
            }
        }
    }

    private void OnDeathTimerEnded()
    {
        Debug.Log("EnemyManager: DeathTimer ended. All enemies resumed.");
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    Debug.Log($"Enemy {enemy.name} resumed: IsActive={enemyComponent.IsActive}");
                }
            }
        }
    }
}
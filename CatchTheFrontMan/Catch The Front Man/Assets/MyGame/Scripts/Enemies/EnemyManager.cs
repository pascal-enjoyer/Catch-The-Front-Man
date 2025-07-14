using UnityEngine;
using System.Collections.Generic;

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
        public EnemyMovement.PatrolType patrolType;
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

        DeathTimer.OnTimerStarted += OnDeathTimerStarted;
        DeathTimer.OnTimerEnded += OnDeathTimerEnded;
    }

    private void OnDestroy()
    {
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

            SetupEnemy(enemy, data.patrolWaypoints, data.patrolType);

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

    private void SetupEnemy(GameObject enemy, Waypoint[] waypoints, EnemyMovement.PatrolType patrolType)
    {
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement == null)
        {
            Debug.LogError("Enemy prefab is missing EnemyMovement component!");
            return;
        }

        movement.waypoints = waypoints;
        movement.patrolType = patrolType;
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
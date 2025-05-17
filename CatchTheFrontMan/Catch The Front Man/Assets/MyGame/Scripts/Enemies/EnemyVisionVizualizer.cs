using UnityEngine;

public class EnemyVisionVizualizer : MonoBehaviour
{
    [SerializeField] private EnemyVision enemyVision; // Ссылка на компонент EnemyVision
    [SerializeField] private Sprite radiusSprite; // Спрайт для радиуса видимости
    [SerializeField] private Sprite angleSprite; // Спрайт для зоны видимости (в радиусе и в угле обзора)
    [SerializeField] private Sprite visibleSprite; // Спрайт для прямой видимости
    [SerializeField] private float spriteHeightOffset = 1.5f; // Высота спавна над врагом
    [SerializeField] private Vector2 spriteScale = Vector2.one; // Масштаб спрайтов
    [SerializeField] private Color spriteColor = Color.white; // Цвет спрайтов

    private GameObject radiusSpriteObject; // Объект для спрайта радиуса
    private GameObject angleSpriteObject; // Объект для спрайта зоны видимости
    private GameObject visibleSpriteObject; // Объект для спрайта прямой видимости
    private GameObject player => enemyVision.player; // Получаем игрока из EnemyVision
    private bool isPlayerInRadius = false; // Флаг нахождения игрока в радиусе
    private bool isPlayerInAngle = false; // Флаг нахождения игрока в угле обзора
    private Camera mainCamera; // Ссылка на основную камеру
    private Enemy enemy; // Ссылка на компонент Enemy

    void Start()
    {
        // Проверяем необходимые компоненты
        if (enemyVision == null)
        {
            //Debug.LogError("SpriteSpawner: EnemyVision component not assigned!");
            TurnOffVisualizer();
            return;
        }

        enemy = enemyVision.GetComponent<Enemy>();
        if (enemy == null)
        {
            //Debug.LogError("SpriteSpawner: Enemy component not found!");
            TurnOffVisualizer();
            return;
        }

        // Подписываемся на событие смерти врага
        enemy.EnemyDie.AddListener(DestroyAllSprites);
        enemy.EnemyDie.AddListener(TurnOffVisualizer);

        // Проверяем назначение спрайтов
        if (radiusSprite == null || angleSprite == null || visibleSprite == null)
        {
           // Debug.LogWarning("SpriteSpawner: One or more sprites not assigned!");
        }

        // Получаем основную камеру
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            //Debug.LogError("SpriteSpawner: Main camera not found!");
            TurnOffVisualizer();
            return;
        }
    }

    void Update()
    {
        if (!enemy.IsActive || player == null || (radiusSprite == null && angleSprite == null && visibleSprite == null) || mainCamera == null)
            return;

        // Проверяем состояние игрока относительно врага
        CheckPlayerVisibilityStates();

        // Логика спавна и уничтожения спрайтов
        UpdateSprites();
    }

    void CheckPlayerVisibilityStates()
    {
        isPlayerInRadius = false;
        isPlayerInAngle = false;
        if (player == null || enemyVision.visionPoint == null) return;

        Vector3 playerCenter = GetPlayerColliderCenter();
        Vector3 toPlayer = playerCenter - enemyVision.visionPoint.position;
        float sqrDistance = toPlayer.sqrMagnitude;

        // Проверяем, находится ли игрок в радиусе видимости
        isPlayerInRadius = sqrDistance <= enemyVision.viewRadius * enemyVision.viewRadius;
        if (!isPlayerInRadius) return;

        // Проверяем, находится ли игрок в угле обзора
        Vector3 directionToPlayer = toPlayer.normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
        isPlayerInAngle = dotProduct >= Mathf.Cos(enemyVision.viewAngle * 0.5f * Mathf.Deg2Rad) || enemyVision.enemyTouchesPlayer;
    }

    void UpdateSprites()
    {
        // Сбрасываем все спрайты, если игрок не в радиусе
        if (!isPlayerInRadius)
        {
            DestroyAllSprites();
            return;
        }

        // Приоритет: visibleSprite > angleSprite > radiusSprite
        if (enemyVision.IsPlayerVisible)
        {
            // Игрок виден: спавним visibleSprite, уничтожаем остальные
            if (visibleSpriteObject == null && visibleSprite != null)
            {
                SpawnVisibleSprite();
            }
            DestroyRadiusSprite();
            DestroyAngleSprite();
        }
        else if (isPlayerInAngle)
        {
            // Игрок в зоне видимости (радиус + угол): спавним angleSprite, уничтожаем остальные
            if (angleSpriteObject == null && angleSprite != null)
            {
                SpawnAngleSprite();
            }
            DestroyRadiusSprite();
            DestroyVisibleSprite();
        }
        else
        {
            // Игрок только в радиусе: спавним radiusSprite, уничтожаем остальные
            if (radiusSpriteObject == null && radiusSprite != null)
            {
                SpawnRadiusSprite();
            }
            DestroyAngleSprite();
            DestroyVisibleSprite();
        }

        // Обновляем позиции и ориентацию спрайтов
        UpdateSpritePositionsAndRotation();
    }

    void SpawnRadiusSprite()
    {
        if (radiusSprite == null) return;

        Vector3 enemyCenter = GetEnemyColliderCenter();
        Vector3 spawnPosition = enemyCenter + Vector3.up * spriteHeightOffset;

        radiusSpriteObject = new GameObject("RadiusSprite");
        SpriteRenderer renderer = radiusSpriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = radiusSprite;
        renderer.color = spriteColor;
        radiusSpriteObject.transform.position = spawnPosition;
        radiusSpriteObject.transform.localScale = spriteScale;
        renderer.sortingOrder = 10;

        OrientSpriteToCamera(radiusSpriteObject);

        //Debug.Log("SpriteSpawner: Radius sprite spawned above enemy");
    }

    void SpawnAngleSprite()
    {
        if (angleSprite == null) return;

        Vector3 enemyCenter = GetEnemyColliderCenter();
        Vector3 spawnPosition = enemyCenter + Vector3.up * spriteHeightOffset;

        angleSpriteObject = new GameObject("AngleSprite");
        SpriteRenderer renderer = angleSpriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = angleSprite;
        renderer.color = spriteColor;
        angleSpriteObject.transform.position = spawnPosition;
        angleSpriteObject.transform.localScale = spriteScale;
        renderer.sortingOrder = 10;

        OrientSpriteToCamera(angleSpriteObject);

        //Debug.Log("SpriteSpawner: Angle sprite spawned above enemy");
    }

    void SpawnVisibleSprite()
    {
        if (visibleSprite == null) return;

        Vector3 enemyCenter = GetEnemyColliderCenter();
        Vector3 spawnPosition = enemyCenter + Vector3.up * spriteHeightOffset;

        visibleSpriteObject = new GameObject("VisibleSprite");
        SpriteRenderer renderer = visibleSpriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = visibleSprite;
        renderer.color = spriteColor;
        visibleSpriteObject.transform.position = spawnPosition;
        visibleSpriteObject.transform.localScale = spriteScale;
        renderer.sortingOrder = 10;

        OrientSpriteToCamera(visibleSpriteObject);

        //Debug.Log("SpriteSpawner: Visible sprite spawned above enemy");
    }

    void OrientSpriteToCamera(GameObject spriteObject)
    {
        if (spriteObject == null || mainCamera == null) return;

        spriteObject.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
    }

    void UpdateSpritePositionsAndRotation()
    {
        Vector3 targetPosition = GetEnemyColliderCenter() + Vector3.up * spriteHeightOffset;

        if (radiusSpriteObject != null)
        {
            radiusSpriteObject.transform.position = targetPosition;
            OrientSpriteToCamera(radiusSpriteObject);
        }

        if (angleSpriteObject != null)
        {
            angleSpriteObject.transform.position = targetPosition;
            OrientSpriteToCamera(angleSpriteObject);
        }

        if (visibleSpriteObject != null)
        {
            visibleSpriteObject.transform.position = targetPosition;
            OrientSpriteToCamera(visibleSpriteObject);
        }
    }

    void DestroyRadiusSprite()
    {
        if (radiusSpriteObject != null)
        {
            Destroy(radiusSpriteObject);
            radiusSpriteObject = null;
            //Debug.Log("SpriteSpawner: Radius sprite destroyed");
        }
    }

    void DestroyAngleSprite()
    {
        if (angleSpriteObject != null)
        {
            Destroy(angleSpriteObject);
            angleSpriteObject = null;
            //Debug.Log("SpriteSpawner: Angle sprite destroyed");
        }
    }

    void DestroyVisibleSprite()
    {
        if (visibleSpriteObject != null)
        {
            Destroy(visibleSpriteObject);
            visibleSpriteObject = null;
            //Debug.Log("SpriteSpawner: Visible sprite destroyed");
        }
    }

    Vector3 GetPlayerColliderCenter()
    {
        Collider playerCollider = player.GetComponent<Collider>();
        PlayerController playerController = player.GetComponent<PlayerController>();

        float verticalOffset = 0.5f; // Значение из EnemyVision
        if (playerController.currentState == PlayerController.PlayerMovementState.down)
        {
            return playerCollider.bounds.center + Vector3.up * verticalOffset;
        }
        return playerCollider.bounds.center;
    }

    Vector3 GetEnemyColliderCenter()
    {
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            return enemyCollider.bounds.center;
        }
        return transform.position;
    }

    void OnDestroy()
    {
        DestroyAllSprites();
    }

    private void DestroyAllSprites()
    {
        DestroyRadiusSprite();
        DestroyAngleSprite();
        DestroyVisibleSprite();
    }

    private void TurnOffVisualizer()
    {
        enabled = false;
    }
}
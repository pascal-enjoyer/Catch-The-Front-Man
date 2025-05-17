using UnityEngine;

public class EnemyVisionVizualizer : MonoBehaviour
{
    [SerializeField] private EnemyVision enemyVision; // ������ �� ��������� EnemyVision
    [SerializeField] private Sprite radiusSprite; // ������ ��� ������� ���������
    [SerializeField] private Sprite angleSprite; // ������ ��� ���� ��������� (� ������� � � ���� ������)
    [SerializeField] private Sprite visibleSprite; // ������ ��� ������ ���������
    [SerializeField] private float spriteHeightOffset = 1.5f; // ������ ������ ��� ������
    [SerializeField] private Vector2 spriteScale = Vector2.one; // ������� ��������
    [SerializeField] private Color spriteColor = Color.white; // ���� ��������

    private GameObject radiusSpriteObject; // ������ ��� ������� �������
    private GameObject angleSpriteObject; // ������ ��� ������� ���� ���������
    private GameObject visibleSpriteObject; // ������ ��� ������� ������ ���������
    private GameObject player => enemyVision.player; // �������� ������ �� EnemyVision
    private bool isPlayerInRadius = false; // ���� ���������� ������ � �������
    private bool isPlayerInAngle = false; // ���� ���������� ������ � ���� ������
    private Camera mainCamera; // ������ �� �������� ������
    private Enemy enemy; // ������ �� ��������� Enemy

    void Start()
    {
        // ��������� ����������� ����������
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

        // ������������� �� ������� ������ �����
        enemy.EnemyDie.AddListener(DestroyAllSprites);
        enemy.EnemyDie.AddListener(TurnOffVisualizer);

        // ��������� ���������� ��������
        if (radiusSprite == null || angleSprite == null || visibleSprite == null)
        {
           // Debug.LogWarning("SpriteSpawner: One or more sprites not assigned!");
        }

        // �������� �������� ������
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

        // ��������� ��������� ������ ������������ �����
        CheckPlayerVisibilityStates();

        // ������ ������ � ����������� ��������
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

        // ���������, ��������� �� ����� � ������� ���������
        isPlayerInRadius = sqrDistance <= enemyVision.viewRadius * enemyVision.viewRadius;
        if (!isPlayerInRadius) return;

        // ���������, ��������� �� ����� � ���� ������
        Vector3 directionToPlayer = toPlayer.normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
        isPlayerInAngle = dotProduct >= Mathf.Cos(enemyVision.viewAngle * 0.5f * Mathf.Deg2Rad) || enemyVision.enemyTouchesPlayer;
    }

    void UpdateSprites()
    {
        // ���������� ��� �������, ���� ����� �� � �������
        if (!isPlayerInRadius)
        {
            DestroyAllSprites();
            return;
        }

        // ���������: visibleSprite > angleSprite > radiusSprite
        if (enemyVision.IsPlayerVisible)
        {
            // ����� �����: ������� visibleSprite, ���������� ���������
            if (visibleSpriteObject == null && visibleSprite != null)
            {
                SpawnVisibleSprite();
            }
            DestroyRadiusSprite();
            DestroyAngleSprite();
        }
        else if (isPlayerInAngle)
        {
            // ����� � ���� ��������� (������ + ����): ������� angleSprite, ���������� ���������
            if (angleSpriteObject == null && angleSprite != null)
            {
                SpawnAngleSprite();
            }
            DestroyRadiusSprite();
            DestroyVisibleSprite();
        }
        else
        {
            // ����� ������ � �������: ������� radiusSprite, ���������� ���������
            if (radiusSpriteObject == null && radiusSprite != null)
            {
                SpawnRadiusSprite();
            }
            DestroyAngleSprite();
            DestroyVisibleSprite();
        }

        // ��������� ������� � ���������� ��������
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

        float verticalOffset = 0.5f; // �������� �� EnemyVision
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
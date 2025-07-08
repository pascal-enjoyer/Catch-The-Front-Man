using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Enemy))]
public class EnemyEars : MonoBehaviour, IEarsComponent, IObjectsHear
{
    [SerializeField] private GameObject distractionSpritePrefab;
    [SerializeField] private float distractionDuration = 5f;
    [SerializeField] private float spriteHeightOffset = 2f;
    [SerializeField] private float rotationSpeed = 5f; // Добавляем поле для rotationSpeed

    private Enemy enemy;
    private GameObject spriteInstance;
    private FillableObject fillableObject;
    private float timer;
    private Vector3 distractionPoint;
    private bool isDistracted;

    public bool IsDistracted(out Vector3 point)
    {
        point = distractionPoint;
        return isDistracted && timer > 0;
    }

    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;

        if (distractionSpritePrefab == null)
            Debug.LogError($"Distraction Sprite Prefab not assigned on {name}!", this);
    }

    public void WatchPoint(Vector3 point)
    {
        if (enemy.IsDead || !enemy.IsActive)
        {
            Debug.Log($"Enemy {name} cannot be distracted: IsDead={enemy.IsDead}, IsActive={enemy.IsActive}");
            return;
        }

        distractionPoint = point;
        StartDistraction(point);
    }

    public void StartDistraction(Vector3 point)
    {
        if (isDistracted) return;

        distractionPoint = point;
        timer = distractionDuration;
        isDistracted = true;

        if (distractionSpritePrefab != null)
        {
            spriteInstance = Instantiate(distractionSpritePrefab,
                transform.position + Vector3.up * spriteHeightOffset,
                Quaternion.identity,
                transform);
            fillableObject = spriteInstance.GetComponent<FillableObject>();
            if (fillableObject != null)
            {
                fillableObject.SetFillColor(Color.red);
                fillableObject.SetFillMethod(Image.FillMethod.Vertical, Image.OriginVertical.Top);
                fillableObject.SetFillAmount(1f);
            }
            else
                Debug.LogError($"Distraction sprite on {name} missing FillableObject component!");
        }
        else
            Debug.LogError($"Distraction Sprite Prefab not assigned on {name}!");

        Debug.Log($"EnemyEars on {name} distracted to look at {point} for {distractionDuration}s");
    }

    public void StopDistraction()
    {
        if (!isDistracted) return;

        isDistracted = false;
        if (spriteInstance != null)
            Destroy(spriteInstance);
        Debug.Log($"EnemyEars on {name} stopped distraction");
        Destroy(this);
    }

    private void Update()
    {
        if (!isDistracted || enemy.IsDead || !enemy.IsActive)
        {
            if (isDistracted)
                StopDistraction();
            return;
        }

        Vector3 direction = (distractionPoint - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        timer -= Time.deltaTime;
        if (fillableObject != null)
            fillableObject.SetFillAmount(timer / distractionDuration);

        if (timer <= 0)
            StopDistraction();
    }
}
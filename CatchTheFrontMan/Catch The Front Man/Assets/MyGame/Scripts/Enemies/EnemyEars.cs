using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;


// Interface for objects that can hear the thrown object's collision
public interface IObjectsHear
{
    void WatchPoint(Vector3 point);
}
[RequireComponent(typeof(Enemy), typeof(EnemyPatrol))]
public class EnemyEars : MonoBehaviour, IObjectsHear
{
    [SerializeField] private GameObject distractionSpritePrefab; // UI Canvas prefab with FillableObject
    [SerializeField] private float distractionDuration = 5f; // Time to look at point
    [SerializeField] private float spriteHeightOffset = 2f; // Height above enemy

    private Enemy enemy;
    private EnemyPatrol patrol;
    private GameObject spriteInstance;
    private FillableObject fillableObject;
    private float timer;
    private Vector3 distractionPoint;
    private bool isDistracted;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        patrol = GetComponent<EnemyPatrol>();

        // Validate setup
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
        timer = distractionDuration;
        isDistracted = true;

        // Spawn sprite with Canvas
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

    void Update()
    {
        if (!isDistracted || enemy.IsDead || !enemy.IsActive)
        {
            if (isDistracted)
                StopDistraction();
            return;
        }

        // Make enemy look at distraction point
        Vector3 direction = (distractionPoint - transform.position).normalized;
        direction.y = 0; // Keep rotation horizontal
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                patrol.rotationSpeed * Time.deltaTime
            );
        }

        // Update timer and sprite fill
        timer -= Time.deltaTime;
        if (fillableObject != null)
            fillableObject.SetFillAmount(timer / distractionDuration);

        if (timer <= 0)
            StopDistraction();
    }

    private void StopDistraction()
    {
        isDistracted = false;
        if (spriteInstance != null)
            Destroy(spriteInstance);
        Debug.Log($"EnemyEars on {name} stopped distraction");
        Destroy(this); // Remove component after distraction
    }
}
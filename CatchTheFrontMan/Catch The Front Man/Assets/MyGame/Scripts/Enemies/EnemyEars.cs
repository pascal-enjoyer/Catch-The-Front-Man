using UnityEngine;
using UnityEngine.UI;
// Interface for objects that can hear the thrown object's collision
public interface IObjectsHear
{
    void WatchPoint(Vector3 point);
}
[RequireComponent(typeof(Enemy), typeof(EnemyStateMachine))]
public class EnemyEars : MonoBehaviour, IObjectsHear
{
    [SerializeField] private GameObject distractionSpritePrefab; // UI Canvas prefab with FillableObject
    [SerializeField] private float distractionDuration = 5f; // Time to look at point
    [SerializeField] private float spriteHeightOffset = 2f; // Height above enemy

    private Enemy _enemy;
    private EnemyStateMachine _stateMachine;

    public GameObject DistractionSpritePrefab => distractionSpritePrefab;
    public float DistractionDuration => distractionDuration;
    public float SpriteHeightOffset => spriteHeightOffset;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _stateMachine = GetComponent<EnemyStateMachine>();

        if (distractionSpritePrefab == null)
            Debug.LogError($"Distraction Sprite Prefab not assigned on {name}!", this);
    }

    public void WatchPoint(Vector3 point)
    {
        if (_enemy.IsDead || !_enemy.IsActive)
        {
            Debug.Log($"Enemy {name} cannot be distracted: IsDead={_enemy.IsDead}, IsActive={_enemy.IsActive}");
            return;
        }

        _stateMachine.WatchPoint(point);
        Debug.Log($"EnemyEars on {name} triggered distraction to look at {point} for {distractionDuration}s");
    }
}
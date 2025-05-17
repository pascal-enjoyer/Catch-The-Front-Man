using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifetime = 2f;
    public int damage = 1;
    [SerializeField] private bool isDeadly = true;

    [Header("Shrink Settings")]
    [SerializeField] private int maxCollisions = 3; // Максимальное количество столкновений
    [SerializeField] private float shrinkSpeed = 2f; // Скорость уменьшения (единиц в секунду)
    [SerializeField] private float minScale = 0.1f; // Минимальный масштаб
    [SerializeField] private float shrinkDelay = 0.2f; // Задержка перед уменьшением

    private Rigidbody rb;
    private int remainingCollisions;
    private Vector3 initialScale;
    private Coroutine shrinkCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing on Bullet!");
        }

        remainingCollisions = maxCollisions;
        initialScale = transform.localScale;
    }

    public void Initialize(Vector3 velocity)
    {
        if (rb == null) return;

        rb.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDeadly) return;

        // Проверяем, является ли объект игроком
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            if (collision.gameObject.TryGetComponent(out PlayerInvincibility playerInvincibility)
                && playerInvincibility.IsInvincible)
                return;

            playerController.Die();
            Destroy(this.gameObject);
        }
        else
        {
            // Уменьшаем при столкновении с не-игроком
            HandleNonPlayerCollision();
        }
    }

    private void HandleNonPlayerCollision()
    {
        remainingCollisions--;

        // Запускаем уменьшение, если еще не запущено
        if (shrinkCoroutine == null)
        {
            shrinkCoroutine = StartCoroutine(ShrinkRoutine());
        }

        // Уничтожаем пулю, если достигнуто максимальное количество столкновений
        if (remainingCollisions <= 0)
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator ShrinkRoutine()
    {
        // Задержка перед уменьшением
        yield return new WaitForSeconds(shrinkDelay);

        Vector3 targetScale = Vector3.one * minScale;
        float elapsedTime = 0f;

        while (transform.localScale.x > minScale)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * shrinkSpeed;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            // Уничтожаем, если масштаб достиг минимального
            if (transform.localScale.x <= minScale)
            {
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    private void OnDestroy()
    {
        // Останавливаем корутину, чтобы избежать утечек
        if (shrinkCoroutine != null)
        {
            StopCoroutine(shrinkCoroutine);
        }
    }
}
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2f;
    public int damage = 1;
    [SerializeField] private bool isDeadly = true;

    private Rigidbody rb;

    public void Initialize(Vector3 velocity)
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDeadly) return;
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            if (collision.gameObject.TryGetComponent(out PlayerInvincibility playerInvincibility)
                && playerInvincibility.IsInvincible)
                return;

            playerController.Die();
            Destroy(gameObject);
        }
    }
}
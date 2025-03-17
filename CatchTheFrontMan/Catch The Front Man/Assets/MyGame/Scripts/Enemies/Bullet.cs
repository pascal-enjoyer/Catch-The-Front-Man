using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    public int damage = 1;

    [SerializeField] private bool isDeadly = true;

    private Rigidbody rb;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDeadly) return;
        if (collision.gameObject.TryGetComponent(out PlayerController playerController) )
        {
            playerController.Die();

        }

        Destroy(gameObject);
    }

}
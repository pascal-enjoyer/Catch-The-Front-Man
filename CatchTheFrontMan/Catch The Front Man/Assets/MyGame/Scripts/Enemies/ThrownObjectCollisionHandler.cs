using UnityEngine;

public class ThrownObjectCollisionHandler : MonoBehaviour
{
    private float sphereCastRadius;
    private LayerMask hearableMask;
    private bool hasCollided;

    public void Setup(float radius, LayerMask mask)
    {
        sphereCastRadius = radius;
        hearableMask = mask;
        hasCollided = false;
        Debug.Log($"Collision handler setup on {name} with radius {radius} and mask {mask}");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        hasCollided = true;
        Vector3 collisionPoint = collision.GetContact(0).point;
        Debug.Log($"Thrown object {name} collided at {collisionPoint}");

        // Perform sphere cast to detect IObjectsHear objects
        Collider[] colliders = Physics.OverlapSphere(collisionPoint, sphereCastRadius, hearableMask);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IObjectsHear>(out var hearable))
            {
                Debug.Log($"Detected IObjectsHear on {collider.name}. Calling WatchPoint.");
                hearable.WatchPoint(collisionPoint);
            }
        }

        // Destroy this script after processing
        Destroy(this);
    }
}
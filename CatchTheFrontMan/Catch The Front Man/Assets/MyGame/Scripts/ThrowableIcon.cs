// Separate class for handling sprite click
using UnityEngine;


public class ThrowableIcon : MonoBehaviour
{
    private GameObject targetObject;
    private ObjectsThrowZone throwZone;

    public void Setup(GameObject target, ObjectsThrowZone zone)
    {
        targetObject = target;
        throwZone = zone;

        // Make sprite face camera (optional, for 2D sprites)
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        // Ensure collider exists
        if (!TryGetComponent<Collider>(out var collider))
        {
            Debug.LogWarning($"No Collider found on icon {name}. Adding SphereCollider.", this);
            collider = gameObject.AddComponent<SphereCollider>();
            (collider as SphereCollider).radius = 0.5f; // Adjust size as needed
            collider.isTrigger = true;
        }
        else
        {
            Debug.Log($"Collider found on icon {name}: {collider.GetType().Name}");
        }
    }

    public void OnClick()
    {
        if (targetObject != null && throwZone != null)
        {
            // Trigger throw and destroy the icon
            throwZone.ThrowObject(targetObject);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError($"OnClick failed: targetObject or throwZone is null on {name}");
        }
    }
}
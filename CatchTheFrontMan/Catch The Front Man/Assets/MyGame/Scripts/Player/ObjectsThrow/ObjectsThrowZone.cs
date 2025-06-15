using UnityEngine;

public class ObjectsThrowZone : MonoBehaviour
{
    [SerializeField] private float objectsDetectRadius = 5f; // Configurable radius in Inspector
    [SerializeField] private GameObject iconPrefab; // Sprite prefab to show above object
    [SerializeField] private LayerMask throwableMask; // Layer mask for throwable objects
    [SerializeField] private float throwForce = 10f; // Force to apply when throwing
    [SerializeField] private float upwardForce = 5f; // Upward force for arc trajectory
    [SerializeField] private LayerMask iconLayerMask; // Layer mask for icon raycast detection
    [SerializeField] private float sphereCastRadius = 2f; // Radius for sphere cast on collision
    [SerializeField] private LayerMask hearableMask; // Layer mask for objects with IObjectsHear

    private SphereCollider zoneCollider;
    private Camera mainCamera;

    void Start()
    {
        // Initialize sphere collider
        zoneCollider = gameObject.AddComponent<SphereCollider>();
        zoneCollider.radius = objectsDetectRadius;
        zoneCollider.isTrigger = true;

        // Cache main camera
        mainCamera = Camera.main;

        // Validate setup
        if (iconPrefab == null)
            Debug.LogError("Icon Prefab is not assigned in ObjectsThrowZone!", this);
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!", this);
        if (iconLayerMask.value == 0)
            Debug.LogWarning("Icon Layer Mask is not set! Raycasts may miss icons.", this);
        if (hearableMask.value == 0)
            Debug.LogWarning("Hearable Mask is not set! Sphere cast may miss IObjectsHear objects.", this);
    }

    void Update()
    {
        // Process all inputs (mouse and touches)
        HandleInput();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object is in the throwable layer
        if (((1 << other.gameObject.layer) & throwableMask) != 0)
        {
            Debug.Log($"Throwable object detected: {other.name}");

            // Ensure the object has a Rigidbody
            Rigidbody rb = other.GetComponent<Rigidbody>() ?? other.gameObject.AddComponent<Rigidbody>();

            // Instantiate sprite above object
            GameObject icon = Instantiate(iconPrefab,
                other.transform.position + new Vector3(0, 1, 0),
                Quaternion.identity,
                other.transform);

            // Get the first layer from the LayerMask
            int iconLayer = Mathf.RoundToInt(Mathf.Log(iconLayerMask.value, 2));
            if (iconLayer >= 0 && iconLayer <= 31)
            {
                icon.layer = iconLayer;
                Debug.Log($"Set icon layer to: {LayerMask.LayerToName(iconLayer)}");
            }
            else
            {
                Debug.LogError($"Invalid icon layer: {iconLayer}. Ensure Icon Layer Mask is set to a single valid layer.", this);
            }

            // Add click handler component to the sprite
            icon.AddComponent<ThrowableIcon>().Setup(other.gameObject, this);

            Debug.Log($"Icon instantiated for {other.name} at {icon.transform.position}");
        }
    }

    void HandleInput()
    {
        // Process mouse input as a "virtual touch" with index -1
        Vector2[] inputPositions = new Vector2[Input.touchCount + 1];
        bool[] inputBegan = new bool[Input.touchCount + 1];

        // Add mouse input if present
        inputPositions[0] = Input.mousePosition;
        inputBegan[0] = Input.GetMouseButtonDown(0);

        // Add touch inputs
        for (int i = 0; i < Input.touchCount; i++)
        {
            inputPositions[i + 1] = Input.GetTouch(i).position;
            inputBegan[i + 1] = Input.GetTouch(i).phase == TouchPhase.Began;
        }

        // Process all inputs
        for (int i = 0; i < inputPositions.Length; i++)
        {
            if (inputBegan[i])
            {
                Ray ray = mainCamera.ScreenPointToRay(inputPositions[i]);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, iconLayerMask))
                {
                    if (hit.collider.TryGetComponent<ThrowableIcon>(out var icon))
                    {
                        Debug.Log($"Icon clicked/tapped: {hit.collider.name}");
                        icon.OnClick();
                    }
                    else
                        Debug.Log($"Raycast hit {hit.collider.name} but no ThrowableIcon component found.");
                }
                else
                    Debug.Log("Raycast missed any objects.");
            }
        }
    }

    public void ThrowObject(GameObject obj)
    {
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            // Calculate forward direction from the zone's transform
            Vector3 throwDirection = transform.forward;
            // Apply force with slight upward component for arc
            rb.AddForce(throwDirection * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);
            Debug.Log($"Threw object {obj.name} with force {throwForce} and upward force {upwardForce}");

            // Add collision handler script
            var collisionHandler = obj.AddComponent<ThrownObjectCollisionHandler>();
            collisionHandler.Setup(sphereCastRadius, hearableMask);
        }
    }
}

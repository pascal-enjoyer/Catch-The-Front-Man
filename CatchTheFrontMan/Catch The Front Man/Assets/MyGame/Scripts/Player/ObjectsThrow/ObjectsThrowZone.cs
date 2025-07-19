using UnityEngine;
using System.Collections.Generic;

public class ObjectsThrowZone : MonoBehaviour
{
    [SerializeField] private float objectsDetectRadius = 5f; // ������ ����������� ��������
    [SerializeField] private LayerMask throwableMask; // ���� ��� ����������� ��������
    [SerializeField] private float throwForce = 10f; // ���� ������
    [SerializeField] private float upwardForce = 5f; // ���� ������� ��� ����
    [SerializeField] private float sphereCastRadius = 2f; // ������ ��� ������ IObjectsHear
    [SerializeField] private LayerMask hearableMask; // ���� ��� �������� � IObjectsHear
    [SerializeField] private string thrownObjectNewMask; // ����� ���� ��� �������� � ����

    private SphereCollider _zoneCollider;
    private Camera _mainCamera;
    private Dictionary<GameObject, LayerMask> _throwableObjectsInZone = new Dictionary<GameObject, LayerMask>(); // ������ �������� � ����

    void Start()
    {
        // ������������� ���������� ����
        _zoneCollider = gameObject.AddComponent<SphereCollider>();
        _zoneCollider.radius = objectsDetectRadius;
        _zoneCollider.isTrigger = true;

        // �������� ������
        _mainCamera = Camera.main;
        if (_mainCamera == null)
            Debug.LogError("Main Camera not found!", this);
        if (throwableMask.value == 0)
            Debug.LogWarning("Throwable Mask is not set! Raycasts may miss objects.", this);
        if (hearableMask.value == 0)
            Debug.LogWarning("Hearable Mask is not set! Sphere cast may miss IObjectsHear objects.", this);

        if (string.IsNullOrEmpty(thrownObjectNewMask) || LayerMask.GetMask(thrownObjectNewMask) == 0)
            Debug.LogError("Thrown Object New Mask is invalid or not set!", this);
    }

    void Update()
    {
        // ������������ ����
        HandleInput();
    }

    void OnTriggerEnter(Collider other)
    {
        // ���������, ��������� �� ������ � ���� throwableMask
        if (((1 << other.gameObject.layer) & throwableMask) != 0)
        {
            Debug.Log($"Throwable object detected: {other.name}");

            // ��������, ��� � ������� ���� Rigidbody
            if (!other.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = other.gameObject.AddComponent<Rigidbody>();
            }

            // ��������� ��������� ��� ��������� ������ � ���������
            var handler = other.gameObject.AddComponent<ThrowableObjectHandler>();
            handler.Setup(this);

            // ��������� ������� ���� � ������ �� �����
            LayerMask previousMask = other.gameObject.layer;
            _throwableObjectsInZone.Add(other.gameObject, previousMask);
            if (LayerMask.GetMask(thrownObjectNewMask) == 0)
            {
                Debug.LogError($"Invalid layer mask '{thrownObjectNewMask}' for object {other.name}!", this);
            }
            else
            {
                other.gameObject.layer = LayerMask.NameToLayer(thrownObjectNewMask);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // ���������, ������� �� ������ �� ����
        if (_throwableObjectsInZone.ContainsKey(other.gameObject))
        {
            if (other.TryGetComponent<ThrowableObjectHandler>(out var handler))
            {
                handler.Highlight(false); // ��������� ���������
                other.gameObject.layer = _throwableObjectsInZone[other.gameObject]; // ��������������� ����
                _throwableObjectsInZone.Remove(other.gameObject);
                Debug.Log($"Object {other.name} exited throw zone, highlight disabled.");
            }
        }
    }

    void HandleInput()
    {
        // �������� ������� ����� (���� � �������)
        Vector2[] inputPositions = new Vector2[Input.touchCount + 1];
        bool[] inputBegan = new bool[Input.touchCount + 1];

        // ���� ��� "����������� �������" � �������� -1
        inputPositions[0] = Input.mousePosition;
        inputBegan[0] = Input.GetMouseButtonDown(0);

        // �������
        for (int i = 0; i < Input.touchCount; i++)
        {
            inputPositions[i + 1] = Input.GetTouch(i).position;
            inputBegan[i + 1] = Input.GetTouch(i).phase == TouchPhase.Began;
        }

        // ������������ �����
        for (int i = 0; i < inputPositions.Length; i++)
        {
            if (inputBegan[i])
            {
                Ray ray = _mainCamera.ScreenPointToRay(inputPositions[i]);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(thrownObjectNewMask)))
                {
                    // ���������, ��������� �� ������ � ����
                    if (_throwableObjectsInZone.ContainsKey(hit.collider.gameObject))
                    {
                        if (hit.collider.TryGetComponent<ThrowableObjectHandler>(out var handler))
                        {
                            Debug.Log($"Object clicked/tapped: {hit.collider.name}");
                            handler.OnClick();
                        }
                        else
                        {
                            Debug.Log($"Raycast hit {hit.collider.name} but no ThrowableObjectHandler component found.");
                        }
                    }
                    else
                    {
                        Debug.Log($"Raycast hit {hit.collider.name} but object is not in throw zone.");
                    }
                }
                else
                {
                    Debug.Log("Raycast missed any objects.");
                }
            }
        }
    }

    public void ThrowObject(GameObject obj)
    {
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            // ������������ ����������� ������
            Vector3 throwDirection = transform.forward;
            rb.AddForce(throwDirection * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);
            Debug.Log($"Threw object {obj.name} with force {throwForce} and upward force {upwardForce}");

            // ��������� ���������� ������������
            var collisionHandler = obj.AddComponent<ThrownObjectCollisionHandler>();
            collisionHandler.Setup(sphereCastRadius, hearableMask);

        }
    }
}
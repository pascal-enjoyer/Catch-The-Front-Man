using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.UI;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager Instance;

    public GameObject playerPrefab;

    public GameObject currentPlayer;

    public MovementButtonsController movementButtonsController;

    [SerializeField] private CameraMovement cameraController; // ������ �� ���������� ������

    public static UnityEvent<GameObject> PlayerChanged = new ();
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentPlayer = Instantiate(playerPrefab, transform);
        PlayerChanged.Invoke(currentPlayer);

    }

}

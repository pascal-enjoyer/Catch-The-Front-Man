using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // �������� �������� ������
    [SerializeField] private float rayDistance = 5f; // ����� ����
    [SerializeField] private LayerMask interactableLayer; // ���� ��� �������� ��������

    public PlayerAnimationManager animatorController;

    // ��������� ������
    private enum PlayerState
    {
        Center, // � ������
        Left,   // �����
        Right   // ������
    }

    private PlayerState currentState = PlayerState.Center; // ������� ��������� ������
    private bool isMovingForward = false; // �������� �� ����� ������
    [SerializeField] private Vector3 targetPosition; // ������� ������� ��� �����������
    private bool isMovingToSide = false; // ������������ �� ����� � �������
    private bool isMovingToCenter = false;
    private bool isPaused = true;


    private void Update()
    {
        if (isMovingForward)
        {

            // �������� ������
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            animatorController.ChangeAnimation("Crouch Walk");

            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (isMovingToSide)
        {
            if (!isMovingToCenter)
            {
                transform.LookAt(new Vector3(0, transform.position.y, transform.position.z));
                animatorController.ChangeAnimation("Wall Lean");
            }
            // ������� ����������� � ������� �������
            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(targetPosition.x, 0, transform.position.z), moveSpeed * Time.deltaTime);

            // ���� ����� ������ ������� �������, ��������� ���������
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                UpdatePlayerState();
            }
        }
    }

    // ����� ��� ���������� ���� � ��������� �����������
    public void CastRay(Vector3 direction)
    {
        if (isMovingToSide || isPaused) return; // ���� ��� ������������ � �������, ����������

        // ���������, ����� �� ��������� � ��������� �����������
        if ((direction == Vector3.left && currentState == PlayerState.Left) ||
            (direction == Vector3.right && currentState == PlayerState.Right))
        {
            return; // ����� ��� ��������� � ���� �������
        }

        if ((direction == Vector3.right && currentState == PlayerState.Left) ||
            (direction == Vector3.left && currentState == PlayerState.Right))
        {
            isMovingToCenter = true;
            ReturnToCenter();
            return;
        }


        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, rayDistance, interactableLayer))
        {
            
            // ������������� �������� ������
            isMovingForward = false;
            isMovingToCenter = false;
            isMovingToSide = true;

            // ������������� ������� �������
            targetPosition = new Vector3(hit.point.x, 0, hit.point.z);
        }

    }

    // ���������� ��������� ������
    private void UpdatePlayerState()
    {
        isMovingToSide = false; 
        if (targetPosition.x < 0)
        {
            
            currentState = PlayerState.Left; // ����� �����
        }
        else if (targetPosition.x > 0)
        {
            currentState = PlayerState.Right; // ����� ������
        }
        else
        {
            currentState = PlayerState.Center; // ����� � ������
            StartMovingForward();
        }

    }



    // ����������� ������ � �����
    private void ReturnToCenter()
    {
        isMovingToSide = true;
        targetPosition = new Vector3(0, 0, transform.position.z); // ������������ � ������ �� X
        currentState = PlayerState.Center;
    }

    // ����� ��� ������ �������� ������
    public void StartMovingForward()
    {
        isMovingToCenter = false;
        isPaused = false;
        isMovingForward = true;
        isMovingToSide = false;
    }
}
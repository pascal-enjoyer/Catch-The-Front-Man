using UnityEngine;
using UnityEngine.Events;

public class CameraMovement : MonoBehaviour
{
    // ������������ ��� ��������� ������
    private enum CameraState
    {
        Idle,           // ������ � �����
        Waiting,        // ������ ���� ����� ������� ��������
        MovingToStart,  // ������ ��������� ������������ �� ��������� �������
        MovingToEnd     // ������ ������ ������������ �� �������� �������
    }

    [SerializeField] private Transform startPosition; // ��������� ������� ������
    [SerializeField] private Transform endPosition;   // �������� ������� ������
    [SerializeField] private float moveSpeed = 1.0f;  // �������� ����������� ������
    [SerializeField] private float delayBeforeMove = 2.0f; // ����� �������� ����� ������� ��������

    // �������, ���������� ��� ���������� �������� �����
    public UnityEvent<Vector3> onReachedEndPosition;

    private CameraState currentState = CameraState.Idle;
    private Transform cameraTransform;
    private float waitTimer;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        StartCameraMovement();
    }

    private void Update()
    {
        // ��������� ��������� ������
        switch (currentState)
        {
            case CameraState.Waiting:
                cameraTransform.position = startPosition.position;
                // �������� ����� ������� ��������
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    currentState = CameraState.MovingToStart;
                }
                break;

            case CameraState.MovingToStart:
                // ���������� ����������� �� ��������� �������
                cameraTransform.position = startPosition.position;
                cameraTransform.rotation = startPosition.rotation;
                currentState = CameraState.MovingToEnd;
                break;

            case CameraState.MovingToEnd:
                // ������� ����������� �� �������� �������
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, endPosition.position, moveSpeed * Time.deltaTime);
                cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, endPosition.rotation, moveSpeed * Time.deltaTime);

                // �������� �� ���������� �����������
                if (Vector3.Distance(cameraTransform.position, endPosition.position) < 0.01f)
                {
                    cameraTransform.position = endPosition.position; // ������ ����������������
                    cameraTransform.rotation = endPosition.rotation;
                    currentState = CameraState.Idle;

                    // ����� ������� � ��������� ������� ������
                    onReachedEndPosition?.Invoke(cameraTransform.position);
                }
                break;
        }
    }

    // ����� ��� ������� �������� ������
    public void StartCameraMovement()
    {
        if (currentState == CameraState.Idle)
        {
            currentState = CameraState.Waiting;
            waitTimer = delayBeforeMove;
        }
    }
}
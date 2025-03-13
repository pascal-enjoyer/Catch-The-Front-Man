using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraController; // ������ �� ���������� ������
    [SerializeField] private PlayerController playerController; // ������ �� ���������� ������
    
    private void Start()
    {
        // ������������� �� ������� ���������� �������� ����� �������
        cameraController.onReachedEndPosition.AddListener(OnCameraReachedEnd);
    }

    // �����, ���������� ��� ���������� ������� �������� �����
    private void OnCameraReachedEnd(Vector3 position)
    {
        // �������� �������� ������ ������
        playerController.StartMovingForward();

        // ����������� ������ ��� ���������� �� �������
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.gameObject.AddComponent<CameraFollow>();
        }
        cameraFollow.SetPlayer(playerController.transform);
    }
}
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // ������ �� ������
    [SerializeField] private Vector3 offset; // �������� ������ ������������ ������

    private void LateUpdate()
    {
        if (player == null) return;
        // ������ ������� �� ������� ������ �� ��� Z
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, player.position.z + offset.z);
        transform.position = targetPosition;
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }
}

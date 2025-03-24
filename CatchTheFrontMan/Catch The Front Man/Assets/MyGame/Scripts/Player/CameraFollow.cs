using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player => PlayerManager.Instance.currentPlayer.transform; // ������ �� ������
    [SerializeField] private Vector3 offset; // �������� ������ ������������ ������

    private void LateUpdate()
    {
        if (player == null) return;
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, player.position.z + offset.z);
        transform.position = targetPosition;
    }

}

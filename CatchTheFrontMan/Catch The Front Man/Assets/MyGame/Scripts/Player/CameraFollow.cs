using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // Ссылка на игрока
    [SerializeField] private Vector3 offset; // Смещение камеры относительно игрока

    private void LateUpdate()
    {
        if (player == null) return;
        // Камера следует за игроком только по оси Z
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, player.position.z + offset.z);
        transform.position = targetPosition;
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }
}

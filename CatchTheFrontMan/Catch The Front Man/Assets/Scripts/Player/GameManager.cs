using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraController; // Ссылка на контроллер камеры
    [SerializeField] private PlayerMovement playerController; // Ссылка на контроллер игрока

    private void Start()
    {
        // Подписываемся на событие достижения конечной точки камерой
        cameraController.onReachedEndPosition.AddListener(OnCameraReachedEnd);
    }

    // Метод, вызываемый при достижении камерой конечной точки
    private void OnCameraReachedEnd(Vector3 position)
    {
        // Начинаем движение игрока вперед
        playerController.StartMovingForward();

        // Настраиваем камеру для следования за игроком
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.gameObject.AddComponent<CameraFollow>();
        }
        cameraFollow.SetPlayer(playerController.transform);
    }
}
using UnityEngine;
using UnityEngine.Events;

public class CameraMovement : MonoBehaviour
{
    // Перечисление для состояний камеры
    private enum CameraState
    {
        Idle,           // Камера в покое
        Waiting,        // Камера ждет перед началом движения
        MovingToStart,  // Камера мгновенно перемещается на начальную позицию
        MovingToEnd     // Камера плавно перемещается на конечную позицию
    }

    [SerializeField] private Transform startPosition; // Начальная позиция камеры
    [SerializeField] private Transform endPosition;   // Конечная позиция камеры
    [SerializeField] private float moveSpeed = 1.0f;  // Скорость перемещения камеры
    [SerializeField] private float delayBeforeMove = 2.0f; // Время ожидания перед началом движения

    // Событие, вызываемое при достижении конечной точки
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
        // Обработка состояний камеры
        switch (currentState)
        {
            case CameraState.Waiting:
                cameraTransform.position = startPosition.position;
                // Ожидание перед началом движения
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    currentState = CameraState.MovingToStart;
                }
                break;

            case CameraState.MovingToStart:
                // Мгновенное перемещение на начальную позицию
                cameraTransform.position = startPosition.position;
                cameraTransform.rotation = startPosition.rotation;
                currentState = CameraState.MovingToEnd;
                break;

            case CameraState.MovingToEnd:
                // Плавное перемещение на конечную позицию
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, endPosition.position, moveSpeed * Time.deltaTime);
                cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, endPosition.rotation, moveSpeed * Time.deltaTime);

                // Проверка на завершение перемещения
                if (Vector3.Distance(cameraTransform.position, endPosition.position) < 0.01f)
                {
                    cameraTransform.position = endPosition.position; // Точное позиционирование
                    cameraTransform.rotation = endPosition.rotation;
                    currentState = CameraState.Idle;

                    // Вызов события с передачей позиции камеры
                    onReachedEndPosition?.Invoke(cameraTransform.position);
                }
                break;
        }
    }

    // Метод для запуска движения камеры
    public void StartCameraMovement()
    {
        if (currentState == CameraState.Idle)
        {
            currentState = CameraState.Waiting;
            waitTimer = delayBeforeMove;
        }
    }
}
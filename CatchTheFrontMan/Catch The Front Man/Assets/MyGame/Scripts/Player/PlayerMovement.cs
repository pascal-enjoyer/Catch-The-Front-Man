using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Скорость движения вперед
    [SerializeField] private float rayDistance = 5f; // Длина луча
    [SerializeField] private LayerMask interactableLayer; // Слой для проверки объектов

    public PlayerAnimationManager animatorController;

    // Состояния игрока
    private enum PlayerState
    {
        Center, // В центре
        Left,   // Слева
        Right   // Справа
    }

    private PlayerState currentState = PlayerState.Center; // Текущее состояние игрока
    private bool isMovingForward = false; // Движется ли игрок вперед
    [SerializeField] private Vector3 targetPosition; // Целевая позиция для перемещения
    private bool isMovingToSide = false; // Перемещается ли игрок в сторону
    private bool isMovingToCenter = false;
    private bool isPaused = true;


    private void Update()
    {
        if (isMovingForward)
        {

            // Движение вперед
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
            // Плавное перемещение к целевой позиции
            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(targetPosition.x, 0, transform.position.z), moveSpeed * Time.deltaTime);

            // Если игрок достиг целевой позиции, обновляем состояние
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                UpdatePlayerState();
            }
        }
    }

    // Метод для кастования луча в указанном направлении
    public void CastRay(Vector3 direction)
    {
        if (isMovingToSide || isPaused) return; // Если уже перемещается в сторону, игнорируем

        // Проверяем, можно ли двигаться в указанном направлении
        if ((direction == Vector3.left && currentState == PlayerState.Left) ||
            (direction == Vector3.right && currentState == PlayerState.Right))
        {
            return; // Игрок уже находится в этой стороне
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
            
            // Останавливаем движение вперед
            isMovingForward = false;
            isMovingToCenter = false;
            isMovingToSide = true;

            // Устанавливаем целевую позицию
            targetPosition = new Vector3(hit.point.x, 0, hit.point.z);
        }

    }

    // Обновление состояния игрока
    private void UpdatePlayerState()
    {
        isMovingToSide = false; 
        if (targetPosition.x < 0)
        {
            
            currentState = PlayerState.Left; // Игрок слева
        }
        else if (targetPosition.x > 0)
        {
            currentState = PlayerState.Right; // Игрок справа
        }
        else
        {
            currentState = PlayerState.Center; // Игрок в центре
            StartMovingForward();
        }

    }



    // Возвращение игрока в центр
    private void ReturnToCenter()
    {
        isMovingToSide = true;
        targetPosition = new Vector3(0, 0, transform.position.z); // Возвращаемся к центру по X
        currentState = PlayerState.Center;
    }

    // Метод для начала движения вперед
    public void StartMovingForward()
    {
        isMovingToCenter = false;
        isPaused = false;
        isMovingForward = true;
        isMovingToSide = false;
    }
}
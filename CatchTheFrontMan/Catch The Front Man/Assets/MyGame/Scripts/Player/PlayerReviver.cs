using UnityEngine;
using UnityEngine.Events;

public class PlayerReviver : MonoBehaviour
{
    public float invincibleTime = 3f;

    private PlayerManager playerManager => PlayerManager.Instance;

    private void Update()
    {
        // Обновляем статический таймер
        DeathTimer.UpdateTimer();
    }

    public void RevivePlayer()
    {
        Transform playerPosition = playerManager.currentPlayer.transform;
        PlayerController oldPlayerController = playerManager.currentPlayer.GetComponent<PlayerController>();

        // Сохраняем состояние игрока перед уничтожением
        PlayerController.PlayerMovementState savedState = oldPlayerController.currentState;
        Vector3 savedTargetPosition = oldPlayerController.targetPosition;
        Quaternion savedRotation = playerPosition.rotation;
        bool savedIsMoving = oldPlayerController.isMoving;
        bool savedIsStopped = oldPlayerController.isStopped;
        bool savedIsMovementBlocked = oldPlayerController.isMovementBlocked;

        Debug.Log($"RevivePlayer: Saving state - currentState={savedState}, targetPosition={savedTargetPosition}, rotation={savedRotation.eulerAngles}, isMoving={savedIsMoving}, isStopped={savedIsStopped}, isMovementBlocked={savedIsMovementBlocked}");

        // Уничтожаем старого игрока
        Destroy(playerManager.currentPlayer);

        // Создаём нового игрока
        GameObject player = Instantiate(playerManager.playerPrefab, playerManager.transform);
        player.transform.position = playerPosition.position;
        player.transform.rotation = savedRotation;

        // Восстанавливаем состояние
        PlayerController newPlayerController = player.GetComponent<PlayerController>();
        newPlayerController.currentState = savedState;
        newPlayerController.targetPosition = savedTargetPosition;
        newPlayerController.isMoving = savedIsMoving;
        newPlayerController.isStopped = savedIsStopped;
        newPlayerController.isMovementBlocked = savedIsMovementBlocked;

        // Восстанавливаем настройки в зависимости от состояния
        newPlayerController.RestoreState(savedState, savedTargetPosition);

        // Активируем неуязвимость
        //player.GetComponent<PlayerInvincibility>().ActivateInvincibility(invincibleTime);

        // Обновляем текущего игрока и вызываем событие
        playerManager.currentPlayer = player;
        PlayerManager.PlayerChanged.Invoke(player);

        // Запускаем таймер смерти
        DeathTimer.StartDeathTimer();

        Debug.Log($"RevivePlayer: Restored state - currentState={newPlayerController.currentState}, targetPosition={newPlayerController.targetPosition}, rotation={player.transform.rotation.eulerAngles}, isMoving={newPlayerController.isMoving}, isStopped={newPlayerController.isStopped}, isMovementBlocked={newPlayerController.isMovementBlocked}");
    }
}
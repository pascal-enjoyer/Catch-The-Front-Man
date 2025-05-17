using UnityEngine;
using UnityEngine.Events;

public class PlayerReviver : MonoBehaviour
{
    public float invincibleTime = 3f;

    private PlayerManager playerManager => PlayerManager.Instance;

    private void Update()
    {
        // ��������� ����������� ������
        DeathTimer.UpdateTimer();
    }

    public void RevivePlayer()
    {
        Transform playerPosition = playerManager.currentPlayer.transform;
        PlayerController oldPlayerController = playerManager.currentPlayer.GetComponent<PlayerController>();

        // ��������� ��������� ������ ����� ������������
        PlayerController.PlayerMovementState savedState = oldPlayerController.currentState;
        Vector3 savedTargetPosition = oldPlayerController.targetPosition;
        Quaternion savedRotation = playerPosition.rotation;
        bool savedIsMoving = oldPlayerController.isMoving;
        bool savedIsStopped = oldPlayerController.isStopped;
        bool savedIsMovementBlocked = oldPlayerController.isMovementBlocked;

        Debug.Log($"RevivePlayer: Saving state - currentState={savedState}, targetPosition={savedTargetPosition}, rotation={savedRotation.eulerAngles}, isMoving={savedIsMoving}, isStopped={savedIsStopped}, isMovementBlocked={savedIsMovementBlocked}");

        // ���������� ������� ������
        Destroy(playerManager.currentPlayer);

        // ������ ������ ������
        GameObject player = Instantiate(playerManager.playerPrefab, playerManager.transform);
        player.transform.position = playerPosition.position;
        player.transform.rotation = savedRotation;

        // ��������������� ���������
        PlayerController newPlayerController = player.GetComponent<PlayerController>();
        newPlayerController.currentState = savedState;
        newPlayerController.targetPosition = savedTargetPosition;
        newPlayerController.isMoving = savedIsMoving;
        newPlayerController.isStopped = savedIsStopped;
        newPlayerController.isMovementBlocked = savedIsMovementBlocked;

        // ��������������� ��������� � ����������� �� ���������
        newPlayerController.RestoreState(savedState, savedTargetPosition);

        // ���������� ������������
        //player.GetComponent<PlayerInvincibility>().ActivateInvincibility(invincibleTime);

        // ��������� �������� ������ � �������� �������
        playerManager.currentPlayer = player;
        PlayerManager.PlayerChanged.Invoke(player);

        // ��������� ������ ������
        DeathTimer.StartDeathTimer();

        Debug.Log($"RevivePlayer: Restored state - currentState={newPlayerController.currentState}, targetPosition={newPlayerController.targetPosition}, rotation={player.transform.rotation.eulerAngles}, isMoving={newPlayerController.isMoving}, isStopped={newPlayerController.isStopped}, isMovementBlocked={newPlayerController.isMovementBlocked}");
    }
}
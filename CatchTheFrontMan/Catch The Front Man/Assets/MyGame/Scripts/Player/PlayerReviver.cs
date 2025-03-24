using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class PlayerReviver : MonoBehaviour
{
    public float invincibleTime = 3f;

    private PlayerManager playerManager => PlayerManager.Instance;

    public void RevivePlayer()
    {
        Transform playerPosition = playerManager.currentPlayer.transform;
        GameObject player = playerManager.currentPlayer;

        Destroy(player);
        player = Instantiate(playerManager.playerPrefab, playerManager.transform);
        player.transform.position = playerPosition.position;

        player.GetComponent<PlayerInvincibility>().ActivateInvincibility(invincibleTime);
        playerManager.currentPlayer = player;
        playerManager.currentPlayer.GetComponent<PlayerController>().StartMovingForward();
        PlayerManager.PlayerChanged.Invoke(player);
    }
}

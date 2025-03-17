using UnityEditor;
using UnityEngine;

public class PlayerReviver : MonoBehaviour
{
    public GameObject playerPrefab;

    public GameObject currentPlayer;

    public Transform playerPosition;

    public float invincibleTime = 3f;

    public void RevivePlayer()
    {
        playerPosition = currentPlayer.transform;
        Transform playerParentTransform = playerPosition.parent;

        Destroy(currentPlayer);
        currentPlayer = Instantiate(playerPrefab, playerParentTransform);
        currentPlayer.transform.position = playerPosition.position;
        currentPlayer.GetComponent<PlayerController>().StartMovingForward();

    }
}

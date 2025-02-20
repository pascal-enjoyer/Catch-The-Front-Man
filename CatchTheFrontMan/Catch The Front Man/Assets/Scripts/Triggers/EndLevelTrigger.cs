using UnityEngine;
using UnityEngine.Events;

public class EndLevelTrigger : MonoBehaviour
{

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("hut");
        if (other.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            
            playerController.isMovementBlocked = true;
            GameSettings.Instance.gameController.OnLevelCompleted();
        }
    }
}

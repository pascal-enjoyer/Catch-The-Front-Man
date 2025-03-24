using UnityEngine;
using UnityEngine.UI;

public class MovementButtonsController : MonoBehaviour
{
    public PlayerController playerController => PlayerManager.Instance.currentPlayer.GetComponent<PlayerController>();

    public void OnLeftButtonPressed()
    {
        playerController.OnLeftButtonClicked();
    }

    public void OnRightButtonPressed()
    {
        playerController.OnRightButtonClicked();
    }

    public void OnDownButtonPressed()
    {
        playerController.OnDownButtonClicked();
    }

}
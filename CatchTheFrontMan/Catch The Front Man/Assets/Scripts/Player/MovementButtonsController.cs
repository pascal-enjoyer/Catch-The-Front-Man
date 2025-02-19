using UnityEngine;
using UnityEngine.UI;

public class MovementButtonsController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

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


    public void OnUpButtonPressed()
    {
        playerController.OnUpButtonClicked();
    }
}
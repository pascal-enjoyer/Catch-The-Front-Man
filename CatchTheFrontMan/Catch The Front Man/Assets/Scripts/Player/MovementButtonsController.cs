using UnityEngine;
using UnityEngine.UI;

public class MovementButtonsController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    public void OnLeftButtonPressed()
    {
        playerMovement.CastRay(Vector3.left);
    }

    public void OnRightButtonPressed()
    {
        playerMovement.CastRay(Vector3.right);
    }

    public void OnDownButtonPressed()
    {
        playerMovement.CastRay(Vector3.down);
    }
}
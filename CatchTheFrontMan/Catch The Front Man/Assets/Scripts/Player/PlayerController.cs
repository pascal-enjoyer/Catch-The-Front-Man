using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum PlayerMovementState
    {
        MovingForward,
        MovingLeft,
        MovingRight,
        LiyngDown,
        left,
        right
    }

    // Метод для кастования луча в указанном направлении
    public void CastRay(Vector3 direction)
    {
        if (direction == Vector3.right)
        {

        }
        else if (direction == Vector3.left)
        {

        }
        else if (direction == Vector3.down)
        {

        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, rayDistance, interactableLayer))
        {

            // Устанавливаем целевую позицию
            targetPosition = new Vector3(hit.point.x, 0, hit.point.z);
        }

    }
}

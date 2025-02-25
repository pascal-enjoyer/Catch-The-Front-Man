
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float roomWidth = 8f;
    public LayerMask interactableLayer;

    public float moveSpeed = 3f;
    public float sideSpeed = 10f;

    public Vector3 targetPosition;
    
    public PlayerAnimationManager animManager;

    public PlayerMovementState currentState;
    public Dictionary<PlayerMovementState, PlayerMovementState> antiPairs = 
        new Dictionary<PlayerMovementState, PlayerMovementState>
        {
            {PlayerMovementState.left, PlayerMovementState.right },
            {PlayerMovementState.right, PlayerMovementState.left },
            {PlayerMovementState.up, PlayerMovementState.down}
        };

    public bool isMoving = false;
    public bool isStopped = true;
    public bool isMovementBlocked = true;

    public Vector3 raycastYOffset = new Vector3(0, 0.1f, 0);

    private void Update()
    {
        if (isMovementBlocked) return;
        if (!isStopped)
        {

            // ƒвижение вперед

            animManager.ChangeAnimation("Crouch Walk");
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            isStopped = false;
        }
        else if (isMoving)
        {
            // ѕлавное перемещение к целевой позиции
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(targetPosition.x, transform.position.y, transform.position.z), sideSpeed * Time.deltaTime);

            // ≈сли игрок достиг целевой позиции, обновл€ем состо€ние
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                if (currentState == PlayerMovementState.center)
                {
                    isStopped = false;  
                }
            }
        }

    }

    public enum PlayerMovementState
    {
        center,
        up,
        down,
        left,
        right
    }

    public void OnUpButtonClicked()
    {
        
    }

    public void OnLeftButtonClicked()
    {
        if (isMovementBlocked) return;
        if (currentState == antiPairs[PlayerMovementState.left])
        {
            targetPosition = new Vector3(0, transform.position.y, transform.position.z);
            currentState = PlayerMovementState.center;
            transform.rotation = Quaternion.Euler(0,0,0);

            animManager.ChangeAnimation("Crouch Walk");
            isMoving = true;
            isStopped = true;
        }
        else 
            if (currentState != PlayerMovementState.left)
            {
                if (CastRay(Vector3.left))
                {
                    CastRay(Vector3.left);
                    currentState = PlayerMovementState.left;

                    transform.rotation = Quaternion.Euler(0, 90, 0);
                    animManager.ChangeAnimation("Wall Lean");

                    isStopped = true;
                    isMoving = true;
                }
            }
        }

    public void OnRightButtonClicked()
    {
        if (isMovementBlocked) return;
        if (currentState == antiPairs[PlayerMovementState.right])
        {

            targetPosition = new Vector3(0, transform.position.y, transform.position.z);
            currentState = PlayerMovementState.center;

            animManager.ChangeAnimation("Crouch Walk");
            transform.rotation = Quaternion.Euler(0, 0, 0);
            isMoving = true;
            isStopped = true;
        }
        else if (currentState != PlayerMovementState.right)
        {
            if (CastRay(Vector3.right))
            {
                transform.rotation = Quaternion.Euler(0, -90, 0);
                animManager.ChangeAnimation("Wall Lean");

                currentState = PlayerMovementState.right;
                isStopped = true;
                isMoving = true;
            }
        }
    }

    public void OnDownButtonClicked()
    {
        if (isMovementBlocked) return;
        if (currentState == PlayerMovementState.down)
        {

            animManager.ChangeAnimation("Crouch Walk");
            targetPosition = new Vector3(0, transform.position.y, transform.position.z);

            transform.rotation = Quaternion.Euler(0, 0, 0);
            currentState = PlayerMovementState.center;
            isMoving = false;
            isStopped = false;
        }
        else if (currentState != PlayerMovementState.down)
        {

            targetPosition = new Vector3(0, transform.position.y, transform.position.z);

            transform.rotation = Quaternion.Euler(0, 0, 0);
            animManager.ChangeAnimation("Floor Lie");
            isStopped = true;
            isMoving = true;
            currentState = PlayerMovementState.down;
        }
    }
    

    public void StartMovingForward()
    {
        isMovementBlocked = false;
        isMoving = false;
        isStopped = false;
        currentState = PlayerMovementState.center;
    }
    // ћетод дл€ кастовани€ луча в указанном направлении
    public bool CastRay(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + raycastYOffset, direction + raycastYOffset, out hit, roomWidth, interactableLayer))
        {
            // ”станавливаем целевую позицию
            targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            return true;
        }
        else
            return false;
    }
}

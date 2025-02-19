
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float roomWidth = 8f;
    public LayerMask interactableLayer;

    public float moveSpeed = 10;

    public Vector3 targetPosition;

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


    private void Update()
    {
        if (!isStopped)
        {

            // �������� ������
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            isStopped = false;
        }
        else if (isMoving)
        {
            // ������� ����������� � ������� �������
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(targetPosition.x, transform.position.y, transform.position.z), moveSpeed * Time.deltaTime);

            // ���� ����� ������ ������� �������, ��������� ���������
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                if (currentState == PlayerMovementState.center)
                {
                    isStopped = false;  
                }
            }
        }
        Debug.Log(currentState);
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
        if (currentState == antiPairs[PlayerMovementState.left])
        {
            targetPosition = new Vector3(0, transform.position.y, transform.position.z);
            currentState = PlayerMovementState.center;

            isMoving = true;
            isStopped = true;
        }
        else if (currentState != PlayerMovementState.left)
        {

            CastRay(Vector3.left);
            currentState = PlayerMovementState.left;

            isStopped = true;
            isMoving = true;
        }
    }

    public void OnRightButtonClicked()
    {
        if (currentState == antiPairs[PlayerMovementState.right])
        {

            targetPosition = new Vector3(0, transform.position.y, transform.position.z);
            currentState = PlayerMovementState.center;

            isMoving = true;
            isStopped = true;
        }
        else if (currentState != PlayerMovementState.right)
        {

            CastRay(Vector3.right);
            currentState = PlayerMovementState.right;
            isStopped = true;
            isMoving = true;
        }
    }

    public void OnDownButtonClicked()
    {
        if (currentState == PlayerMovementState.down)
        {

            targetPosition = new Vector3(0, transform.position.y, transform.position.z);
            currentState = PlayerMovementState.center;
            isMoving = false;
            isStopped = false;
        }
        else if (currentState != PlayerMovementState.down)
        {
            isStopped = true;
            isMoving = false;
            currentState = PlayerMovementState.down;
        }
    }
    

    public void StartMovingForward()
    {
        isMoving = false;
        isStopped = false;
        currentState = PlayerMovementState.center;
    }
    // ����� ��� ���������� ���� � ��������� �����������
    public void CastRay(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, roomWidth, interactableLayer))
        {
            // ������������� ������� �������
            targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }

    }
}

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum PlayerState
{
    Idle,
    MovingForward,
    MovingSide,
    Crouching,
    Leaning
}

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float sideSpeed = 3f;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private LayerMask wallLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform modelTransform;
    private Rigidbody rb;

    [Header("Events")]
    public UnityEvent OnCrouchEvent;
    public UnityEvent OnLeanEvent;

    private PlayerState currentState = PlayerState.Idle;
    private int moveDirection = 0; // -1 = left, 1 = right
    private bool isAgainstWall = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckForwardObstacle();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // ���������� �� UI ������ (Event Trigger)
    public void OnMoveInput(int direction)
    {
        if (currentState == PlayerState.Crouching) return;

        // ��������� �������� � �������
        if (direction != 0 && currentState != PlayerState.MovingSide)
        {
            StartSideMovement(direction);
        }
        else
        {
            StopMovement();
        }
    }

    // ���������� �� UI ������ "Down"
    public void OnCrouchInput()
    {
        if (currentState == PlayerState.Crouching)
        {
            StandUp();
        }
        else
        {
            Crouch();
        }
    }

    private void StartForwardMovement()
    {
        if (isAgainstWall) return;
        currentState = PlayerState.MovingForward;
    }

    private void StartSideMovement(int direction)
    {
        if (CheckSideObstacle(direction)) return;

        moveDirection = direction;
        currentState = PlayerState.MovingSide;
        modelTransform.localScale = new Vector3(direction, 1, 1); // �������������� ������
    }

    private void StopMovement()
    {
        if (currentState == PlayerState.MovingSide)
        {
            currentState = PlayerState.Idle;
            StartCoroutine(CheckAutoForwardMove());
        }
    }

    private void Crouch()
    {
        currentState = PlayerState.Crouching;
        OnCrouchEvent?.Invoke();
    }

    private void StandUp()
    {
        currentState = PlayerState.Idle;
    }

    private void HandleMovement()
    {
        switch (currentState)
        {
            case PlayerState.MovingForward:
                if (!isAgainstWall)
                {
                    rb.linearVelocity = transform.forward * forwardSpeed;
                }
                break;

            case PlayerState.MovingSide:
                rb.linearVelocity = transform.right * moveDirection * sideSpeed;
                break;

            default:
                rb.linearVelocity = Vector3.zero;
                break;
        }
    }

    private void CheckForwardObstacle()
    {
        RaycastHit hit;
        isAgainstWall = Physics.Raycast(transform.position, transform.forward,
            out hit, raycastDistance, wallLayer);

        if (isAgainstWall && currentState == PlayerState.MovingForward)
        {
            currentState = PlayerState.Leaning;
            OnLeanEvent?.Invoke();
        }
    }

    private bool CheckSideObstacle(int direction)
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, transform.right * direction,
            out hit, raycastDistance, wallLayer);
    }

    private IEnumerator CheckAutoForwardMove()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentState == PlayerState.Idle && !isAgainstWall)
        {
            StartForwardMovement();
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool("IsMoving", currentState == PlayerState.MovingForward);
        animator.SetBool("IsCrouching", currentState == PlayerState.Crouching);
        animator.SetBool("IsLeaning", currentState == PlayerState.Leaning);
        animator.SetFloat("MoveSpeed", rb.linearVelocity.magnitude);
    }
}
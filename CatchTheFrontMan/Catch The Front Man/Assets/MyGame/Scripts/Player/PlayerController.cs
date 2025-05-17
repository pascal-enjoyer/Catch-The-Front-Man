using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public float roomWidth = 8f;
    public LayerMask interactableLayer;

    public float moveSpeed = 3f;
    public float sideSpeed = 10f;
    public float wallMoveSpeed = 2f; // Speed for moving along walls
    public float crawlSpeed = 1.5f; // Speed for crawling while prone

    public Vector3 targetPosition;

    public PlayerAnimationManager animManager;

    public PlayerMovementState currentState;

    public Dictionary<PlayerMovementState, PlayerMovementState> antiPairs =
        new Dictionary<PlayerMovementState, PlayerMovementState>
        {
            {PlayerMovementState.left, PlayerMovementState.right},
            {PlayerMovementState.right, PlayerMovementState.left},
            {PlayerMovementState.up, PlayerMovementState.down}
        };

    public bool isMoving = false;
    public bool isStopped = true;
    public bool isMovementBlocked = true;
    private bool lockHoldInputs = false; // New flag to lock hold inputs after center return
    private bool shouldStartCrawling = false; // Flag to start crawling after prone transition

    [Header("Kill Settings")]
    public float killStunDuration = 3f;
    private bool isStunned = false;

    public bool isDead = false;

    public Vector3 raycastYOffset = new Vector3(0, 0.1f, 0);

    public UnityEvent PlayerDie;

    private PlayerInvincibility invincibility;
    private bool isMovingAlongWall = false;
    private bool isCrawling = false;
    private bool isLeftHeld = false;
    private bool isRightHeld = false;
    private bool isDownHeld = false;

    private void Awake()
    {
        invincibility = GetComponent<PlayerInvincibility>();
    }

    private void Start()
    {
        DeathTimer.OnTimerEnded += OnDeathTimerEnded;
        EventManager.OnDialogStarted += OnDialogStarted;
        EventManager.OnDialogEnded += OnDialogEnded;
    }

    private void OnDestroy()
    {
        DeathTimer.OnTimerEnded -= OnDeathTimerEnded;
        EventManager.OnDialogStarted -= OnDialogStarted;
        EventManager.OnDialogEnded -= OnDialogEnded;
    }

    private void Update()
    {
        if (ShouldSkipUpdate()) return;

        if (!isStopped)
        {
            HandleForwardMovement();
        }
        else if (isMoving)
        {
            HandleSideMovement();
        }

        // Handle wall movement only when at a wall
        if (((currentState == PlayerMovementState.left && isLeftHeld) ||
             (currentState == PlayerMovementState.right && isRightHeld)) && isStopped && !isMoving)
        {
            if (!isMovingAlongWall)
            {
                isMovingAlongWall = true;
                animManager.ChangeAnimation("Wall Move");
            }
            // Move along the wall in world forward direction
            transform.position += Vector3.forward * wallMoveSpeed * Time.deltaTime;
        }
        else if (isMovingAlongWall)
        {
            isMovingAlongWall = false;
            if (currentState == PlayerMovementState.left || currentState == PlayerMovementState.right)
            {
                animManager.ChangeAnimation("Wall Lean");
            }
        }

        // Handle crawling
        if (currentState == PlayerMovementState.down && isCrawling && !isMoving)
        {
            transform.Translate(Vector3.forward * crawlSpeed * Time.deltaTime);
        }
    }

    private bool ShouldSkipUpdate()
    {
        return isMovementBlocked || isStunned || isDead || DeathTimer.IsTimerActive;
    }

    private void HandleForwardMovement()
    {
        animManager.ChangeAnimation("Crouch Walk");
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void HandleSideMovement()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(targetPosition.x, transform.position.y, targetPosition.z),
            sideSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            if (currentState == PlayerMovementState.center)
            {
                isStopped = false;
                lockHoldInputs = false; // Clear hold input lock
                Debug.Log("Reached center, lockHoldInputs cleared");
            }
            else if (currentState == PlayerMovementState.left && isLeftHeld)
            {
                // Start moving along left wall if button is still held
                isMovingAlongWall = true;
                animManager.ChangeAnimation("Wall Move");
            }
            else if (currentState == PlayerMovementState.right && isRightHeld)
            {
                // Start moving along right wall if button is still held
                isMovingAlongWall = true;
                animManager.ChangeAnimation("Wall Move");
            }
            else if (currentState == PlayerMovementState.down && shouldStartCrawling && isDownHeld)
            {
                // Start crawling if down button is held after prone transition
                isCrawling = true;
                shouldStartCrawling = false;
                animManager.ChangeAnimation("Crawl");
            }
        }
    }

    public void TriggerKillStun()
    {
        if (!CanBeAffectedByEnemy()) return;
        if (!isStunned)
            StartCoroutine(KillStunRoutine());
    }

    private IEnumerator KillStunRoutine()
    {
        isStunned = true;
        animManager.ChangeAnimation("Stab");
        Debug.Log("Kill stun started");

        yield return new WaitForSeconds(killStunDuration);

        isStunned = false;
        Debug.Log("Kill stun ended");

        if (!isMovementBlocked && !isStopped)
            animManager.ChangeAnimation("Crouch Walk");
    }

    public enum PlayerMovementState
    {
        center,
        up,
        down,
        left,
        right
    }

    public void OnDownButtonClicked()
    {
        if (!isDownHeld && ShouldSkipUpdate() == false) // Only trigger if not holding
        {
            SetProneState();
        }
    }

    public void OnUpButtonClicked()
    {
        if (ShouldSkipUpdate()) return;

        if (currentState == PlayerMovementState.down)
        {
            ReturnToCenter();
            isMoving = false;
            isStopped = false;
            isCrawling = false;
            isDownHeld = false;
            shouldStartCrawling = false;
        }
    }

    private void SetProneState()
    {
        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        SetColliderSettings(2, new Vector3(0, 0.2f, 0));
        transform.rotation = Quaternion.identity;
        animManager.ChangeAnimation("Floor Lie");
        isStopped = true;
        isMoving = true;
        currentState = PlayerMovementState.down;
    }

    private void HandleCommonMovement(PlayerMovementState newState, Vector3 direction, float yRotation)
    {
        if (ShouldSkipUpdate()) return;

        if (antiPairs.ContainsKey(newState) && currentState == antiPairs[newState])
        {
            lockHoldInputs = true; // Lock hold inputs during center transition
            Debug.Log($"Returning to center from {currentState}, setting lockHoldInputs = true");
            ReturnToCenter();
        }
        else if (currentState != newState && CastRay(direction))
        {
            MoveToWall(newState, yRotation);
        }
    }

    private void ReturnToCenter()
    {
        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        currentState = PlayerMovementState.center;
        SetColliderSettings(1, new Vector3(0, 0.785f, 0));
        transform.rotation = Quaternion.identity;
        animManager.ChangeAnimation("Crouch Walk");
        isMoving = true;
        isStopped = true;
        isMovingAlongWall = false;
        isLeftHeld = false; // Clear hold states to prevent immediate re-trigger
        isRightHeld = false;
        isCrawling = false;
        isDownHeld = false;
        shouldStartCrawling = false;
    }

    private void MoveToWall(PlayerMovementState state, float rotationY)
    {
        currentState = state;
        SetColliderSettings(1, new Vector3(0, 0.785f, 0));
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        animManager.ChangeAnimation("Wall Lean");
        isStopped = true;
        isMoving = true;
        // Target position is set by CastRay
    }

    private void SetColliderSettings(int direction, Vector3 center)
    {
        var collider = GetComponent<CapsuleCollider>();
        collider.direction = direction;
        collider.center = center;
    }

    public void OnLeftButtonClicked()
    {
        if (!isLeftHeld && ShouldSkipUpdate() == false) // Only trigger if not holding
        {
            HandleCommonMovement(PlayerMovementState.left, Vector3.left, 90f);
        }
    }

    public void OnRightButtonClicked()
    {
        if (!isRightHeld && ShouldSkipUpdate() == false) // Only trigger if not holding
        {
            HandleCommonMovement(PlayerMovementState.right, Vector3.right, -90f);
        }
    }

    public void OnLeftButtonHeld(bool isHeld)
    {
        isLeftHeld = isHeld;
        if (isHeld && currentState != PlayerMovementState.left && !lockHoldInputs && !isMoving && ShouldSkipUpdate() == false)
        {
            Debug.Log("Left button held, initiating wall movement");
            HandleCommonMovement(PlayerMovementState.left, Vector3.left, 90f);
        }
    }

    public void OnRightButtonHeld(bool isHeld)
    {
        isRightHeld = isHeld;
        if (isHeld && currentState != PlayerMovementState.right && !lockHoldInputs && !isMoving && ShouldSkipUpdate() == false)
        {
            Debug.Log("Right button held, initiating wall movement");
            HandleCommonMovement(PlayerMovementState.right, Vector3.right, -90f);
        }
    }

    public void OnDownButtonHeld(bool isHeld)
    {
        isDownHeld = isHeld;
        if (isHeld && currentState != PlayerMovementState.down && ShouldSkipUpdate() == false)
        {
            shouldStartCrawling = true; // Flag to start crawling after prone transition
            SetProneState();
        }
        else if (currentState == PlayerMovementState.down && !isMoving)
        {
            isCrawling = isHeld;
            animManager.ChangeAnimation(isHeld ? "Crawl" : "Floor Lie");
        }
    }

    public void StartMovingForward()
    {
        isMovementBlocked = false;
        isMoving = false;
        isStopped = false;
        currentState = PlayerMovementState.center;
        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;
        SetColliderSettings(1, new Vector3(0, 0.785f, 0));
        animManager.ChangeAnimation("Crouch Walk");
    }

    private bool CastRay(Vector3 direction)
    {
        RaycastHit hit;
        var origin = transform.position + raycastYOffset;

        if (Physics.Raycast(origin, direction, out hit, roomWidth, interactableLayer))
        {
            targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            return true;
        }
        return false;
    }

    public void Die()
    {
        if (isDead || !CanBeAffectedByEnemy()) return;
        isDead = true;
        animManager.animator.enabled = false;
        PlayerDie.Invoke();
    }

    private bool CanBeAffectedByEnemy()
    {
        return !DeathTimer.IsTimerActive && !invincibility.IsInvincible;
    }

    public void RestoreState(PlayerMovementState state, Vector3 savedTargetPosition)
    {
        isDead = false;
        animManager.animator.enabled = true;
        currentState = state;
        targetPosition = savedTargetPosition;

        switch (state)
        {
            case PlayerMovementState.center:
                SetColliderSettings(1, new Vector3(0, 0.785f, 0));
                transform.rotation = Quaternion.identity;
                transform.position = new Vector3(0, transform.position.y, transform.position.z);
                animManager.ChangeAnimation("Crouch Walk");
                isStopped = false;
                isMoving = false;
                break;
            case PlayerMovementState.left:
                SetColliderSettings(1, new Vector3(0, 0.785f, 0));
                transform.rotation = Quaternion.Euler(0, 90f, 0);
                transform.position = new Vector3(savedTargetPosition.x, transform.position.y, transform.position.z);
                animManager.ChangeAnimation("Wall Lean");
                isStopped = true;
                isMoving = false;
                break;
            case PlayerMovementState.right:
                SetColliderSettings(1, new Vector3(0, 0.785f, 0));
                transform.rotation = Quaternion.Euler(0, -90f, 0);
                transform.position = new Vector3(savedTargetPosition.x, transform.position.y, transform.position.z);
                animManager.ChangeAnimation("Wall Lean");
                isStopped = true;
                isMoving = false;
                break;
            case PlayerMovementState.down:
                SetColliderSettings(2, new Vector3(0, 0.2f, 0));
                transform.rotation = Quaternion.identity;
                transform.position = new Vector3(savedTargetPosition.x, transform.position.y, transform.position.z);
                animManager.ChangeAnimation("Floor Lie");
                isStopped = true;
                isMoving = false;
                break;
        }
    }

    private void OnDeathTimerEnded()
    {
        if (invincibility != null && !isDead)
        {
            invincibility.ActivateInvincibility(3f);
            Debug.Log("PlayerController: DeathTimer ended, invincibility activated.");
        }
    }

    private void OnDialogStarted()
    {
        Debug.Log("PlayerController: OnDialogStarted called.");
        isMovementBlocked = true;
        isMoving = false;
        isStopped = true;
        isMovingAlongWall = false;
        isCrawling = false;
        isLeftHeld = false;
        isRightHeld = false;
        isDownHeld = false;
        lockHoldInputs = false;
        shouldStartCrawling = false;
        animManager.ChangeAnimation("idle");
    }

    private void OnDialogEnded()
    {
        Debug.Log("PlayerController: OnDialogEnded called.");
        // Movement will be restored in CameraMovement after camera returns
    }

    public void RestoreStateAfterDialog()
    {
        Debug.Log($"PlayerController: Restoring state after dialog, currentState: {currentState}");
        if (animManager == null)
        {
            Debug.LogError("PlayerController: animManager is null during RestoreStateAfterDialog.", this);
            return;
        }

        isMovementBlocked = false;

        switch (currentState)
        {
            case PlayerMovementState.center:
                animManager.ChangeAnimation("Crouch Walk");
                isStopped = false;
                isMoving = false;
                Debug.Log("PlayerController: Restored Crouch Walk, isStopped = false, isMoving = false");
                break;
            case PlayerMovementState.left:
            case PlayerMovementState.right:
                animManager.ChangeAnimation("Wall Lean");
                isStopped = true;
                isMoving = false;
                Debug.Log("PlayerController: Restored Wall Lean, isStopped = true, isMoving = false");
                break;
            case PlayerMovementState.down:
                animManager.ChangeAnimation("Floor Lie");
                isStopped = true;
                isMoving = false;
                Debug.Log("PlayerController: Restored Floor Lie, isStopped = true, isMoving = false");
                break;
        }
    }
}
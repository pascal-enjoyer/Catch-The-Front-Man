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
    private PlayerMovementState pendingState; // Track intended state during transitions

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
    private bool shouldStartCrawling = false; // Flag to start crawling after prone transition
    private bool ignoreClicksAfterHold = false; // Flag to ignore clicks after hold release

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
        pendingState = PlayerMovementState.center; // Initialize to center
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
                
            }
            transform.position += Vector3.forward * wallMoveSpeed * Time.deltaTime;
            switch (currentState)
            {
                case PlayerMovementState.left:

                    animManager.ChangeAnimation("Left wall walk");
                    break;
                case PlayerMovementState.right:
                    animManager.ChangeAnimation("Right wall walk");
                    break;
            }
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
            currentState = pendingState; // Update state when transition completes
            if (currentState == PlayerMovementState.center)
            {
                isStopped = false;
            }
            else if (currentState == PlayerMovementState.left && isLeftHeld)
            {
                isMovingAlongWall = true;
                animManager.ChangeAnimation("Left wall walk");
            }
            else if (currentState == PlayerMovementState.right && isRightHeld)
            {
                isMovingAlongWall = true;
                animManager.ChangeAnimation("Right wall walk");
            }
            else if (currentState == PlayerMovementState.down && shouldStartCrawling && isDownHeld)
            {
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

        yield return new WaitForSeconds(killStunDuration);

        isStunned = false;

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
        if (!isDownHeld && ShouldSkipUpdate() == false)
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
        pendingState = PlayerMovementState.down;
        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        SetColliderSettings(2, new Vector3(0, 0.2f, 0));
        transform.rotation = Quaternion.identity;
        animManager.ChangeAnimation("Floor Lie");
        isStopped = true;
        isMoving = true;
    }

    private void HandleCommonMovement(PlayerMovementState newState, Vector3 direction, float yRotation, bool isHold = false)
    {
        if (ShouldSkipUpdate()) return;

        if (antiPairs.ContainsKey(newState) && currentState == antiPairs[newState])
        {
            // Lock clicks after hold to prevent OnPointerUp triggering movement
            if (isHold)
            {
                ignoreClicksAfterHold = true;
            }
            // Clear hold inputs to prevent immediate re-trigger
            if (newState == PlayerMovementState.left)
                isLeftHeld = false;
            else if (newState == PlayerMovementState.right)
                isRightHeld = false;
            ReturnToCenter();
        }
        else if (currentState != newState && (!isHold || !isMoving) && CastRay(direction))
        {
            MoveToWall(newState, yRotation);
        }
    }

    private void ReturnToCenter()
    {
        pendingState = PlayerMovementState.center;
        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        SetColliderSettings(1, new Vector3(0, 0.785f, 0));
        transform.rotation = Quaternion.identity;
        animManager.ChangeAnimation("Crouch Walk");
        isMoving = true;
        isStopped = true;
        isMovingAlongWall = false;
        isLeftHeld = false;
        isRightHeld = false;
        isCrawling = false;
        isDownHeld = false;
        shouldStartCrawling = false;
    }

    private void MoveToWall(PlayerMovementState state, float rotationY)
    {
        pendingState = state;
        SetColliderSettings(1, new Vector3(0, 0.785f, 0));
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        animManager.ChangeAnimation("Wall Lean");
        isStopped = true;
        isMoving = true;
    }

    private void SetColliderSettings(int direction, Vector3 center)
    {
        var collider = GetComponent<CapsuleCollider>();
        collider.direction = direction;
        collider.center = center;
    }

    public void OnLeftButtonClicked()
    {
        if (!isLeftHeld && !ignoreClicksAfterHold && !isMoving && pendingState != PlayerMovementState.center && ShouldSkipUpdate() == false)
        {
            HandleCommonMovement(PlayerMovementState.left, Vector3.left, 90f);
        }
    }

    public void OnRightButtonClicked()
    {
        if (!isRightHeld && !ignoreClicksAfterHold && !isMoving && pendingState != PlayerMovementState.center && ShouldSkipUpdate() == false)
        {
            HandleCommonMovement(PlayerMovementState.right, Vector3.right, -90f);
        }
    }

    public void OnLeftButtonHeld(bool isHeld)
    {
        isLeftHeld = isHeld;
        if (isHeld && currentState != PlayerMovementState.left && ShouldSkipUpdate() == false)
        {
            HandleCommonMovement(PlayerMovementState.left, Vector3.left, 90f, true);

        }

    }

    public void OnRightButtonHeld(bool isHeld)
    {
        isRightHeld = isHeld;
        if (isHeld && currentState != PlayerMovementState.right && ShouldSkipUpdate() == false)
        {
            HandleCommonMovement(PlayerMovementState.right, Vector3.right, -90f, true);

        }
        
    }

    public void OnLeftButtonReleased()
    {
        isLeftHeld = false;
        if (currentState == PlayerMovementState.center && !isMoving)
        {
            ignoreClicksAfterHold = false; // Only clear when stable in center
        }
    }

    public void OnRightButtonReleased()
    {
        isRightHeld = false;
        if (currentState == PlayerMovementState.center && !isMoving)
        {
            ignoreClicksAfterHold = false; // Only clear when stable in center
        }
    }

    public void OnDownButtonHeld(bool isHeld)
    {
        isDownHeld = isHeld;
        if (isHeld && currentState != PlayerMovementState.down && ShouldSkipUpdate() == false)
        {
            shouldStartCrawling = true;
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
        pendingState = PlayerMovementState.center;
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
        pendingState = state;
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
        }
    }

    private void OnDialogStarted()
    {
        isMovementBlocked = true;
        isMoving = false;
        isStopped = true;
        isMovingAlongWall = false;
        isCrawling = false;
        isLeftHeld = false;
        isRightHeld = false;
        isDownHeld = false;
        shouldStartCrawling = false;
        ignoreClicksAfterHold = false;
        animManager.ChangeAnimation("idle");
    }

    private void OnDialogEnded()
    {
        // Movement will be restored in CameraMovement after camera returns
    }

    public void RestoreStateAfterDialog()
    {
        if (animManager == null)
        {
            return;
        }

        isMovementBlocked = false;

        switch (currentState)
        {
            case PlayerMovementState.center:
                animManager.ChangeAnimation("Crouch Walk");
                isStopped = false;
                isMoving = false;
                break;
            case PlayerMovementState.left:
            case PlayerMovementState.right:
                animManager.ChangeAnimation("Wall Lean");
                isStopped = true;
                isMoving = false;
                break;
            case PlayerMovementState.down:
                animManager.ChangeAnimation("Floor Lie");
                isStopped = true;
                isMoving = false;
                break;
        }
    }
}
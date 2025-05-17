using System.Collections;
using UnityEngine;

public class FreezeEnemiesBonus : Bonus
{
    [Header("Animation Settings")]
    [SerializeField] private float kneeDollDuration = 1.5f;
    [SerializeField] private GameObject handPrefab;
    private GameObject spawnedHandObject;

    private PlayerAnimationManager playerAnimManager;
    private string previousAnimation;
    private Coroutine animationRoutine;

    // ��������� ������ ����� ����������
    private Quaternion originalRotation;
    private PlayerController.PlayerMovementState originalState;
    private bool originalIsMoving;
    private bool originalIsStopped;

    protected override void ApplyEffect(bool activate)
    {
        PlayerController playerController = target.GetComponent<PlayerController>();
        if (playerController == null) return;

        foreach (GameObject enemy in EnemyManager.Instance.SpawnedEnemies)
        {
            var patrol = enemy.GetComponent<EnemyPatrol>();
            var vision = enemy.GetComponent<EnemyVision>();
            if (patrol != null) patrol.enabled = !activate;
            if (vision != null) vision.enabled = !activate;
        }

        if (activate)
        {
            // ��������� ������� ���������
            originalRotation = playerController.transform.rotation;
            originalState = playerController.currentState;
            originalIsMoving = playerController.isMoving;
            originalIsStopped = playerController.isStopped;

            // ��������� ��������
            playerAnimManager = playerController.animManager;
            if (playerAnimManager != null)
            {
                previousAnimation = playerAnimManager.GetCurrentAnimationName();
                AudioManager.Instance.Play("DollRedLight");
                playerAnimManager.ChangeAnimation("Knee doll");
            }

            // ���������� ��������
            playerController.isMovementBlocked = true;
            playerController.isStopped = true;
            playerController.isMoving = false;

            // ����� ������� � ����
            if (handPrefab != null)
            {
                Transform hand = FindPlayerHand();
                if (hand != null)
                    spawnedHandObject = Instantiate(handPrefab, hand);
            }

            // ������ �������� ��������������
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);
            animationRoutine = StartCoroutine(RestoreStateRoutine(playerController));
        }
        else
        {
            AudioManager.Instance.Play("DollGreenLight");
            // ������� ��� �����������
            if (spawnedHandObject != null)
                Destroy(spawnedHandObject);
        }
    }

    private IEnumerator RestoreStateRoutine(PlayerController playerController)
    {
        yield return new WaitForSeconds(kneeDollDuration);

        // �������������� ��������
        playerController.isMovementBlocked = false;
        playerController.isStopped = originalIsStopped;
        playerController.isMoving = originalIsMoving;

        // �������������� ������� � ��������
        playerController.transform.rotation = originalRotation;
        playerController.currentState = originalState;
        playerAnimManager.ChangeAnimation(previousAnimation);

        // �������� ������� �� ����
        if (spawnedHandObject != null)
            Destroy(spawnedHandObject);
    }

    private Transform FindPlayerHand()
    {
        foreach (Transform child in target.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Hand_R"))
                return child;
        }
        return null;
    }


    public override void CopyFrom(Bonus source)
    {
        base.CopyFrom(source);
        if (source is FreezeEnemiesBonus pinkSource)
        {
            handPrefab = pinkSource.handPrefab;
        }
    }

    protected void OnDestroy()
    {
        // ������������� �������� ��� �����������
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        // ������� ������ �� ����
        if (spawnedHandObject != null)
        {
            Destroy(spawnedHandObject);
        }
    }
}
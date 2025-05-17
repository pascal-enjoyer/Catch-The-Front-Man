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

    // Состояние игрока перед активацией
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
            // Сохраняем текущее состояние
            originalRotation = playerController.transform.rotation;
            originalState = playerController.currentState;
            originalIsMoving = playerController.isMoving;
            originalIsStopped = playerController.isStopped;

            // Настройка анимации
            playerAnimManager = playerController.animManager;
            if (playerAnimManager != null)
            {
                previousAnimation = playerAnimManager.GetCurrentAnimationName();
                AudioManager.Instance.Play("DollRedLight");
                playerAnimManager.ChangeAnimation("Knee doll");
            }

            // Блокировка движения
            playerController.isMovementBlocked = true;
            playerController.isStopped = true;
            playerController.isMoving = false;

            // Спавн объекта в руке
            if (handPrefab != null)
            {
                Transform hand = FindPlayerHand();
                if (hand != null)
                    spawnedHandObject = Instantiate(handPrefab, hand);
            }

            // Запуск корутины восстановления
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);
            animationRoutine = StartCoroutine(RestoreStateRoutine(playerController));
        }
        else
        {
            AudioManager.Instance.Play("DollGreenLight");
            // Очистка при деактивации
            if (spawnedHandObject != null)
                Destroy(spawnedHandObject);
        }
    }

    private IEnumerator RestoreStateRoutine(PlayerController playerController)
    {
        yield return new WaitForSeconds(kneeDollDuration);

        // Восстановление движения
        playerController.isMovementBlocked = false;
        playerController.isStopped = originalIsStopped;
        playerController.isMoving = originalIsMoving;

        // Восстановление позиции и поворота
        playerController.transform.rotation = originalRotation;
        playerController.currentState = originalState;
        playerAnimManager.ChangeAnimation(previousAnimation);

        // Удаление объекта из руки
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
        // Останавливаем корутину при уничтожении
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        // Удаляем объект из руки
        if (spawnedHandObject != null)
        {
            Destroy(spawnedHandObject);
        }
    }
}
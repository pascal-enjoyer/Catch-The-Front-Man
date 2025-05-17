using UnityEngine;
using System.Collections;

public class PinkSuitBonus : Bonus
{
    public GameObject pinkSuitPrefab;
    private GameObject spawnedPinkSuit;
    private PlayerAnimationManager playerAnimationManager;

    private GameObject originalAnimatorObject;
    private Animator originalAnimator;


    private PlayerAnimationManager animManager;
    private GameObject originalModel;
    // Переменные для синхронизации анимации
    private string currentAnimation;
    private float normalizedTime;
    private float playbackSpeed;

    protected override void ApplyEffect(bool isActive)
    {
        // Обработка врагов
        foreach (GameObject enemy in EnemyManager.Instance.SpawnedEnemies)
        {
            var vision = enemy.GetComponent<EnemyVision>();
            if (vision != null) vision.enabled = !isActive;
        }

        // Получаем PlayerAnimationManager
        playerAnimationManager = GetComponent<PlayerAnimationManager>();
        if (playerAnimationManager == null)
        {
            Debug.LogError("PlayerAnimationManager not found!");
            return;
        }

        if (isActive)
        {
            animManager = GetComponent<PlayerAnimationManager>();
            // Сохраняем оригинальные компоненты
            originalModel = animManager.animator.gameObject;
            originalAnimator = animManager.animator;
            
            // Сохраняем текущее состояние анимации
            AnimatorStateInfo state = originalAnimator.GetCurrentAnimatorStateInfo(0);

            normalizedTime = state.normalizedTime;
            playbackSpeed = originalAnimator.speed;

            // Создаем костюм и синхронизируем анимацию
            spawnedPinkSuit = Instantiate(pinkSuitPrefab, animManager.transform);
            spawnedPinkSuit.transform.localPosition = Vector3.zero;

            Animator suitAnimator = spawnedPinkSuit.GetComponent<Animator>();

            currentAnimation = playerAnimationManager.GetCurrentAnimationName();
            suitAnimator.Play(currentAnimation, 0, normalizedTime);
            suitAnimator.speed = playbackSpeed;
            
            animManager.animator = suitAnimator;
            originalModel.SetActive(false);
        }
        else
        {
            if (spawnedPinkSuit != null)
            {
                // Сохраняем состояние анимации костюма
                Animator suitAnimator = spawnedPinkSuit.GetComponent<Animator>();
                AnimatorStateInfo suitState = suitAnimator.GetCurrentAnimatorStateInfo(0);
                currentAnimation = GetAnimationName(suitAnimator);
                normalizedTime = suitState.normalizedTime % 1.0f; // Обеспечиваем цикличность
                playbackSpeed = suitAnimator.speed;

                // Восстанавливаем оригинальный аниматор
                originalModel.SetActive(true);
                animManager.animator = originalAnimator;
                
                // Применяем сохраненное состояние
                originalAnimator.Play(currentAnimation, 0, normalizedTime);
                originalAnimator.speed = playbackSpeed;
                originalAnimator.Update(0f); // Немедленное обновление

                Destroy(spawnedPinkSuit);
                spawnedPinkSuit = null;
            }
        }
    }
    // Метод для получения имени текущей анимации
    private string GetAnimationName(Animator animator)
    {
        if (animator.runtimeAnimatorController == null) return "";

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(clip.name))
            {
                return clip.name;
            }
        }
        return "";
    }


    public override void CopyFrom(Bonus source)
    {
        base.CopyFrom(source);
        if (source is PinkSuitBonus pinkSource)
        {
            pinkSuitPrefab = pinkSource.pinkSuitPrefab;
        }
    }
}
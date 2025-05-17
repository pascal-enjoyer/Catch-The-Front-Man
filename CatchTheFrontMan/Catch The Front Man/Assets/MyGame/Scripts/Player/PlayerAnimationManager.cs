using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public Animator animator;
    public string currentAnimation = "";

    public void ChangeAnimation(string animName)
    {
        if (animator == null || string.IsNullOrEmpty(animName) || animName == currentAnimation)
        {
            //Debug.LogWarning($"ChangeAnimation failed: animator={animator}, animName={animName}, currentAnimation={currentAnimation}");
            return;
        }

        //Debug.Log($"Changing animation to: {animName}");
        animator.Play(animName);
        currentAnimation = animName;
    }

    public string GetCurrentAnimationName()
    {
        if (animator == null || !animator.isActiveAndEnabled)
        {
            //Debug.LogWarning("[Animation] Animator is not active!");
            return currentAnimation;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // Проверяем все возможные клипы
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (animator.HasState(0, Animator.StringToHash(clip.name)))
            {
                if (state.IsName(clip.name))
                {
                    currentAnimation = clip.name;
                    return currentAnimation;
                }
            }
        }

        //Debug.LogWarning($"[Animation] Unknown animation state: {state.shortNameHash}");
        return currentAnimation;
    }
    public void ForceAnimationUpdate()
    {
        if (animator == null || !animator.isActiveAndEnabled) return;

        string animName = GetCurrentAnimationName();
        //Debug.Log($"[Animation] Force updating animation: {animName}");
        animator.Play(animName, 0, 0f);
        animator.Update(0f);
    }

    public bool IsAnimationPlaying(string animName)
    {
        if (animator == null || !animator.isActiveAndEnabled)
            return false;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName(animName) && state.normalizedTime < 1f;
    }
}
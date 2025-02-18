using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{

    public Animator animator;

    public string currentAnimation = "";

    public void ChangeAnimation(string animName)
    {
        if (currentAnimation == animName) return;
        animator.CrossFade(animName, 0.2f);
        currentAnimation = animName;
    }

}

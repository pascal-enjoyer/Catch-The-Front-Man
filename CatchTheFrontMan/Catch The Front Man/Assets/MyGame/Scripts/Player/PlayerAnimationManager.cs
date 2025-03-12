using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{

    public Animator animator;

    public string currentAnimation = "";

    public void ChangeAnimation(string animName)
    {
        if (currentAnimation == animName) return;
        animator.Play(animName); // ���������� Play ������ CrossFade
        currentAnimation = animName;
    }
}

using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private bool isPlaying = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[AnimationController] No Animator component found!");
        }
        else
        {
            animator.speed = 0f; // Start paused
        }
    }

    public void ToggleAnimation()
    {
        if (animator == null) return;

        isPlaying = !isPlaying;
        animator.speed = isPlaying ? 1f : 0f;

        Debug.Log("[AnimationController] Animation " + (isPlaying ? "resumed" : "paused"));
    }

    public void PlayFromStart()
    {
        if (animator == null) return;

        animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f);
        animator.speed = 1f;
        isPlaying = true;

        Debug.Log("[AnimationController] Animation restarted from beginning.");
    }

    public void Pause()
    {
        if (animator == null) return;

        animator.speed = 0f;
        isPlaying = false;

        Debug.Log("[AnimationController] Animation paused.");
    }
}
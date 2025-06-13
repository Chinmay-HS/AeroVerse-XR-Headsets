using UnityEngine;

public class ModelAnimationMeta : MonoBehaviour
{
    public string animationStateName;       // Name of the state inside Animator (e.g. "Explode JWT")
    public float animationDuration = 2f;    // Total time in seconds
    [Range(0f, 1f)] public float explodeNormalizedTime = 0.5f;  // Midpoint (e.g. 0.5 = 50%)
}
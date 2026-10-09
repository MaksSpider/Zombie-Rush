using UnityEngine;

public class UIEnterAnimations : MonoBehaviour
{
    private Vector2 _originalPosition => transform.position;

    public enum AnimationType
    {
        FadeIn,
        FadeOut,
        SlideInFromLeft,
        SlideInFromRight,
        SlideInFromTop,
        SlideInFromBottom
    }

    private Animator _animator => GetComponent<Animator>();

    public void EnterAnimation()
    {
        _animator.SetTrigger("SlideInFromTop");
    }

    public void ExitAnimation()
    {
        _animator.SetTrigger("SlideOutToTop");
    }
}

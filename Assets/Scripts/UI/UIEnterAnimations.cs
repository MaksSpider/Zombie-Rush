
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIEnterAnimations : MonoBehaviour
{
    public enum AnimationType
    {
        None,
        FadeIn,
        FadeOut,
        SlideInFromLeft,
        SlideInFromRight,
        SlideInFromTop,
        SlideInFromBottom,
        SlideOutToLeft,
        SlideOutToRight,
        SlideOutToTop,
        SlideOutToBottom
    }

    [Header("Animations")]
    public AnimationType enterAnimation = AnimationType.SlideInFromRight;
    public AnimationType exitAnimation = AnimationType.SlideOutToRight;

    [Header("Settings")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float slideDistance = 1000f;

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Vector2 _originalPosition;
    private Coroutine _animation;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _originalPosition = _rect.anchoredPosition;
    }

    public void EnterAnimation(Action actionAfterAnimation = null)
    {
        PlayAnimation(enterAnimation, actionAfterAnimation);
    }

    public void ExitAnimation(Action actionAfterAnimation = null)
    {
        PlayAnimation(exitAnimation, actionAfterAnimation);
    }

    private void PlayAnimation(AnimationType type, Action actionAfterAnimation)
    {
        if (_animation != null)
            StopCoroutine(_animation);

        _animation = StartCoroutine(Animate(type, actionAfterAnimation));
    }

    private IEnumerator Animate(AnimationType type, Action actionAfterAnimation)
    {
        Vector2 startPosition = _rect.anchoredPosition;
        Vector2 targetPosition = _originalPosition;

        float startAlpha = _canvasGroup.alpha;
        float targetAlpha = 1f;

        switch (type)
        {
            case AnimationType.SlideInFromLeft:
                startPosition = _originalPosition + Vector2.left * slideDistance;
                break;

            case AnimationType.SlideInFromRight:
                startPosition = _originalPosition + Vector2.right * slideDistance;
                break;

            case AnimationType.SlideInFromTop:
                startPosition = _originalPosition + Vector2.up * slideDistance;
                break;

            case AnimationType.SlideInFromBottom:
                startPosition = _originalPosition + Vector2.down * slideDistance;
                break;

            case AnimationType.SlideOutToLeft:
                targetPosition = _originalPosition + Vector2.left * slideDistance;
                break;

            case AnimationType.SlideOutToRight:
                targetPosition = _originalPosition + Vector2.right * slideDistance;
                break;

            case AnimationType.SlideOutToTop:
                targetPosition = _originalPosition + Vector2.up * slideDistance;
                break;

            case AnimationType.SlideOutToBottom:
                targetPosition = _originalPosition + Vector2.down * slideDistance;
                break;

            case AnimationType.FadeIn:
                startAlpha = 0f;
                break;

            case AnimationType.FadeOut:
                targetAlpha = 0f;
                break;

            case AnimationType.None:
                _rect.anchoredPosition = _originalPosition;
                _canvasGroup.alpha = 1f;
                _animation = null;
                yield break;
        }

        if (type == AnimationType.SlideInFromLeft ||
            type == AnimationType.SlideInFromRight ||
            type == AnimationType.SlideInFromTop ||
            type == AnimationType.SlideInFromBottom)
        {
            _rect.anchoredPosition = startPosition;
        }

        _canvasGroup.alpha = startAlpha;

        Vector2 actualStartPosition = _rect.anchoredPosition;
        float actualStartAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

            // SmoothStep zapewnia płynne rozpędzanie i hamowanie.
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            _rect.anchoredPosition =
                Vector2.Lerp(actualStartPosition, targetPosition, easedT);

            _canvasGroup.alpha =
                Mathf.Lerp(actualStartAlpha, targetAlpha, easedT);

            yield return null;
        }

        _rect.anchoredPosition = targetPosition;
        _canvasGroup.alpha = targetAlpha;
        _animation = null;

        actionAfterAnimation?.Invoke();
    }
}

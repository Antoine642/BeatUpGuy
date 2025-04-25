using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SimpleMenuAnimator : MonoBehaviour
{
    [Header("Button Animation")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.9f;
    public float animationDuration = 0.2f;

    private Vector3 originalScale;
    private Coroutine currentAnimation;

    void Start()
    {
        originalScale = transform.localScale;

        // Add hover and click handlers
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Add event triggers for hover
            EventTrigger trigger = gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<EventTrigger>();
            }

            // Add pointer enter event
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { OnPointerEnter(); });
            trigger.triggers.Add(enterEntry);

            // Add pointer exit event
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { OnPointerExit(); });
            trigger.triggers.Add(exitEntry);

            // Add click event
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnPointerEnter()
    {
        // Stop any current animation
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Start hover animation
        currentAnimation = StartCoroutine(ScaleAnimation(hoverScale));
    }

    public void OnPointerExit()
    {
        // Stop any current animation
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Return to original scale
        currentAnimation = StartCoroutine(ScaleAnimation(1f));
    }

    public void OnClick()
    {
        // Stop any current animation
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Start click animation sequence
        currentAnimation = StartCoroutine(ClickAnimation());
    }

    private IEnumerator ScaleAnimation(float targetScale)
    {
        float startTime = Time.time;
        float endTime = startTime + animationDuration;
        Vector3 startScale = transform.localScale;
        Vector3 targetVector = originalScale * targetScale;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, targetVector, t);
            yield return null;
        }

        transform.localScale = targetVector;
        currentAnimation = null;
    }

    private IEnumerator ClickAnimation()
    {
        // Shrink
        float startTime = Time.time;
        float halfDuration = animationDuration / 2;
        float endTime = startTime + halfDuration;
        Vector3 startScale = transform.localScale;
        Vector3 clickVector = originalScale * clickScale;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / halfDuration;
            transform.localScale = Vector3.Lerp(startScale, clickVector, t);
            yield return null;
        }

        transform.localScale = clickVector;

        // Expand
        startTime = Time.time;
        endTime = startTime + halfDuration;
        startScale = transform.localScale;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / halfDuration;
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        currentAnimation = null;
    }
}

public class MenuPanelAnimator : MonoBehaviour
{
    [Header("Panel Animation")]
    public float fadeInDuration = 0.5f;
    public float slideDistance = 50f;
    public bool slideFromBottom = true;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 targetPosition;

    void Awake()
    {
        // Get components
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
        targetPosition = rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        // Only animate when in play mode
        if (Application.isPlaying)
        {
            StartAnimation();
        }
    }

    public void StartAnimation()
    {
        // Reset initial state
        canvasGroup.alpha = 0;

        Vector2 startPosition;
        if (slideFromBottom)
        {
            startPosition = new Vector2(targetPosition.x, targetPosition.y - slideDistance);
        }
        else
        {
            startPosition = new Vector2(targetPosition.x - slideDistance, targetPosition.y);
        }

        rectTransform.anchoredPosition = startPosition;

        // Start animations
        StartCoroutine(FadeIn());
        StartCoroutine(SlideIn());
    }

    private IEnumerator FadeIn()
    {
        float startTime = Time.time;
        float endTime = startTime + fadeInDuration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator SlideIn()
    {
        float startTime = Time.time;
        float endTime = startTime + fadeInDuration;
        Vector2 startPosition = rectTransform.anchoredPosition;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / fadeInDuration;
            // Use EaseOutQuad easing function for smoother animation
            float easedT = 1 - (1 - t) * (1 - t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 버튼에 붙이면 마우스 오버 시 크기가 커지는 효과
/// 시작 시 특정 버튼을 강조 가능
/// </summary>
public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("크기 설정")]
    [Tooltip("마우스 오버 시 크기 (1.0 = 원본)")]
    public float hoverScale = 1.3f;
    [Tooltip("크기 변화 속도")]
    public float scaleSpeed = 0.2f;

    [Header("흐림 설정")]
    [Tooltip("다른 버튼들의 밝기 (0=검은색, 1=원래색)")]
    [Range(0f, 1f)]
    public float dimmedBrightness = 0.5f;

    [Header("시작 시 강조")]
    [Tooltip("이 버튼을 시작할 때 강조?")]
    public bool highlightOnStart = false;

    private static List<ButtonScaleEffect> allButtons = new List<ButtonScaleEffect>();
    private Vector3 originalScale;
    private Vector3 targetScale;
    private CanvasGroup canvasGroup;

    private TextMeshProUGUI tmpText;
    private Color originalColor;
    private float targetBrightness = 1f;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            originalColor = tmpText.color;
        }

        allButtons.Add(this);

        // 시작 시 모든 버튼을 일단 흐리게 설정
        if (!highlightOnStart)
        {
            targetBrightness = dimmedBrightness;
            targetScale = originalScale; // 크기는 원래대로

            if (tmpText != null)
            {
                tmpText.color = originalColor * dimmedBrightness; // 즉시 어둡게
            }
        }
        else
        {
            // highlightOnStart인 버튼만 밝고 크게
            targetBrightness = 1f;
            targetScale = originalScale * hoverScale;
        }
    }

    private void OnEnable()
    {
        if (highlightOnStart)
        {
            Invoke(nameof(HighlightThisButton), 0.1f);
        }
    }

    private void HighlightThisButton()
    {
        targetScale = originalScale * hoverScale;
        targetBrightness = 1f;

        // 다른 버튼들은 흐리게
        foreach (var btn in allButtons)
        {
            if (btn != this)
            {
                btn.targetScale = btn.originalScale;
                btn.targetBrightness = dimmedBrightness;
            }
        }
    }

    private void Update()
    {
        // 부드럽게 크기 변화
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed / Time.unscaledDeltaTime * 60f);

        // 텍스트 색상을 회색으로
        if (tmpText != null)
        {
            Color targetColor = originalColor * targetBrightness;
            tmpText.color = Color.Lerp(tmpText.color, targetColor, scaleSpeed / Time.unscaledDeltaTime * 60f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 올린 버튼만 밝고 크게
        targetScale = originalScale * hoverScale;
        targetBrightness = 1f;

        // 다른 버튼들은 흐리고 작게
        foreach (var btn in allButtons)
        {
            if (btn != this)
            {
                btn.targetScale = btn.originalScale;
                btn.targetBrightness = dimmedBrightness;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스 뗐을 때 모든 버튼 흐리게
        targetScale = originalScale;
        targetBrightness = dimmedBrightness;

        foreach (var btn in allButtons)
        {
            btn.targetScale = btn.originalScale;
            btn.targetBrightness = dimmedBrightness;
        }
    }

    private void OnDestroy()
    {
        allButtons.Remove(this);
    }
}

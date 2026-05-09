using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 점수 변경 시 화면에 떠오르는 텍스트
/// +50 (빨강), -100 (파랑) 등을 표시하고 2초 후 사라짐
/// </summary>
[RequireComponent(typeof(Text))]
[RequireComponent(typeof(CanvasGroup))]
public class ScoreChangeText : MonoBehaviour
{
    private Text text;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [Header("애니메이션 설정")]
    [SerializeField] private float moveSpeed = 50f; // 아래로 이동 속도
    [SerializeField] private float fadeDuration = 2f; // 페이드 아웃 시간
    [SerializeField] private Color plusColor = Color.red; // + 점수 색상
    [SerializeField] private Color minusColor = Color.blue; // - 점수 색상

    [Header("폰트 설정")]
    [SerializeField] private Font customFont; // 커스텀 폰트 (비워두면 기본 폰트)
    [SerializeField] private int fontSize = 36; // 폰트 크기

    private void Awake()
    {
        text = GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 점수 변경 텍스트 초기화 및 애니메이션 시작
    /// </summary>
    public void Initialize(int scoreChange)
    {
        // 폰트 설정
        if (customFont != null)
        {
            text.font = customFont;
        }
        text.fontSize = fontSize;

        // 텍스트 설정
        if (scoreChange > 0)
        {
            text.text = $"+{scoreChange}";
            text.color = plusColor;
        }
        else
        {
            text.text = $"{scoreChange}";
            text.color = minusColor;
        }

        // 초기 알파값
        canvasGroup.alpha = 1f;

        // 애니메이션 시작
        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // 아래로 이동
            rectTransform.anchoredPosition += Vector2.down * moveSpeed * Time.deltaTime;

            // 페이드 아웃
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);

            yield return null;
        }

        // 2초 후 삭제
        Destroy(gameObject);
    }
}
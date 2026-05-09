using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreDisplayUI : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private ScoreManager scoreManager;
    private RectTransform rectTransform;

    [Header("표시 설정")]
    [SerializeField] private string prefix = "";
    [SerializeField] private float digitSpacing = 30f; // 한 자리 늘어날 때 왼쪽으로 이동할 거리

    private Vector2 startPosition;

    private void Awake()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition; // Inspector에서 지정한 시작 위치

        scoreManager = Resources.Load<ScoreManager>("ScoreManager");
        if (scoreManager == null)
        {
            Debug.LogError("[ScoreDisplayUI] ScoreManager를 찾을 수 없습니다.");
            return;
        }

        UpdateScoreDisplay();
    }

    private void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.whenScoreChanged += UpdateScoreDisplay;
    }

    private void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.whenScoreChanged -= UpdateScoreDisplay;
    }

    private void UpdateScoreDisplay()
    {
        int score = scoreManager.Score;
        scoreText.text = $"{prefix}{score}";

        // 자리수에 따라 위치 조정
        int numDigits = score.ToString().Length;
        float offset = (numDigits - 1) * digitSpacing;
        rectTransform.anchoredPosition = startPosition - new Vector2(offset, 0);
    }
}

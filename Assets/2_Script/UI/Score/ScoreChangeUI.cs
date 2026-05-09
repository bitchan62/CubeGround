using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScoreManager의 점수 변경을 감지하고 UI에 표시
/// Canvas에 배치하여 사용
/// </summary>
public class ScoreChangeUI : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private GameObject scoreChangeTextPrefab;
    [SerializeField] private Transform spawnParent; // 텍스트가 생성될 부모 Transform
    [SerializeField] private Vector2 spawnPosition = new Vector2(0, 100); // 생성 위치

    [Header("폰트 설정 (Prefab 없을 때만)")]
    [SerializeField] private Font defaultFont; // 기본 폰트
    [SerializeField] private int defaultFontSize = 36; // 기본 폰트 크기

    private ScoreManager scoreManager;

    private void Awake()
    {
        scoreManager = Resources.Load<ScoreManager>("ScoreManager");
        if (scoreManager == null)
        {
            Debug.LogError("[ScoreChangeUI] ScoreManager를 찾을 수 없다");
            return;
        }

        // spawnParent가 없으면 자기 자신을 사용
        if (spawnParent == null)
        {
            spawnParent = this.transform;
        }
    }

    private void OnEnable()
    {
        if (scoreManager != null)
        {
            scoreManager.whenScoreAdded += OnScoreAdded;
        }
    }

    private void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.whenScoreAdded -= OnScoreAdded;
        }
    }

    private void OnScoreAdded(int amount)
    {
        // 0점은 표시하지 않음
        if (amount == 0) return;

        // Prefab이 없으면 자동 생성
        if (scoreChangeTextPrefab == null)
        {
            CreateScoreChangeText(amount);
        }
        else
        {
            InstantiateFromPrefab(amount);
        }
    }

    /// <summary>
    /// Prefab에서 점수 변경 텍스트 생성
    /// </summary>
    private void InstantiateFromPrefab(int amount)
    {
        GameObject obj = Instantiate(scoreChangeTextPrefab, spawnParent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = spawnPosition;
        }

        ScoreChangeText scoreText = obj.GetComponent<ScoreChangeText>();
        if (scoreText != null)
        {
            scoreText.Initialize(amount);
        }
        else
        {
            Debug.LogWarning("[ScoreChangeUI] Prefab에 ScoreChangeText 컴포넌트가 없다");
        }
    }

    /// <summary>
    /// Prefab이 없을 때 런타임에 텍스트 생성
    /// </summary>
    private void CreateScoreChangeText(int amount)
    {
        // GameObject 생성
        GameObject obj = new GameObject("ScoreChangeText");
        obj.transform.SetParent(spawnParent);

        // RectTransform 설정
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = spawnPosition;
        rect.sizeDelta = new Vector2(200, 50);

        // Text 컴포넌트 추가
        Text text = obj.AddComponent<Text>();

        // 폰트 설정
        if (defaultFont != null)
        {
            text.font = defaultFont;
        }
        else
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        text.fontSize = defaultFontSize;
        text.alignment = TextAnchor.MiddleCenter;

        // CanvasGroup 추가
        obj.AddComponent<CanvasGroup>();

        // ScoreChangeText 추가 및 초기화
        ScoreChangeText scoreText = obj.AddComponent<ScoreChangeText>();
        scoreText.Initialize(amount);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : SingletonT<Fade>
{
    [Header("페이드 설정")]
    [Tooltip("페이드 아웃 시간 (초)")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("페이드 아웃 후 씬 로드 전 대기 시간 (초)")]
    public float delayBeforeLoad = 0.2f;

    [Tooltip("페이드 인 시간 (초) - 새 씬에서")]
    public float fadeInDuration = 0.5f;

    private Image fadeImage;
    private Coroutine fadeCoroutine;
    private Canvas fadeCanvas;
    private ScoreManager scoreManager;

    public static bool isRestarting = false; // ⭐ private → public static으로 변경

    protected override void Awake()
    {
        base.Awake();

        // ScoreManager 로드
        scoreManager = Resources.Load<ScoreManager>("ScoreManager");

        // Canvas 생성
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Image 생성
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 1); // 시작은 검정색
        fadeImage.raycastTarget = false; // 레이캐스트 타겟에서 제외

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    public void TransitionToScene(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[Fade] 씬 이름이 비어있습니다!");
            return;
        }

        Debug.Log($"[Fade] 씬 전환 시작: {targetSceneName}");

        string currentSceneName = SceneManager.GetActiveScene().name;

        // 새로운 씬으로 갈 때, 지금 점수를 목표 씬의 시작 점수로 저장
        if (scoreManager != null && targetSceneName != currentSceneName)
        {
            scoreManager.SaveSceneStartScoreForce(targetSceneName, scoreManager.Score);
            Debug.Log($"[Fade] {targetSceneName} 진입 시 점수를 {scoreManager.Score}로 저장");
        }

        StartFadeOut(fadeOutDuration);

        float totalDelay = fadeOutDuration + delayBeforeLoad;
        Timer.Instance.StartTimer(this, "SceneTransition", totalDelay, () => LoadScene(targetSceneName));
    }

    public void RestartCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[Fade] 현재 씬 재시작: {currentSceneName}");

        // 재시작 전에 점수 복원
        if (scoreManager != null)
        {
            scoreManager.RestoreSceneStartScore(currentSceneName);
        }

        // 재시작 플래그 설정
        isRestarting = true;
        TransitionToScene(currentSceneName);
    }

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;

        if (Timer.Instance != null)
        {
            Timer.Instance.StopAllCoroutines();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ForceReset();
        }

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.HideBossHealthBar();
        }

        if (FindObjectOfType<SceneMusicPlayer>() is SceneMusicPlayer bgm)
            bgm.StopMusic();

        Debug.Log($"[Fade] 씬 로드 실행: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[Fade] 씬 로드 완료: {scene.name} - 페이드 인 시작");

        isRestarting = false; // 재시작 플래그 해제
        StartFadeIn(fadeInDuration);
    }

    public void StartFadeOut(float duration)
    {
        fadeImage.gameObject.SetActive(true);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(0, 1, duration));
    }

    public void StartFadeIn(float duration)
    {
        fadeImage.gameObject.SetActive(true);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(1, 0, duration));
    }

    private IEnumerator FadeCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, to);

        if (to == 0)
        {
            fadeImage.gameObject.SetActive(false);
        }

        fadeCoroutine = null;
    }
}
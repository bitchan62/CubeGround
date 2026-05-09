using UnityEngine;

/// <summary>
/// 게임 상태에 따라 마우스 커서 표시/숨김을 자동으로 관리
/// - 메뉴 씬이나 일시정지 상태에서는 커서 표시
/// - 게임 플레이 중에는 투명 커서로 변경 (완전히 숨김)
/// - 씬 전환 시에도 자동으로 유지됨 (SingletonT 패턴)
/// </summary>
public class CursorManager : SingletonT<CursorManager>
{
    [Header("커서 이미지")]
    [Tooltip("일반 커서")]
    public Texture2D normalCursor;

    [Tooltip("투명 커서")]
    public Texture2D transparentCursor;

    /// <summary>
    /// 현재 UI가 활성화되어 있는지 여부
    /// </summary>
    private bool isUIActive = false;

    protected override void Awake()
    {
        base.Awake();

        // 투명 커서 자동 생성
        transparentCursor = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        transparentCursor.SetPixel(0, 0, new Color(0, 0, 0, 0));
        transparentCursor.Apply();
    }


    /// <summary>
    /// 시작 시 커서 상태 초기화
    /// </summary>
    private void Start()
    {
        CheckAndUpdateCursor();
    }

    /// <summary>
    /// 씬 로드 이벤트 구독
    /// </summary>
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// 씬 로드 이벤트 구독 해제
    /// </summary>
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 오브젝트 파괴 시 이벤트 정리
    /// </summary>
    protected override void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    /// <summary>
    /// 씬이 로드될 때 호출됨
    /// 0.1초 대기 후 커서 상태 확인 (씬 초기화 완료 대기)
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (Timer.Instance != null)
        {
            Timer.Instance.StartTimer(this, 0.1f, CheckAndUpdateCursor);
        }
        else
        {
            CheckAndUpdateCursor();
        }
    }

    /// <summary>
    /// 매 프레임마다 커서 상태 확인 및 업데이트
    /// </summary>
    private void Update()
    {
        CheckAndUpdateCursor();
    }

    /// <summary>
    /// 현재 게임 상태를 확인하고 커서를 표시하거나 숨김
    /// - 메뉴 씬이면 커서 표시
    /// - Time.timeScale이 0이면 (일시정지/게임오버) 커서 표시
    /// - 그 외에는 투명 커서로 변경
    /// </summary>
    private void CheckAndUpdateCursor()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 1. 메뉴 씬인지 확인 (MainMenu 또는 이름에 "Menu" 포함)
        bool isMenuScene = (currentScene == "MainMenu" || currentScene.Contains("Menu"));

        // 2. 게임이 일시정지 상태인지 확인 (Pause Menu, Game Over)
        bool isGamePaused = (Time.timeScale == 0f);

        // 3. UI가 활성화되어야 하는 상황 판단
        isUIActive = isMenuScene || isGamePaused;

        // 4. 커서 표시/숨김 적용
        if (isUIActive)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
    }

    /// <summary>
    /// 커서를 표시 (일반 커서로 변경)
    /// UI 조작이 필요한 상황에서 사용
    /// </summary>
    private void ShowCursor()
    {
        // 일반 커서로 변경 (null이면 시스템 기본 커서)
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// 커서를 숨김 (투명 커서로 변경)
    /// 게임 플레이 중에 사용
    /// </summary>
    private void HideCursor()
    {
        // 투명 커서로 변경
        Cursor.SetCursor(transparentCursor, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;  // visible은 true로 유지 (이벤트 작동 위해)
        //Cursor.lockState = CursorLockMode.Confined;
        Cursor.lockState = CursorLockMode.Locked; // <- 사용감 상 락이 맞는 거 같음
    }
}
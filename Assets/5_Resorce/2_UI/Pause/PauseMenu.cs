using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI 패널들")]
    public GameObject mainPausePanel;
    public GameObject volumePanel;

    private bool isPaused = false;

    // <- 임시 static 필드
    // 3 보스 네크로 사망 시 2초 간 게임 슬로우(+키보드 입력 막기) 용도
    public static bool isCanPause = true;

    void Start()
    {
        if (mainPausePanel != null)
            mainPausePanel.SetActive(false);
        if (volumePanel != null)
            volumePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isCanPause)
        {
            // 씬 전환 중이면 ESC 무시
            if (SceneMoveButton.IsTransitioning)
            {
                Debug.Log("[PauseMenu] 씬 전환 중이라 ESC 무시");
                return;
            }

            // 메인 메뉴에서는 ESC 무시
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene == "MainMenu")  // 메인 메뉴 씬 이름 (필요시 수정)
            {
                return;
            }

            // 랭킹 UI 열려있으면 ESC 무시
            if (RankingUI.Instance != null && RankingUI.Instance.IsRankingPanelActive())
            {
                Debug.Log("[PauseMenu] 랭킹 화면 중이라 ESC 무시");
                return;
            }


            if (GameOverUI.Instance != null &&
                GameOverUI.Instance.gameOverPanel != null &&
                GameOverUI.Instance.gameOverPanel.activeSelf)
            {
                Debug.Log("[PauseMenu] 게임오버 중이라 ESC 무시");
                return;
            }

            // Debug.Log("ESC 호출");

            if (volumePanel != null && volumePanel.activeSelf)
            {
                Debug.Log("ESC 호출 : ShowMainPausePanel");
                ShowMainPausePanel();
            }
            else
            {
                Debug.Log("ESC 호출 : TogglePause");
                TogglePause();
            }

        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            // 닫기 - Resume 호출
            Resume();
        }
        else
        {
            // 열기
            isPaused = true;
            Time.timeScale = 0f;

            if (mainPausePanel != null)
            { mainPausePanel.SetActive(true); }

            if (volumePanel != null)
            { volumePanel.SetActive(false); }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PauseAllSounds();
            }
        }
    }

    public void Resume()
    {
        // 클릭 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }

        isPaused = false;
        Time.timeScale = 1f;

        if (mainPausePanel != null)
        { mainPausePanel.SetActive(false); }

        if (volumePanel != null)
        { volumePanel.SetActive(false); }

        if (SoundManager.Instance != null)
        { SoundManager.Instance.ResumeAllSounds(); }
    }

    public void ShowVolumePanel()
    {
        // 클릭 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }

        if (mainPausePanel != null)
        { mainPausePanel.SetActive(false); }

        if (volumePanel != null)
        { volumePanel.SetActive(true); }
    }

    public void ShowMainPausePanel()
    {
        // 클릭 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }

        if (volumePanel != null)
        { volumePanel.SetActive(false); }

        if (mainPausePanel != null)
        { mainPausePanel.SetActive(true); }
    }

}
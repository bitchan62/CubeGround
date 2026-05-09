using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneMoveButton : MonoBehaviour
{
    public enum TargetScene
    {
        This,
        MainMenu,
        Tutorial,
        Scene1,
        Scene1_Boss,
        Scene2,
        Scene3
    }

    [Header("이 버튼을 누를 경우 이동할 씬")]
    [SerializeField]
    private TargetScene nextScene;

    [Header("재시작 모드 (현재 씬을 처음부터)")]
    [SerializeField]
    private bool isRestartMode = false; // 추가

    public static bool IsTransitioning => isTransitioning;

    private static bool isTransitioning = false;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ClickEvent);
        }
    }

    private string SceneName(TargetScene scene)
    {
        string nextSceneName;
        switch (scene)
        {
            case TargetScene.This: nextSceneName = SceneManager.GetActiveScene().name; break;
            case TargetScene.MainMenu: nextSceneName = "MainMenu"; break;
            case TargetScene.Tutorial: nextSceneName = "0_Play"; break;
            case TargetScene.Scene1: nextSceneName = "1_Play"; break;
            case TargetScene.Scene1_Boss: nextSceneName = "1_Play_Boss"; break;
            case TargetScene.Scene2: nextSceneName = "2_Play"; break;
            case TargetScene.Scene3: nextSceneName = "3_Play"; break;
            default: nextSceneName = "TestRoom"; break;
        }
        return nextSceneName;
    }

    private void ClickEvent()
    {
        if (isTransitioning)
        {
            Debug.Log("[SceneMoveButton] 이미 씬 전환 중 - 클릭 무시");
            return;
        }
        isTransitioning = true;

        // 클릭 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }

        Time.timeScale = 1.0f;

        string next = SceneName(nextScene);

        // 메인 메뉴로 갈때 스코어 매니저 초기화
        if (next == "MainMenu")
        {
            var scoreManager = Resources.Load<ScoreManager>("ScoreManager");
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }
        }

        if (Fade.Instance != null)
        {
            // 재시작 모드면 RestartCurrentScene 호출
            if (isRestartMode)
            {
                Debug.Log($"[SceneMoveButton] 재시작 모드 - 점수 복원");
                Fade.Instance.RestartCurrentScene();
            }
            else
            {
                Debug.Log($"[SceneMoveButton] 일반 전환 - {next}으로 이동");
                Fade.Instance.TransitionToScene(next);
            }
        }
    }

    private void OnDestroy()
    {
        isTransitioning = false;
    }
}
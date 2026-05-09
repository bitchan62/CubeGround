using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    [Header("씬 전환 설정")]
    [Tooltip("전환할 게임 씬 이름")]
    public string gameSceneName = "GameScene";

    [Header("입력 설정")]
    [Tooltip("게임 시작 키")]
    public KeyCode startKey = KeyCode.Space;

    [Header("UI 설정")]
    [Tooltip("'Press Space to Start' 텍스트")]
    public GameObject pressStartText;

    [Tooltip("텍스트 깜빡임 속도")]
    public float blinkSpeed = 1f;

    private bool hasStarted = false;

    private void Update()
    {
        if (Input.GetKeyDown(startKey) && !hasStarted)
        {
            StartGame();
        }
        BlinkText();
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[StartScreen] 게임 씬 이름 없다");
            return;
        }

        hasStarted = true;
        Debug.Log($"[StartScreen] {gameSceneName} 씬으로 전환");

        if (Fade.Instance != null)
        {
            Fade.Instance.TransitionToScene(gameSceneName);
        }
    }

    private void BlinkText()
    {
        if (pressStartText != null)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            pressStartText.SetActive(alpha > 0.5f);
        }
    }
}
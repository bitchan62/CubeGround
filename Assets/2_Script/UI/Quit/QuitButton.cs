using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼에 붙이면 게임 종료 기능을 자동으로 추가
/// </summary>
public class QuitButton : MonoBehaviour
{
    // 더블클릭 방지
    private bool isQuitting = false;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(QuitGame);
        }
    }

    private void QuitGame()
    {
        // 이미 종료 중이면 무시
        if (isQuitting)
        {
            Debug.Log("[QuitButton] 이미 종료 중 - 클릭 무시");
            return;
        }

        isQuitting = true;

        // 클릭 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }

        Debug.Log("[QuitButton] 게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
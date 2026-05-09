using UnityEngine;
using UnityEngine.UI;

public class ResetButtonAuto : MonoBehaviour
{
    [SerializeField] private Button resetButton;           // 버튼 연결
    [SerializeField] private RankingManager rankingManager; // 랭킹 매니저 연결

    private void Awake()
    {
        // 버튼이 연결되어 있다면 클릭 이벤트 등록
        if (resetButton != null && rankingManager != null)
        {
            resetButton.onClick.AddListener(() =>
            {
                rankingManager.ResetRankings();
            });
        }
        else
        {
            Debug.LogWarning("ResetButtonAuto 설정이 완료되지 않았습니다!");
        }
    }
}

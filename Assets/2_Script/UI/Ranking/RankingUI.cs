using UnityEngine;
using TMPro; 
using System.Collections.Generic;

public class RankingUI : MonoBehaviour
{
    public static RankingUI Instance { get; private set; }

    [Header("UI")]
    public GameObject rankingPanel;
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI rankingText;
    public TextMeshProUGUI rank1Text;
    public TextMeshProUGUI rank2Text;
    public TextMeshProUGUI rank3Text;
    public TextMeshProUGUI rank4Text;
    public TextMeshProUGUI rank5Text;

    [Header("매니저")]
    public RankingManager rankingManager;
    private void Awake()
    {
        Instance = this;
    }

    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = Resources.Load<ScoreManager>("ScoreManager");
        rankingPanel.SetActive(false);
    }

    // 랭킹 패널 활성화 여부 체크
    public bool IsRankingPanelActive()
    {
        return rankingPanel != null && rankingPanel.activeSelf;
    }


    public void ShowRanking()
    {
        Debug.Log("[RankingUI] ShowRanking 호출됨!");

        if (scoreManager == null)
        {
            Debug.LogError("[RankingUI] scoreManager가 null입니다!");
            return;
        }

        int score = scoreManager.Score;
        Debug.Log($"[RankingUI] 점수: {score}");

        if (rankingManager == null)
        {
            Debug.LogError("[RankingUI] rankingManager가 null입니다!");
            return;
        }

        int rank = rankingManager.AddScore(score);
        Debug.Log($"[RankingUI] 등수: {rank}");

        if (scoreText == null)
        {
            Debug.LogError("[RankingUI] scoreText가 null입니다!");
            return;
        }
        scoreText.text = $"점수 : {score} 점";

        if (rankingText == null)
        {
            Debug.LogError("[RankingUI] rankingText가 null입니다!");
            return;
        }

        if (rank <= 5)
            rankingText.text = $"랭킹 : {rank} 등";
        else
            rankingText.text = "랭킹 :     -";

        List<int> top5 = rankingManager.GetTop5();

        UpdateRankText(rank1Text, 1, top5);
        UpdateRankText(rank2Text, 2, top5);
        UpdateRankText(rank3Text, 3, top5);
        UpdateRankText(rank4Text, 4, top5);
        UpdateRankText(rank5Text, 5, top5);

        if (rankingPanel == null)
        {
            Debug.LogError("[RankingUI] rankingPanel이 null입니다!");
            return;
        }

        Time.timeScale = 0f;
        rankingPanel.SetActive(true);
        Debug.Log("[RankingUI] 패널 활성화 완료!");
    }

    private void UpdateRankText(TextMeshProUGUI text, int rank, List<int> scores)
    {
        if (rank <= scores.Count)
            text.text = $"{rank} 등     {scores[rank - 1]} 점";
        else
            text.text = $"{rank}        -";
    }
}
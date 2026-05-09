using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    private const string SAVE_KEY = "Rankings";

    // Update 추가
    private void Update()
    {
        // Ctrl + T 동시에 누르면 랭킹 초기화
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKeyDown(KeyCode.T))
        {
            ResetRankings();
            Debug.Log(" 랭킹 초기화 실행");
        }
    }

    public int AddScore(int newScore)
    {
        List<int> scores = LoadScores();
        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a));
        SaveScores(scores);
        return scores.IndexOf(newScore) + 1;
    }

    public List<int> GetTop5()
    {
        List<int> scores = LoadScores();
        return scores.Take(5).ToList();
    }

    public void ResetRankings()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[RankingManager] 랭킹 초기화 완료");
    }

    private List<int> LoadScores()
    {
        string data = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(data)) return new List<int>();
        return data.Split(',').Select(int.Parse).ToList();
    }

    private void SaveScores(List<int> scores)
    {
        string data = string.Join(",", scores);
        PlayerPrefs.SetString(SAVE_KEY, data);
        PlayerPrefs.Save();
    }
}
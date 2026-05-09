using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreManager", menuName = "Game Data/ScoreManager")]
public class ScoreManager : ScriptableObject
{
    [SerializeField]
    private int score = 0;

    // 씬별 시작 점수 저장
    private Dictionary<string, int> sceneStartScores = new Dictionary<string, int>();

    // 사망 여부 플래그
    [System.NonSerialized]
    public bool isDiedBeforeRestart = false;

    // 스코어 바뀔 때마다 이벤트
    public event Action whenScoreChanged;
    public event Action<int> whenScoreAdded; // 점수 변경 UI용

    public int Score
    {
        get { return score; }
        set
        {
            score = Mathf.Max(0, value); // 0 이하로 안 내려가게
            whenScoreChanged?.Invoke();
        }
    }

    public void AddScore(int amount)
    {
        // 재시작 중이면 점수 변경 무시
        if (Fade.isRestarting)
        {
            Debug.Log("[ScoreManager] 재시작 중이라 점수 변경 무시");
            return;
        }

        Score += amount;
        whenScoreAdded?.Invoke(amount);
    }

    public void ResetScore()
    {
        Score = 0;
        sceneStartScores.Clear();
    }

    // 해당 씬의 시작 점수가 저장되어 있는지 확인
    public bool HasSceneStartScore(string sceneName)
    {
        return sceneStartScores.ContainsKey(sceneName);
    }

    // 씬 시작 점수 저장/업데이트
    public void SaveSceneStartScoreForce(string sceneName, int score)
    {
        sceneStartScores[sceneName] = score;
        Debug.Log($"[ScoreManager] {sceneName} 시작 점수 저장: {score}");
    }

    // 씬 재시작 시 점수 복원
    public void RestoreSceneStartScore(string sceneName)
    {
        if (sceneStartScores.ContainsKey(sceneName))
        {
            int startScore = sceneStartScores[sceneName];
            Score = startScore; // 그냥 시작 점수로 복원 (사망 여부 무관)
            Debug.Log($"[ScoreManager] {sceneName} 재시작: {Score}");

            // 플래그 초기화
            isDiedBeforeRestart = false;
        }
        else
        {
            Debug.Log($"[ScoreManager] {sceneName}의 시작 점수 없음");
        }
    }
}
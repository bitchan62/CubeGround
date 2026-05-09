using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : SingletonT<GameOverUI>
{
    [Header("UI 요소")]
    [SerializeField] public GameObject gameOverPanel;
    public GameObject player;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ConnectToPlayer()
    {
        if (player != null)
        {
            PlayerDamageReaction damageReaction = player.GetComponent<PlayerDamageReaction>();
            if (damageReaction != null)
            {
                damageReaction.whenPlayerDie += ShowGameOverUI;
            }
        }
    }


    private void ShowGameOverUI()
    {
        Time.timeScale = 0f;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseAllSounds();
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}
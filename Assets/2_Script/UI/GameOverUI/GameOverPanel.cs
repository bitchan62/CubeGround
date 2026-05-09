using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("GameOverPanel awake");
        GameOverUI.Instance.gameOverPanel = this.gameObject;
        gameObject.SetActive(false);

        Timer.Instance.StartTimer(this, 0.1f, GameOverUI.Instance.ConnectToPlayer);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스태미나 아이콘 기반 UI (싱글톤 방식)
/// </summary>
public class StaminaUI : SingletonT<StaminaUI>
{
    private StaminaAction staminaAction;

    [Header("스태미나 아이콘 설정")]
    [SerializeField] private Transform staminaContainer;
    [SerializeField] private GameObject staminaIconPrefab;

    [Header("스태미나 이미지들")]
    [SerializeField] private Sprite fullStaminaSprite;
    [SerializeField] private Sprite emptyStaminaSprite;

    private List<Image> staminaIcons = new List<Image>();
    private int maxStamina = 0;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ConnectToPlayer();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        DisconnectFromPlayer();
    }

    protected override void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        DisconnectFromPlayer();
        base.OnDestroy();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ConnectToPlayer();
    }

    private void ConnectToPlayer()
    {
        DisconnectFromPlayer();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            staminaAction = player.GetComponent<StaminaAction>();
            if (staminaAction != null)
            {
                staminaAction.whenStaminaChanged.AddListener(UpdateStaminaUI);
                SetupStaminaIcons();
                UpdateStaminaUI();
                Debug.Log("StaminaUI: 플레이어 연결 완료");
            }
        }
    }

    private void DisconnectFromPlayer()
    {
        if (staminaAction != null)
        {
            staminaAction.whenStaminaChanged.RemoveListener(UpdateStaminaUI);
            staminaAction = null;
        }
    }

    private void SetupStaminaIcons()
    {
        if (staminaAction == null) return;

        maxStamina = staminaAction.maxStaminaValue;

        ClearIcons();

        for (int i = 0; i < maxStamina; i++)
        {
            CreateIcon();
        }
    }

    private void CreateIcon()
    {
        GameObject iconObj = null;

        if (staminaIconPrefab != null)
        {
            iconObj = Instantiate(staminaIconPrefab, staminaContainer);
        }
        else
        {
            iconObj = new GameObject("StaminaIcon");
            iconObj.transform.SetParent(staminaContainer);
            iconObj.AddComponent<Image>();
        }

        Image iconImage = iconObj.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = fullStaminaSprite;
            staminaIcons.Add(iconImage);
        }

        iconObj.transform.localScale = Vector3.one;
    }

    private void ClearIcons()
    {
        foreach (Image icon in staminaIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        staminaIcons.Clear();
    }

    private void UpdateStaminaUI()
    {
        if (staminaAction == null || staminaIcons.Count == 0) return;

        int currentStamina = staminaAction.stamina;

        for (int i = 0; i < staminaIcons.Count; i++)
        {
            Sprite targetSprite = i < currentStamina ? fullStaminaSprite : emptyStaminaSprite;
            staminaIcons[i].sprite = targetSprite;
        }
    }

    public void ConnectToNewPlayer() => ConnectToPlayer();
    public void ManualUpdateStamina() => UpdateStaminaUI();

    [ContextMenu("테스트 - 스태미나 사용")]
    public void TestUseStamina()
    {
        staminaAction?.UseStamina(1);
    }

    [ContextMenu("테스트 - 스태미나 회복")]
    public void TestRecoverStamina()
    {
        staminaAction?.RecoverStamina(1);
    }
}
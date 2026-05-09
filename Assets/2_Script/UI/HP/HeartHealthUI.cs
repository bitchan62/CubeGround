using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartHealthUI : SingletonT<HeartHealthUI>
{
    private DamageReaction damageReaction;

    [Header("하트 설정")]
    [SerializeField] private Transform heartContainer;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private int healthPerHeart = 2;

    [Header("하트 이미지들")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("텍스트 (옵션)")]
    [SerializeField] private TextMeshProUGUI healthText;

    private List<Image> heartImages = new List<Image>();
    private int maxHearts = 0;

    // 추가됨: 마지막 체력 기억용 변수
    private int lastHealth = -1;

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
            damageReaction = player.GetComponent<DamageReaction>();
            if (damageReaction != null)
            {
                damageReaction.whenHealthChange.AddListener(UpdateHeartUI);
                SetupHearts();
                UpdateHeartUI();
            }
        }
    }

    private void DisconnectFromPlayer()
    {
        if (damageReaction != null)
        {
            damageReaction.whenHealthChange.RemoveListener(UpdateHeartUI);
            damageReaction = null;
        }
    }

    private void SetupHearts()
    {
        if (damageReaction == null) return;

        maxHearts = Mathf.CeilToInt((float)damageReaction.maxHealthPoint / healthPerHeart);

        ClearHearts();

        for (int i = 0; i < maxHearts; i++)
        {
            CreateHeart();
        }
    }

    private void CreateHeart()
    {
        GameObject heartObj = null;

        if (heartPrefab != null)
        {
            heartObj = Instantiate(heartPrefab, heartContainer);
        }
        else
        {
            heartObj = new GameObject("Heart");
            heartObj.transform.SetParent(heartContainer);
            heartObj.AddComponent<Image>();
        }

        Image heartImage = heartObj.GetComponent<Image>();
        if (heartImage != null)
        {
            heartImage.sprite = fullHeartSprite;
            heartImages.Add(heartImage);
        }

        heartObj.transform.localScale = Vector3.one;
    }

    private void ClearHearts()
    {
        foreach (Image heart in heartImages)
        {
            if (heart != null)
                Destroy(heart.gameObject);
        }
        heartImages.Clear();
    }

    private void UpdateHeartUI()
    {
        if (damageReaction == null || heartImages.Count == 0) return;

        int currentHealth = damageReaction.healthPoint;

        if (lastHealth != -1 && currentHealth != lastHealth)
        {
            if (currentHealth < lastHealth)
            {
                // 체력 감소: 깎인 하트만 흔들기
                int startIndex = Mathf.FloorToInt((float)currentHealth / healthPerHeart);
                if (startIndex >= 0 && startIndex < heartImages.Count)
                {
                    if (heartImages[startIndex] != null)
                        StartCoroutine(ShakeHeart(heartImages[startIndex]));
                }
            }
            else if (currentHealth > lastHealth)
            {
                // 체력 회복: 새로 찬 하트들 흔들기
                int healedAmount = currentHealth - lastHealth;
                int startIndex = Mathf.FloorToInt((float)lastHealth / healthPerHeart);
                int endIndex = Mathf.FloorToInt((float)(currentHealth - 1) / healthPerHeart);

                for (int i = startIndex; i <= endIndex && i < heartImages.Count; i++)
                {
                    if (heartImages[i] != null)
                        StartCoroutine(ShakeHeart(heartImages[i]));
                }
            }
        }
        lastHealth = currentHealth;

        // 기존 하트 업데이트
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] == null) continue;

            int heartMinHealth = i * healthPerHeart;
            int heartMaxHealth = (i + 1) * healthPerHeart;

            Sprite targetSprite;

            if (currentHealth >= heartMaxHealth)
            {
                targetSprite = fullHeartSprite;
            }
            else if (currentHealth > heartMinHealth)
            {
                targetSprite = halfHeartSprite;
            }
            else
            {
                targetSprite = emptyHeartSprite;
            }

            heartImages[i].sprite = targetSprite;
        }

        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null && damageReaction != null)
        {
            healthText.text = $"{damageReaction.healthPoint}/{damageReaction.maxHealthPoint}";
        }
    }

    // 추가됨: 하트 흔들림 코루틴
    private IEnumerator ShakeHeart(Image heart)
    {
        if (heart == null) yield break;

        Vector3 originalScale = heart.rectTransform.localScale;
        float duration = 0.15f;
        float strength = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float x = Mathf.Sin(elapsed * 50f) * strength;
            heart.rectTransform.localScale = originalScale + new Vector3(x, x, 0);
            yield return null;
        }

        heart.rectTransform.localScale = originalScale;
    }

    public void ConnectToNewPlayer() => ConnectToPlayer();
    public void ManualUpdateHealth() => UpdateHeartUI();

    [ContextMenu("테스트 - HP 감소")]
    public void TestDecreaseHealth()
    {
        damageReaction?.TakeDamage(1, null, 0f, 0f);
    }

    [ContextMenu("테스트 - HP 회복")]
    public void TestHealHealth()
    {
        damageReaction?.Heal(1);
    }

    public void SetHealthPerHeart(int health)
    {
        healthPerHeart = health;
        SetupHearts();
        UpdateHeartUI();
    }
}

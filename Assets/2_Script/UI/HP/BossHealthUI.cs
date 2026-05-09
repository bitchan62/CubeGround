using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : SingletonT<BossHealthUI>
{
    public FinalBossHp finalBossHp;

    private DamageReaction bossDamageReaction;
    private Boss currentBoss;

    [Header("보스 HP 게이지")]
    [SerializeField] private GameObject bossHealthPanel;
    [SerializeField] private Image farBackgroundImage;   // ← 맨 뒤 이미지 추가
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private Image overlayImage;

    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float showHideDuration = 0.3f;

    private Coroutine healthAnimCoroutine;
    private Coroutine showHideCoroutine;
    private float currentHealthRatio = 1f;
    private bool isShowing = false;


    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (bossHealthSlider != null)
        {
            bossHealthSlider.interactable = false;
        }

        // ⭐ DontDestroyOnLoad 상태를 현재 씬으로 되돌림
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
            gameObject,
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        if (bossHealthPanel != null)
            bossHealthPanel.SetActive(false);


        if (finalBossHp != null)
        {
            finalBossHp.whenDamaged -= UpdateFinalBossHp;
            finalBossHp.whenDamaged += UpdateFinalBossHp;
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        DisconnectFromBoss();
        StopAllAnimations();
    }

    protected override void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        DisconnectFromBoss();
        StopAllAnimations();
        base.OnDestroy();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (bossHealthPanel != null)
            bossHealthPanel.SetActive(false);
        isShowing = false;
    }


    public void UpdateFinalBossHp()
    {
        if (bossHealthSlider == null) return;

        //Debug.Log("UpdateFinalBossHp");

        float targetRatio = (float)finalBossHp.sharedHp / finalBossHp.originalHp;

        if (healthAnimCoroutine != null)
            StopCoroutine(healthAnimCoroutine);

        healthAnimCoroutine = StartCoroutine(AnimateHealthBar(targetRatio));
    }



    public void ConnectToNewBoss(Boss boss)
    {
        DisconnectFromBoss();

        if (boss == null)
        {
            Debug.LogWarning("BossHealthUI: 연결할 보스가 null입니다.");
            return;
        }

        currentBoss = boss;
        bossDamageReaction = boss.GetComponent<DamageReaction>();

        if (bossDamageReaction != null)
        {
            bossDamageReaction.whenHealthChange.AddListener(UpdateBossHealthUI);

            currentHealthRatio = 1f;
            UpdateBossHealthUI();

            bossDamageReaction.whenDie.Add(OnBossDefeated);
        }
    }

    private void DisconnectFromBoss()
    {
        if (bossDamageReaction != null)
        {
            bossDamageReaction.whenHealthChange.RemoveListener(UpdateBossHealthUI);
            bossDamageReaction.whenDie.Remove(OnBossDefeated);
            bossDamageReaction = null;
        }
        currentBoss = null;
    }

    private void OnBossDefeated()
    {
        HideBossHealthBar();
        Timer.Instance.StartTimer(this, "DisconnectBoss", showHideDuration + 0.1f, () => DisconnectFromBoss());
    }

    public void ShowBossHealthBar()
    {
        if (bossHealthPanel == null || isShowing) return;

        if (showHideCoroutine != null)
            StopCoroutine(showHideCoroutine);

        showHideCoroutine = StartCoroutine(ShowBossHealthBarCoroutine());
    }

    public void HideBossHealthBar()
    {
        if (bossHealthPanel == null || !isShowing) return;

        if (showHideCoroutine != null)
            StopCoroutine(showHideCoroutine);

        showHideCoroutine = StartCoroutine(HideBossHealthBarCoroutine());
    }

    private IEnumerator ShowBossHealthBarCoroutine()
    {
        bossHealthPanel.SetActive(true);
        isShowing = true;

        Vector3 originalScale = bossHealthPanel.transform.localScale;
        bossHealthPanel.transform.localScale = Vector3.zero;

        float elapsedTime = 0f;
        while (elapsedTime < showHideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / showHideDuration);
            bossHealthPanel.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        bossHealthPanel.transform.localScale = originalScale;
        showHideCoroutine = null;
    }

    private IEnumerator HideBossHealthBarCoroutine()
    {
        Vector3 originalScale = bossHealthPanel.transform.localScale;

        float elapsedTime = 0f;
        while (elapsedTime < showHideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = 1f - animationCurve.Evaluate(elapsedTime / showHideDuration);
            bossHealthPanel.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        bossHealthPanel.transform.localScale = Vector3.zero;
        bossHealthPanel.SetActive(false);
        isShowing = false;
        showHideCoroutine = null;
    }

    private void UpdateBossHealthUI()
    {
        if (bossHealthSlider == null || bossDamageReaction == null) return;

        float targetRatio = (float)bossDamageReaction.healthPoint / bossDamageReaction.maxHealthPoint;

        if (healthAnimCoroutine != null)
            StopCoroutine(healthAnimCoroutine);

        healthAnimCoroutine = StartCoroutine(AnimateHealthBar(targetRatio));
    }

    private IEnumerator AnimateHealthBar(float targetRatio)
    {
        float startRatio = currentHealthRatio;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            //  if ((finalBossHp == null || finalBossHp.sharedHp <= 0) &&
            //      (bossDamageReaction == null || bossDamageReaction.isDie))
            //  {
            //      yield break;
            //  }

            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / animationDuration);
            currentHealthRatio = Mathf.Lerp(startRatio, targetRatio, progress);

            bossHealthSlider.value = currentHealthRatio;

            yield return null;
        }

        currentHealthRatio = targetRatio;
        bossHealthSlider.value = currentHealthRatio;


        if (currentHealthRatio <= 0)
        {
            HideBossHealthBar();
        }
    }

    private void StopAllAnimations()
    {
        if (healthAnimCoroutine != null)
        {
            StopCoroutine(healthAnimCoroutine);
            healthAnimCoroutine = null;
        }
        if (showHideCoroutine != null)
        {
            StopCoroutine(showHideCoroutine);
            showHideCoroutine = null;
        }
    }
}
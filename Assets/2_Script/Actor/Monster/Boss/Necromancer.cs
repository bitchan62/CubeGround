using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Necromancer : Monster, IBossPattern
{
    public FinalBossHp finalBossHp;
    [Tooltip("보스 처치 후, 네크로맨서가 위치할 장소")]
    public Transform goalPos;
    public float downSpeed = 2f;

    [Header("처치 시 시간 배율 / 지속시간")]
    public float timeScaleWhenDie = 0.5f;
    public float dieSlowTime = 2f;

    [Header("보스 패턴 정보")]
    public BossPatternContext[] patternContexts;

    [Header("랭킹 UI")]
    public RankingUI rankingUI;

    [Header("랭킹 연출용 BGM")]
    public AudioClip rankingBgm;


    public override MonsterAttackStatus attackStatus
    {
        get
        {
            if (_attackStatus == null)
            { _attackStatus = new NecroAttackStatus(this); }
            return _attackStatus;
        }
    }
    
    // 밑으로 내려오는 상태

    public BossNecroDownStatus _downStatus;
    public BossNecroDownStatus downStatus
    {
        get
        {
            if (_downStatus == null)
            { _downStatus = new BossNecroDownStatus(this, goalPos, downSpeed); }
            return _downStatus;
        }
    }



    protected override void Awake()
    {
        isBoss = true;
        base.Awake();
        nowAttackKey = AttackName.Monster_MageSpellAttack;
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded -= finalBossHp.OnSceneLoaded;
        SceneManager.sceneLoaded += finalBossHp.OnSceneLoaded;
        BossPatternManager.Instance.finalBossHp = finalBossHp; ;
    }

    protected override void Start()
    {
        base.Start();

        foreach (var con in patternContexts)
        {
            con.Initialize(this);
            BossPatternManager.Instance.AddPattern(con);
        }

        // <- finalBossHp.whenDieEvent에다가 밑으로 내려오는 상태로 전환 넣기
        finalBossHp.whenDied += () => { SwitchStatus(downStatus); };

        // 임시 트리거
        Timer.Instance.StartTimer(this, 2f, BossPatternManager.Instance.PatternStartTrigger);
        Timer.Instance.StartTimer(this, "ShowBossHealthBar", 2f, BossHealthUI.Instance.ShowBossHealthBar);

        // 처치 시 슬로우
        damageReaction.whenDie.Add(() =>
        {
            PauseMenu.isCanPause = false;
            StartCoroutine(TimeLerp());

            Timer.Instance.StartTimer(this, dieSlowTime + 0.1f, () =>
            {
                Time.timeScale = 1f;
                PauseMenu.isCanPause = true;

                // 씬 뮤직 플레이어에 랭킹 음악 재생 요청
                SceneMusicPlayer sceneMusic = FindObjectOfType<SceneMusicPlayer>();
                if (sceneMusic != null)
                    sceneMusic.PlayRankingMusic();

                rankingUI.ShowRanking();
            });
        }, 1);


    }

    protected override void FixedUpdate() { }
    protected override void LateUpdate() { }

    public System.Action NextPattern { get; set; }

    void IBossPattern.PatternStart()
    { SwitchStatus(idleStatus); }

    protected override void Spawn()
    {
        TriggerAnimationPlayStatus spawnStatus = new TriggerAnimationPlayStatus(this, null, "Spawn");
        SwitchStatus(spawnStatus);
    }


    IEnumerator TimeLerp()
    {
        float elapsedTime = 0f;
        float startValue = Time.timeScale;
        float endValue = timeScaleWhenDie;

        while (elapsedTime < dieSlowTime)
        {
            Time.timeScale = Mathf.Lerp(startValue, endValue, elapsedTime / dieSlowTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}

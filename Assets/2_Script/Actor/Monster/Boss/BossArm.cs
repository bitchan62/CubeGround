using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArm : Monster, IBossPattern
{
    // public 보스
    // 생성될 때 보스 쪽에서 부여해줌

    // override DamageReaction
    // null이면 보스.DamageReaction GetComponent
    // <- 그러면 빨개지는 거는 어떻게 해야할?까용?

    [Header("점프 정보")]
    public float jumpHeight = 60f;
    public float jumpSpeed = 40f;
    public float fallSpeed = 40;
    public float jumpDelay = 1f;

    [Header("Boar 패턴 중 피해있을 장소")]
    public Transform avoidPos;

    [Tooltip("사망 시 가라앉는 정도")]
    public float fallHeight = 5f;

    // 이 팔이 다음 패턴으로 사용할 상태
    [HideInInspector]
    public IStatus myNextPatternStatus;



    // 점프 상태
    private BossArmJumpStatus _jumpStatus;
    public virtual BossArmJumpStatus jumpStatus
    {
        get
        {
            if (_jumpStatus == null)
            { _jumpStatus = new BossArmJumpStatus(this, jumpHeight, jumpSpeed, fallSpeed, jumpDelay, chaseSpeedWhenJump); }
            return _jumpStatus;
        }
    }


    public override MonsterMoveStatus moveStatus
    {
        get { return _moveStatus ??= new BossArmAvoidStatus(this, avoidPos); }
    }


    public BossArmAvoidAndAttackStatus _avoidJumpStatus;
    public BossArmAvoidAndAttackStatus avoidJumpStatus
    {
        get { return _avoidJumpStatus ??= new BossArmAvoidAndAttackStatus(this, avoidPos); }
    }



    public FinalBossHp finalBossHp;

    protected override void Spawn()
    {
        TriggerAnimationPlayStatus spawnStatus = new TriggerAnimationPlayStatus(this, null, "Spawn");
        SwitchStatus(spawnStatus);
    }

    protected override void Awake()
    {
        isBoss = true;
        base.Awake();
        nowAttackKey = AttackName.Monster_BossDropAttack;
        myNextPatternStatus = jumpStatus;

        System.Action tempAction = () => transform.position -= Vector3.up * Time.deltaTime * fallHeight;
        damageReaction.whenDie.Add(() => {
            if (damageReaction is MonsterDamageReaction monsterReaction)
            { Timer.Instance.StartRepeatTimer(this, "Die", monsterReaction.remainTime, tempAction); }
        });
    }

    protected override void Start()
    {
        base.Start();

        foreach (var con in patternContexts)
        {
            con.Initialize(this);
            BossPatternManager.Instance.AddPattern(con);
        }

        BoarWaveManager bossPatternManager = FindObjectOfType<BoarWaveManager>();
        if (bossPatternManager != null)
        {
            foreach(var pattern in bossPatternManager.wavePatterns)
            { pattern.whenDoPattern += () => { SwitchStatus(moveStatus); }; }
        }

        // 보스 Hp에 등록
        finalBossHp.Init(damageReaction);
    }

    protected override void FixedUpdate() { }

    protected override void LateUpdate() { }

    public Action NextPattern { get; set; }

    public void PatternStart()
    {
        SwitchStatus(myNextPatternStatus);
    }

    [Tooltip("점프 시 소리")]
    public EffectData jumpEffectData = new EffectData();

    [Tooltip("점프 중 추격 속도")]
    public float chaseSpeedWhenJump = 12f;


    [Header("보스 패턴 정보")]
    public BossPatternContext[] patternContexts;
}

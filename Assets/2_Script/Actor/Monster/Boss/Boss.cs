using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Boss : Monster
{
    public override MonsterIdleStatus idleStatus
    {
        get
        {
            if(_idleStatus == null)
            { _idleStatus = new BossIdleStatus(this); }
            return _idleStatus;
        }
    }

    // 점프 정보
    public float jumpHeight = 60f;
    public float jumpSpeed = 40f;
    public float fallSpeed = 40;

    // 점프 상태
    private BossJumpStatus _jumpStatus;
    public virtual BossJumpStatus jumpStatus
    {
        get
        {
            if (_jumpStatus == null)
            { _jumpStatus = new BossJumpStatus(this, jumpHeight, jumpSpeed, fallSpeed, chaseSpeedWhenJump); }
            return _jumpStatus;
        }
    }

    private BossStunStatus _stunStatus;
    public virtual BossStunStatus stunStatus
    {
        get
        {
            if ( _stunStatus == null)
            { _stunStatus = new BossStunStatus(this, selectStatus); }
            return _stunStatus;
        }
    }

    public override MonsterAttackStatus attackStatus
    {
        get
        {
            if (_attackStatus == null)
            { _attackStatus = new BossAttackStatus(this); }
            return _attackStatus;
        }
    }

    protected BossNextSelectStatus _selectStatus;
    public virtual BossNextSelectStatus selectStatus
    {
        get
        {
            if (_selectStatus == null)
            { _selectStatus = new BossNextSelectStatus(this); }
            return _selectStatus;
        }
    }

    protected override void Spawn()
    {
        TriggerAnimationPlayStatus spawnStatus = new TriggerAnimationPlayStatus(this, jumpStatus, "Spawn");
        SwitchStatus(spawnStatus);
    }


    // ==========================
    //        Unity 기본 메서드
    // ==========================
    protected override void Awake()
    {
        isBoss = true;
        base.Awake();

        // --- 스턴 이벤트 등록 ---
        nowAttackKey = AttackName.Monster_BossChargeAttack;
        BossChargeAttack bossChargeAttack = attackAction as BossChargeAttack;

        if (bossChargeAttack != null)
        { bossChargeAttack.stunEvent.Add(() => { SwitchStatus(stunStatus); }); }

        // // --- 점프 애니메이션 이벤트 등록 ---
        // foot.whenJumpEvent.Add(() => actorAnimator.SetAnimationParam("IsJump", true));
        // foot.whenGroundEvent.Add(() => actorAnimator.SetAnimationParam("IsJump", false));

        // --- 시작 공격 선택 ---
        nowAttackKey = AttackName.Monster_BossChargeAttack;
    }


    protected override void Start()
    {
        base.Start();
        DestructibleCube[] cubes = FindObjectsOfType<DestructibleCube>();
        foreach (DestructibleCube cube in cubes)
        { cube.SetBoss(this); }

        BossHealthUI.Instance.ConnectToNewBoss(this);
        Timer.Instance.StartTimer(this, "ShowBossHealthBar", 5f, () =>
        {
            BossHealthUI.Instance.ShowBossHealthBar();
        });
    }

    // <- 리팩토링 중 임시 타이머 (Status 전달용)
    // 사용처 : jumpStatus
    public void ActionTimer(string key, float duration, System.Action action)
    { Timer.Instance.StartTimer(this, key, duration, action); }


    // 패턴 관련
    public bool isWavePattern = false;

    // Wave 실행 위치
    [field: SerializeField] public Transform wavePos { get; private set; }

    // 점프 후, 대상 머리 위로 이동
    // <- 임시 명칭
    public void MovePosToTargetHead()
    {
        if (isWavePattern) { transform.position = new Vector3(wavePos.position.x, transform.position.y, wavePos.position.z); }
        else
        {
            Vector3 targetPos = Target.position;
            targetPos.x += UnityEngine.Random.Range(-0.1f, 0.1f);
            targetPos.z += UnityEngine.Random.Range(-0.1f, 0.1f);
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
            transform.position = targetPos;
        }

        // <- 그 다음 상태로 변경 (이후 wave패턴 제작 후 주석 해제)
        // isWavePattern = !isWavePattern;
    }


    // ==========================
    //        필드/프로퍼티
    // ==========================

    // 스턴 애니메이션 재생 속도 (배율)
    [Tooltip("스턴 애니메이션 속도 배율(예: 0.1 배율이면 10% 속도로 재생)")]
    [field: SerializeField] public float stunAnimationSpeed { get; private set; } = 0.4f;

    // 웨이브 생성 상태
    // <- 트리거 애니메이션 스테이터스
    // protected void WaveStatus()
    // {
    //     PlayTriggerAnimationOnce("DoWaveMaking");
    //     SwitchStatusWhenAnimationEnd("Wave_Making", IdleStatus);
    // }

    [Tooltip("점프 시 소리")]
    public EffectData jumpEffectData = new EffectData();

    [Tooltip("점프 중 추격 속도")]
    public float chaseSpeedWhenJump = 12f;
}

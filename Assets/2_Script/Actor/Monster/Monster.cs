using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// ===== [컴포넌트 요구] =====
[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DamageReaction))]
public abstract class Monster : Actor, IClearTrigger
{
    // ==============================
    // ■ 상태 관련 멤버
    // ==============================
    protected MonsterIdleStatus _idleStatus;
    public virtual MonsterIdleStatus idleStatus
    {
        get
        {
            if (_idleStatus == null)
            { _idleStatus = new MonsterIdleStatus(this); }
                
            return _idleStatus;
        }
    }

    protected MonsterMoveStatus _moveStatus;
    public virtual MonsterMoveStatus moveStatus
    {
        get
        {
            if (_moveStatus == null)
            { _moveStatus = new MonsterMoveStatus(this); }
            return _moveStatus;
        }
    }

    protected MonsterAttackStatus _attackStatus;
    public virtual MonsterAttackStatus attackStatus
    {
        get
        {
            if (_attackStatus == null)
            { _attackStatus = new MonsterAttackStatus(this); }
            return _attackStatus;
        }
    }

    // 현재 상태
    public IStatus nowStatus;

    // 상태 변경
    public void SwitchStatus(IStatus nextStatus)
    {
        // Debug.Log($"상태 변화 : {nowStatus} -> {nextStatus}");
        nowStatus?.Exit();
        nowStatus = nextStatus;
        nowStatus?.Enter();
    }

    // ==============================
    // ■ 타겟 관련
    // ==============================
    public Transform Target
    {
        get
        {
            return TargetManager.Instance.Target; ;
        }
    }

    protected virtual void Spawn()
    {
        TriggerAnimationPlayStatus spawnStatus = new TriggerAnimationPlayStatus(this, idleStatus, "Spawn");
        SwitchStatus(spawnStatus);
    }

    // ==============================
    // ■ 액션 관련
    // ==============================
    [HideInInspector]
    public ChaseAction chaseAction;    // 추격 액션
    protected bool isBoss = false;     // 보스 여부

    // ==============================
    // ■ Unity 기본 함수
    // ==============================
    // 초기화
    protected override void Awake()
    {
        base.Awake();

        // 피격/사망 상태
        TriggerAnimationPlayStatus HitStatus = new TriggerAnimationPlayStatus(this, idleStatus, "Hit", "IsHit");
        TriggerAnimationPlayStatus DieStatus = new TriggerAnimationPlayStatus(this, null, "Die", "IsDie");

        Spawn();

        if (!isBoss)
        { damageReaction?.whenHit.Add(() => { SwitchStatus(HitStatus); }); }
        damageReaction?.whenDie.Add(() => { SwitchStatus(DieStatus); }, 1);

        //  if (isBoss)
        //  { damageReaction?.whenDie.Add(() => { SwitchStatus(DieStatus); }, 1); }
        //  else
        //  { damageReaction?.whenDie.Add(() => { SwitchStatus(DieStatus); }); }

        // 낙사 처리
        if (GetComponent<FallingAction>() == null)
        {
            if (!isBoss) { gameObject.AddComponent<DestroyWhenFallingAction>(); }
            else
            {
                var temp = gameObject.AddComponent<RespawnWhenFallingAciton>();
                temp.FallDistance = 100f;
            }
        }

        // moveAction에서 chaseAction 분리
        chaseAction = moveAction as ChaseAction;
        if (chaseAction == null)
        { Debug.Log(this.gameObject.name + " : ChaseAction 아님"); }
    }

    // 시작 시 타겟 방향 바라보기
    protected virtual void Start()
    {
        Vector3 targetPos = Target.position;
        targetPos.y = this.transform.position.y;
        this.transform.LookAt(targetPos);

        damageReaction.whenDie.Add(() => { ClearTriggerListManager.Instance.Remove(this); }, 1);
    }

    //private void OnEnable()
    //{
    //    Spawn();
    //    if (Target != null)
    //    {
    //        //Debug.Log($"{Target.transform.root.name} 바라보기");
    //        Vector3 targetPos = Target.position;
    //        targetPos.y = this.transform.position.y;
    //        this.transform.LookAt(targetPos);
    //    }
    //}

    // 매 프레임마다 상태 표시 갱신 (디버그)
#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly] private string currentStateName;
#endif

    protected virtual void Update()
    {
        if (!damageReaction.isDie)
        {
            //Debug.Log($"{name} : Update");
            nowStatus?.Update();
        }

#if UNITY_EDITOR
        if (nowStatus == null)
        { currentStateName = "null"; }
        else
        { currentStateName = nowStatus.GetType().Name; }
#endif
    }

    // 이동 처리
    protected virtual void FixedUpdate()
    {
        if (moveAction.isMove)
        { moveAction.Move(); }
    }

    // 애니메이션 처리
    protected virtual void LateUpdate()
    {
        actorAnimator.SetAnimationParam("IsMove", moveAction.isMove);
    }

    //protected void OnDestroy()
    //{
    //    Debug.Log($"{name} : 파괴");
    //}

    // ==============================
    // ■ 공격 관련
    // ==============================
    // 공격 사거리 체크
    public bool CheckInAttackRange()
    {
        return (Target.position - this.transform.position).sqrMagnitude <=
               attackAction.attackRange * attackAction.attackRange;
    }

    // 공격 준비 상태
    public bool isInAttackRange { get; private set; } = false;
    public bool isFacing { get; private set; } = false;
    public bool isClear { get; private set; } = false;
    public bool isReadyToAttack
    {
        get
        {
            isInAttackRange = CheckInAttackRange();
            isFacing = chaseAction.IsFacingTarget();
            if (isInAttackRange)
            { isClear = chaseAction.isClearToTargetAsCash(); }
            else
            { isClear = false; }

            return isInAttackRange &&
                   isFacing &&
                   isClear;
        }
    }
}

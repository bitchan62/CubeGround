using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AttackAction2 : ActorAction, IAttackAction
{
    [Tooltip("공격 데이터")]
    public AttackData attackData = new AttackData();
    [Tooltip("넉백 데이터")]
    public KnockBackData knockBackData = new KnockBackData();

    [Tooltip("공격 이전에 발생시킬 이펙트")]
    public EffectData beforeAttackEffect = new EffectData();
    [Tooltip("공격과 함께 발생시킬 이펙트")]
    public EffectData doAttackEffect = new EffectData();

    [Tooltip("공격이 명중한 경우 발생시킬 이펙트")]
    public EffectData hitEffect = new EffectData();


    public float attackRange
    { get { return attackData.range; } }

    public AttackName attackName
    { get { return attackData.attackName; } }

    public int attackCost
    { get { return attackData.cost; } }


    protected override void Awake()
    {
        base.Awake();

        // 타겟태그 검사
        // 배정되지 않은 경우 : 기초적인 재배정
        if (attackData.targetTag == "")
        {
            if (thisActor.gameObject.tag == "Monster")
            { attackData.targetTag = "Player"; }

            else if (thisActor.gameObject.tag == "Player")
            { attackData.targetTag = "Monster"; }
        }
    }

    private void Start()
    {
        // attackData에 따른 ExitCallBack 세팅
        thisActor.actorAnimator.RegisterExitCallback(attackData.attackName, Exit);
    }

    protected virtual void OnEnable()
    {
        thisActor.actorAnimator.SetAnimationSpeed();
        thisActor.isAttacking = false;
    }

    // --- 이펙트 ---

    private void BeforeAttackEffect()
    {
        // Debug.Log($"{owner.name} : BeforeAttackEffect");

        //GameObject temp =
            beforeAttackEffect.Instantiate(thisActor.gameObject);
        //if (temp != null)
        //{
        //    Debug.Log($"{name} : beforeAttackEffect : {beforeAttackEffect.effectPrefab.name}");
        //}
    }

    private void DoAttackEffect()
    {
        // SoundManager.Instance.PlayMonsterAttackByType(owner.nowAttackKey);
        // SoundManager.Instance.PlayPlayerAttackByType(owner.nowAttackKey);

        //GameObject temp =
            doAttackEffect.Instantiate(thisActor.gameObject);
        //if (temp != null)
        //{
        //    Debug.Log($"{name} : doAttackEffect : {doAttackEffect.effectPrefab.name}");
        //}
    }


    // --- 애니메이션 이벤트 ---

    // 작동 시점
    public virtual void Do()
    {
        // 공격 활성화, projectile 생성, SetData 등
        DoAttackEffect();
    }

    // 애니메이션 어느 때든 종료 시점
    protected virtual void Exit()
    {
        // 애니메이션 속도 정상화, 공격 비활성화 등
        thisActor.actorAnimator.SetAnimationSpeed();
        thisActor.isAttacking = false;
        // Debug.Log($"{this.gameObject.name} : Exit 콜백");
    }

    // --- 실제 호출 대상 (Before) ---
    public virtual void Attack()
    {
        // Debug.Log($"{owner.name} : Attack");
        thisActor.isAttacking = true; // 공격 상태 true
        thisActor.actorAnimator.SetAnimationParam("DoAttack");
        thisActor.actorAnimator.SetAnimationSpeed(attackData.speed);
        BeforeAttackEffect();
    }

    public virtual void Cancel()
    {
        thisActor.actorAnimator.SetAnimationSpeed();
    }

}

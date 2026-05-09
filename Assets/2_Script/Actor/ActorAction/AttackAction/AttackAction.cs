using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//==================================================
// 공격 행동
//==================================================
abstract public class AttackAction : ActorAction, IAttackAction
{
    // 해당 공격의 명칭
    [field: SerializeField] public AttackName attackName { get; private set; }
    
    // 공격력
    [SerializeField] protected int attackDamage = 1;

    // 공격 사거리
    // Monster의 사거리 요소로도 사용 중
    [field: SerializeField] public float attackRange { get; protected set; } = 3f;

    // 공격 대상 태그 (해당 태그를 가진 오브젝트만 공격)
    [SerializeField] protected string targetTag = "";

    // 최대 히트 가능 횟수
    [SerializeField] protected int maxHitCount = 1;

    // <- 공격 대상의 레이어

    // 공격 간격 (== 공격 속도)
    [SerializeField] protected float attackRate = 0.5f;

    // 넉백 거리
    [SerializeField] protected float knockBackPower = 0f;
    [SerializeField] protected float knockBackHeight = 0f;

    // 공격 코스트
    [field: SerializeField] public int attackCost { get; set; } = 0;

    // 이펙트 프리펩
    [SerializeField] protected GameObject hitEffect = null;
    [SerializeField] protected float effectDestroyTime = 1f; // <- LeftTime 설정 고려

    // 공격 선딜레이
    [SerializeField] public float weaponBeforeDelay = 0.2f;
    [SerializeField] protected GameObject beforeDelayEffect = null;
    [SerializeField] protected Transform beforeDelayEffectPos = null;

    // 피격당했을 경우, 이 공격을 취소할 것인가?
    [SerializeField] protected bool isCancelWhenHit = false;


    protected override void Awake()
    {
        base.Awake();

        // 타겟태그 검사
        // 배정되지 않은 경우 : 기초적인 재배정
        if (targetTag == "")
        {
            if (gameObject.tag == "Monster") { targetTag = "Player"; }
            else if (gameObject.tag == "Player") { targetTag = "Monster"; }
        }

        // 공격 선딜레이가 공격 간격에 포함
        attackRate += weaponBeforeDelay;

        // 공격받았을 경우 캔슬 여부
        if (isCancelWhenHit)
        {
            thisActor?.damageReaction?.whenHit.Add(CancelAttack);
            thisActor?.damageReaction?.whenDie.Add(CancelAttack);
        }
    }


    // 실제로 호출할 메서드
    public void Attack()
    {
        // 공격 선딜레이 중 동작 (이펙트)
        BeforeAttack();

        // 선딜레이 후 발생
        Timer.Instance.StartTimer(this, "_DoAttack", weaponBeforeDelay, DoAttack);
    }


    // 공격 캔슬
    protected virtual void CancelAttack()
    { Timer.Instance.StopTimer(this, "_DoAttack"); }


    public virtual void Cancel()
    {
        //Debug.Log($"{owner.name} : Attack Cancel");
        CancelAttack();
    }


    // 공격 전 동작 (공격 전 이펙트)
    protected virtual void BeforeAttack()
    {
        if (beforeDelayEffect != null && beforeDelayEffectPos != null)
        {
            GameObject effect = Instantiate(beforeDelayEffect, beforeDelayEffectPos.position, beforeDelayEffectPos.rotation);
            Timer.Instance.StartRepeatTimer(this, "_BeforeDelayEffect", weaponBeforeDelay,
                () => { if (effect != null) { effect.transform.position = beforeDelayEffectPos.position; } });

            Destroy(effect, weaponBeforeDelay);
        }
    }

    // 실제 Attack 구현
    protected virtual void DoAttack()
    {
        //사운드 매니저용 실험용 
        SoundManager.Instance.PlayPlayerAttackByType(attackName);
        SoundManager.Instance.PlayMonsterAttackByType(attackName);
    }
}
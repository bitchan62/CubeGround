using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


public class DropAttackAction2 : MeleeAttackAction2
{
    protected int originalLayer = -1; // 원래 레이어
    protected int dropAttackLayer;

    // dropAttack 이전에 땅에 닿은 적이 있는지 검사
    protected bool isRanded = false;

    [Tooltip("착지 시 이펙트 정보")]
    public EffectData groundedEffectData;

    [Tooltip("공격 위치 기준점, null이면 약간 앞쪽 위치")]
    public Transform groundAttackCenterPos;
    
    protected override void Awake()
    {
        base.Awake();
        originalLayer = gameObject.layer;
        dropAttackLayer = LayerMask.NameToLayer("IgnoreOtherActor");
        //weapon.whenHit.Add();
    }

    public override void Attack()
    {
        base.Attack();
        gameObject.layer = dropAttackLayer;

        // 낙하 딜레이
        System.Action action =
            () => {
                if (!thisActor.isRand)
                {
                    // 레이캐스트 정보
                    RaycastHit tempHit;
                    // 낙하 시도 여부
                    bool isCanUseDropAttack = Physics.Raycast(this.transform.position, Vector3.down, out tempHit, 20);

                    // --- 낙하 여부 ---
                    if (isCanUseDropAttack)
                    { thisActor.rigid.velocity = Vector3.down * 20f; } // <- 20f : 임의값
                }
            };
        Timer.Instance.StartTimer(this, "_낙하딜레이", 0.2f, action);

        // 착지 상태 판정
        isRanded = thisActor.isRand;
        if (!isRanded)
        {
            thisActor.foot.whenGroundEvent.Add(() => isRanded = true, 1);
        }
    }


    public void DoWeaponActive()
    {
        //Debug.Log($"{name} : DoWeaponActive");
        //Debug.Log($"{owner.name} : DoWeaponActive");
        Do();
    }

    public void DoDrop()
    {
        //Debug.Log($"{name} : DoDrop");

        // 지상에 착지한 적이 있는 경우: 즉시 사용
        if (isRanded)
        {
           // Debug.Log($"{owner.name} : DropAttack 착지 후 사용");
            DropAttack();
        }
        // 공중인 경우: 착지 시 사용
        else
        {
            //Debug.Log($"{owner.name} : DropAttack 즉시 사용");
            thisActor.actorAnimator.SetAnimationSpeed(0);
            thisActor.foot.whenGroundEvent.Add(DropAttack, 1);
        }
    }

    public void AfterDrop()
    {
        //Debug.Log($"{name} : AfterDrop");
        Cancel();
        isRanded = false;
    }


    // 일정 범위 이내의 모든 대상들에게 피해
    protected void DropAttack()
    {
        //Debug.Log($"{name} : DropAttack");

        // ----- 레이어 원복 (Drop 충돌 직후 처리) -----
        gameObject.layer = originalLayer;

        // 공격 위치 구하기
        Vector3 attackPos;

        // 지정된 위치가 있으면
        if (groundAttackCenterPos != null) { attackPos = groundAttackCenterPos.position; }
        // 없으면 : 공격자의 약간 앞
        else { attackPos = transform.position + transform.forward * (attackRange / 2); }

        // ----- 콜라이더 탐색 -----
        Collider[] colliders = Physics.OverlapSphere(attackPos, attackRange);

        foreach (Collider collider in colliders)
        {
            GameObject target = collider.gameObject;

            // DamageReaction 컴포넌트가 있으면
            DamageReaction reaction = target.GetComponent<DamageReaction>();
            if (reaction != null && reaction.CompareTag(attackData.targetTag))
            {
                reaction.KnockBack(knockBackData, transform);
                reaction.TakeDamage(attackData);
            }
        }

        // --- 이펙트 발생 ---
        groundedEffectData.Instantiate(thisActor.gameObject);
        //Debug.Log($"{owner.name} : groundedEffect");

        // --- 애니메이션 속도 정상화 ---
        thisActor.actorAnimator.SetAnimationSpeed(1f);

#if UNITY_EDITOR
        // ----- 디버그 -----
        showGizmo = true;
#endif

        // 플레이어 낙하 시 충격음
        // SoundManager.Instance.PlayPlayerDropImpact();
    }


#if UNITY_EDITOR
    private bool showGizmo = false;
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = Color.yellow;

            // 공격 위치 구하기
            Vector3 attackPos;
            // 지정된 위치가 있으면
            if (groundAttackCenterPos != null) { attackPos = groundAttackCenterPos.position; }
            // 없으면 : 공격자의 약간 앞
            else { attackPos = transform.position + transform.forward * (attackRange / 2); }

            Gizmos.DrawWireSphere(attackPos, attackRange);
            Timer.Instance.StartTimer(this, "_기즈모", 0.2f, () => showGizmo = false);
        }
    }
#endif
}

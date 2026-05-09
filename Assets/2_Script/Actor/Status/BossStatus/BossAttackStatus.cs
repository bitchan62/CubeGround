using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BossAttackStatus : MonsterAttackStatus
{
    public BossAttackStatus(Actor owner) : base(owner)
    {
        owner.damageReaction.whenDie.Add(() => { WarningPlaneSetter.DelWarning(owner, ref warning); }, 1);
        thisBoss = owner as Boss;
    }
    private Boss thisBoss;
    private GameObject warning;



    public override void Enter()
    {
        // 카운트다운
        switch (thisBoss.nowAttackKey)
        {
            case AttackName.Monster_BossNormalAttack:
            case AttackName.Monster_BossTripleAttack:
                attackPhase = AttackPhase.Do;
                break;

            case AttackName.Monster_BossChargeAttack:
                attackPhase = AttackPhase.Before;
                thisBoss.actorAnimator.SetAnimationParam("DoChargeAttack");

                // <- 여기서 경고발판 생성
                warning = WarningPlanePool.Instance.GetWarningPlaneFromPool(WarningShape.Circle);
                warning.SetActive(true);
                WarningPlaneCustom.Instance.UpdatePosition(warning, thisBoss.transform.position);
                WarningPlaneCustom.Instance.UpdateSize(warning, 15f, 15f);
                WarningPlaneSetter.UpdateWarningAlpha(thisBoss, warning, 1.5f);
                warning.transform.SetParent(thisBoss.transform);
                break;

            default:
                attackPhase = AttackPhase.Before;
                break;
        }
    }


    public override void Update()
    {
        switch (attackPhase)
        {
            // 공격 전 애니메이션
            case AttackPhase.Before:
                switch (thisBoss.nowAttackKey)
                {
                    case AttackName.Monster_BossChargeAttack:
                        if (thisBoss.actorAnimator.CheckAnimationEnd("Before_Charge"))
                        {
                            WarningPlaneSetter.DelWarning(thisBoss, ref warning);
                            attackPhase = AttackPhase.Do;
                        }
                        break;

                    default:
                        attackPhase = AttackPhase.Do;
                        break;
                }
                break;


            // 실제 공격 발생
            case AttackPhase.Do:
                    switch (thisBoss.nowAttackKey)
                    {
                        // 기본공격
                        case AttackName.Monster_BossNormalAttack:
                            thisBoss.actorAnimator.SetAnimationParam("DoNomalAttack_1");
                            thisBoss.attackAction.Attack();
                            attackPhase = AttackPhase.DoAttackAnimation;
                            break;

                        // 3연타 공격
                        case AttackName.Monster_BossTripleAttack:
                            thisBoss.attackAction.Attack();
                            attackPhase = AttackPhase.DoAttackAnimation;
                            break;

                        // 돌진공격
                        case AttackName.Monster_BossChargeAttack:
                            thisBoss.attackAction.Attack();
                            attackPhase = AttackPhase.DoAttackAnimation;
                            break;

                        default:
                            thisBoss.SwitchStatus(thisBoss.idleStatus);
                            break;
                    }
                break;


            // 공격 중 애니메이션
            case AttackPhase.DoAttackAnimation:
                switch (thisBoss.nowAttackKey)
                {
                    // // 기본공격
                    // case AttackName.Monster_BossNormalAttack:
                    //     if (thisBoss.actorAnimator.CheckAnimationEnd("Nomal_Attack_2"))
                    //     { thisBoss.SwitchStatus(thisBoss.selectStatus); }
                    //     break;

                    // 돌진공격
                    case AttackName.Monster_BossChargeAttack:
                        if (thisBoss.actorAnimator.CheckAnimationEnd("Charge_Attack", true))
                        { thisBoss.SwitchStatus(thisBoss.selectStatus); }
                        break;
                }
                break;


            // 공격 후 애니메이션 (들어올 일 없음)
            case AttackPhase.After:
            default:
                thisBoss.SwitchStatus(thisBoss.idleStatus);
                break;
        }

    }



    public override void Exit()
    {
        thisBoss.actorAnimator.SetAnimationSpeed();
    }


}

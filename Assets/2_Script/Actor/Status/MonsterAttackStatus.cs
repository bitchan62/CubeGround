using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class MonsterAttackStatus : Status
{
    protected enum AttackPhase
    {
        Before,
        Do,
        DoAttackAnimation,
        After
    }

    protected Monster thisMonster;
    public MonsterAttackStatus(Actor owner) : base(owner)
    { thisMonster = owner as Monster; }

    // 공격 단계
    protected AttackPhase attackPhase;

    public override void Enter()
    {
        attackPhase = AttackPhase.Before;
        thisMonster.attackAction.Attack();
    }


    public override void Update()
    {
        switch (attackPhase)
        {
            // 공격 전 애니메이션
            case AttackPhase.Before:
                attackPhase = AttackPhase.Do;
                break;

                // 실제 공격 발생
             case AttackPhase.Do:
                attackPhase = AttackPhase.DoAttackAnimation;
                break;

                // 공격 중 애니메이션
             case AttackPhase.DoAttackAnimation:
                if (thisMonster.actorAnimator.CheckAnimationEnd("Attack"))
                { attackPhase = AttackPhase.After; }
                break;

                // 공격 후 애니메이션
             case AttackPhase.After:
                if (thisMonster.actorAnimator.CheckAnimationEnd("Reload"))
                { AfterAttack(); }
                break;
        }
    }


    protected virtual void AfterAttack()
    {
        // Debug.Log($"{thisBossArm.name} : Attack -> Idle");
        thisMonster.SwitchStatus(thisMonster.idleStatus);
    }


    public override void Exit()
    {
        thisMonster.actorAnimator.SetAnimationSpeed();
        thisMonster.attackAction.Cancel();
    }
}

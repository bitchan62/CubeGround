using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossNextSelectStatus : Status
{
    private Boss thisBoss;

    // 공격 횟수
    public MaxNowInt normalAttackCount = new MaxNowInt(2);
    public MaxNowInt chargeAttackCount = new MaxNowInt(1);
    public MaxNowInt beforeJumpCycleCount = new MaxNowInt(0);

    public BossNextSelectStatus(Actor owner) : base(owner)
    { thisBoss = owner as Boss; }


    public override void Enter()
    { }


    public override void Update()
    { thisBoss.SwitchStatus(SelectNextStatus()); }


    public override void Exit()
    { }


    protected IStatus SelectNextStatus()
    {
        Debug.Log($"돌진:{chargeAttackCount.now} " +
            $"/ 평타:{normalAttackCount.now} " +
            $"/ 사이클:{beforeJumpCycleCount.now}");

        IStatus nextStatus = thisBoss.idleStatus;

        // 돌진공격 실행 가능
        if (0 < chargeAttackCount)
        {
            chargeAttackCount -= 1;
            thisBoss.nowAttackKey = AttackName.Monster_BossChargeAttack;
        }

        // 일반공격 사용가능
        else if (0 < normalAttackCount)
        {
            normalAttackCount -= 1;
            thisBoss.actorAnimator.SetAnimationParam("AttackCount", normalAttackCount.now);

            if (normalAttackCount == 1)
            {
                Debug.Log($"{thisBoss.name} AttackName : {AttackName.Monster_BossNormalAttack}");
                thisBoss.nowAttackKey = AttackName.Monster_BossNormalAttack;
            }
            else if (normalAttackCount == 0)
            {
                Debug.Log($"{thisBoss.name} AttackName : {AttackName.Monster_BossTripleAttack}");
                thisBoss.nowAttackKey = AttackName.Monster_BossTripleAttack;
            }
        }

        // 사이클 1회 돌았음
        else if (0 < beforeJumpCycleCount)
        {
            beforeJumpCycleCount -= 1;
            thisBoss.nowAttackKey = AttackName.Monster_BossChargeAttack;
            chargeAttackCount.Reset();
            normalAttackCount.Reset();
        }

        // 점프공격 사용가능
        else if (beforeJumpCycleCount == 0)
        {
            thisBoss.nowAttackKey = AttackName.Monster_BossChargeAttack;
            chargeAttackCount.Reset();
            normalAttackCount.Reset();
            beforeJumpCycleCount.Reset();
            nextStatus = thisBoss.jumpStatus;
        }

        // 뭔가 이상한 경우
        else
        {
            thisBoss.nowAttackKey = AttackName.Monster_BossChargeAttack;
            chargeAttackCount.Reset();
            normalAttackCount.Reset();
            beforeJumpCycleCount.Reset();
        }

        return nextStatus;
    }


}

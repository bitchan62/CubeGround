using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMoveStatus : Status
{
    protected Monster thisMonster;
    private ChaseAction chaseAction;

    public MonsterMoveStatus(Actor owner) : base(owner)
    {
        thisMonster = owner as Monster;
        chaseAction = thisMonster.moveAction as ChaseAction;
    }

    public override void Enter()
    {
        chaseAction.ReturnToNav();
        Timer.Instance.StartEndlessTimer(thisMonster, "ReturnToNav", 1.0f, () => chaseAction?.ReturnToNav());
    }

    public override void Update()
    {
        // 공격 가능 시 : 변경
        if (thisMonster.isReadyToAttack)
        { thisMonster.SwitchStatus(thisMonster.attackStatus); }

        // 이동
        else if (thisMonster.chaseAction.IsCanChase())
        {
            // 추격
            if (!thisMonster.isInAttackRange || !thisMonster.isClear)
            { thisMonster.chaseAction.isMove = true; }
            else
            { thisMonster.chaseAction.isMove = false; }

            // 바라보기
            if (!thisMonster.isFacing)
            { thisMonster.moveAction.Turn(); }
        }

        else
        { thisMonster.SwitchStatus(thisMonster.idleStatus); }
    }

    public override void Exit()
    {
        thisMonster.chaseAction.isMove = false;
        Timer.Instance.StopEndlessTimer(thisMonster, "ReturnToNav");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIdleStatus : Status
{
    private Monster thisMonster;
    private ChaseAction chaseAction;

    public MonsterIdleStatus(Actor owner) : base(owner)
    {
        thisMonster = owner as Monster;
        chaseAction = thisMonster.moveAction as ChaseAction;
    }

    public override void Enter()
    {
        chaseAction?.ReturnToNav();
    }

    public override void Update()
    {
        if (thisMonster.isReadyToAttack)
        { thisMonster.SwitchStatus(thisMonster.attackStatus); }
        else if (thisMonster.chaseAction.IsCanChase())
        { thisMonster.SwitchStatus(thisMonster.moveStatus); }
        else
        {
            // 바라보기
            if (!thisMonster.isFacing)
            { thisMonster.moveAction.Turn(); }
        }
    }


    public override void Exit()
    {
    }


}

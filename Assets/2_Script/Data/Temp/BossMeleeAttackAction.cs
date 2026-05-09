using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMeleeAttackAction : BasicMeleeAttackAction
{
    private Boss thisBoss;

    protected override void Awake()
    {
        base.Awake();
        thisBoss = thisActor as Boss;
    }

    public void DoBossMeleeAttack()
    { Do(); }

    public void AfterBossMeleeAttack()
    { Cancel(); }

    protected override void Exit()
    {
        base.Exit();
        thisBoss.SwitchStatus(thisBoss.selectStatus);
    }
}

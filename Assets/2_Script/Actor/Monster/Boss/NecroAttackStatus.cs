using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecroAttackStatus : MonsterAttackStatus
{
    protected Necromancer thisNecro;

    public NecroAttackStatus(Actor owner) : base(owner)
    { thisNecro = owner as Necromancer; }

    protected override void AfterAttack()
    {
        thisNecro.SwitchStatus(null);
        thisNecro.NextPattern?.Invoke();
    }

}

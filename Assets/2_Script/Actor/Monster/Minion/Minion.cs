using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : Monster
{
    protected override void Awake()
    {
        base.Awake();
        nowAttackKey = AttackName.Monster_MinionNormalAttack;
    }
}

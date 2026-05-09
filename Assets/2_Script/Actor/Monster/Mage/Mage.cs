using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Monster
{
    protected override void Awake()
    {
        base.Awake();
        nowAttackKey = AttackName.Monster_MageSpellAttack;
    }
}

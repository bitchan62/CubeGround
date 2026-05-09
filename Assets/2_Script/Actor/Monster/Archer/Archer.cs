using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Monster
{
    protected override void Awake()
    {
        base.Awake();
        nowAttackKey = AttackName.Monster_ArcherFireAttack;
    }
}

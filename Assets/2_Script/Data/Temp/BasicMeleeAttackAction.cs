using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeAttackAction : MeleeAttackAction2
{
    public void DoBasicMeleeAttack()
    { Do(); }

    public void AfterBasicMeleeAttack()
    { Cancel(); }

}

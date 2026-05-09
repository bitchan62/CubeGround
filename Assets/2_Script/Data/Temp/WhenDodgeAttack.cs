using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class WhenDodgeAttack : MeleeAttackAction2
{
    public void DoDodge()
    { Do(); }
    

    public void AfterDodge()
    { Cancel(); }

}

using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAction2 : AttackAction2
{
    public ActorWeapon2 weapon;


    protected override void Awake()
    {
        base.Awake();
        if (weapon == null) { Debug.LogError($"{thisActor.name} : weapon이 지정되어있지 않음"); }
        else { weapon.SetWeaponOwner(thisActor); }
    }

    public override void Do()
    {
        base.Do();
        weapon.SetData(attackData);
        weapon.SetData(knockBackData);
        weapon.SetData(hitEffect);
        weapon.isActivate = true;
    }

    public override void Cancel()
    {
        //Debug.Log($"{owner.name} : Attack Cancel");
        weapon.isActivate = false;
    }

    protected override void Exit()
    {
        //Debug.Log($"{owner.name} : Attack Exit");
        base.Exit();
        weapon.isActivate = false;
    }


    private void OnDisable()
    {
        weapon.isActivate = false;
    }

}

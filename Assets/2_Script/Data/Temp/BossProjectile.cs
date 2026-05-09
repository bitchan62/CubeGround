using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : Projectile2
{
    public FinalBossHp finalBossHp;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Monster"))
        { EffectAndDestory(); }
    }

    private void Update()
    {
        if (finalBossHp != null)
        {
            if (finalBossHp.sharedHp <= 0) { EffectAndDestory(); }
        }
    }

}

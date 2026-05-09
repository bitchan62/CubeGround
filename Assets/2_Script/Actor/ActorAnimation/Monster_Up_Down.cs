using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_Up_Down : Up_Down
{
    private DamageReaction damageReaction;

    protected override void Start()
    {
        base.Start();

        damageReaction = GetComponent<DamageReaction>();
        damageReaction?.whenDie.Add(() => this.enabled = false);
    }
}

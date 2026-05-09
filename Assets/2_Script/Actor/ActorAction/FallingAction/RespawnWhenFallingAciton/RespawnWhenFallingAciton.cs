using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RespawnAction))]
public class RespawnWhenFallingAciton : FallingAction
{
    // 떨어졌을 경우의 데미지
    [SerializeField] protected int fallingDamage = 3;

    protected override void Awake()
    {
        base.Awake();
        RespawnAction respawnAction = GetComponent<RespawnAction>();
        DamageReaction damageReaction = GetComponent<DamageReaction>();

        whenFallingEvent.Add(
            () => {
                damageReaction.TakeDamage(fallingDamage, thisActor);
                respawnAction.ReturnToSafePos();
            });
    }

}

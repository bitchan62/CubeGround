using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnWhenSandwitchedReaction : SandwitchedReaction
{
    // 끼었을 경우의 피해량
    [SerializeField] protected int sandwichedDamage = 3;

    private RespawnAction respawnAction;
    private DamageReaction damageReaction;
 
    // // 이동 중에는 끼임 X <- 헛소리같음
    // private MoveAction moveAction; 
    // private DodgeAction dodgeAction;

    protected override void Awake()
    {
        base.Awake();
        respawnAction = GetComponentInParent<RespawnAction>();
        if (respawnAction == null) { Debug.LogError($"{actor.gameObject}에 RespawnAction 없음"); }

        damageReaction = actor.damageReaction;
        //GetComponentInParent<DamageReaction>();
        if (damageReaction == null) { Debug.LogError($"{actor.gameObject}에 DamageReaction 없음"); }

        // moveAction = actor.moveAction;
        // dodgeAction = actor.gameObject.GetComponent<DodgeAction>();
    }


    // private bool IsCanNotSandwitched()
    // {
    //     if (moveAction == null || dodgeAction == null)
    //     {
    //         Debug.Log($"{actor.name} : RespawnWhenSandwitchedReaction : move나 dodge 없음");
    //         return false;
    //     }
    // 
    //     // 이동 중이거나 dodge 중이면 샌드위치 X
    //     return moveAction.isMove || dodgeAction.isDodge;
    // }


    protected override void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Cube")) { return; }
        //if (IsCanNotSandwitched()) { return; }

        damageReaction.TakeDamage(sandwichedDamage, actor); // 데미지
        respawnAction.ReturnToSafePos();                    // 텔포
    }


}

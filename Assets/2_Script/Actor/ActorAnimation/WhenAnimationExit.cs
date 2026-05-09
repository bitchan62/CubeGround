using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenAnimationExit : StateMachineBehaviour
{
    private AttackName attackName;
    private Actor owner;

    private void Callback()
    {
        attackName = owner.nowAttackKey;
        owner.actorAnimator.InvokeCallback(attackName);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (owner != null) { Callback(); }
        else
        {
            owner = animator.gameObject.GetComponent<Actor>();
            if (owner != null) { Callback(); }
            else { Debug.Log("Actor 대상이 아님"); }
        }

    }
}
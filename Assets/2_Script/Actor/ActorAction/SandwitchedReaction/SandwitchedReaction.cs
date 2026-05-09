using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// <- 얘는 ActorAction이 아님....
// 왜냐하면 Actor과 별개의 오브젝트에 붙여야 하기 때문임....
public class SandwitchedReaction : MonoBehaviour
{
    protected Actor actor;

    protected virtual void Awake()
    {
        actor = GetComponentInParent<Actor>();

        if (actor == null)
        { Debug.Log("actor이 없는 놈한테 SandwitchedReaction을 붙였음"); return; }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Cube")) { return; }
        actor?.damageReaction.TrueTakeDamage(int.MaxValue);
    }
}
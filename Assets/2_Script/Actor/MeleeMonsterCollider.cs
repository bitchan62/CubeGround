using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeMonsterCollider : MonoBehaviour
{
    private void Awake()
    {
        Actor thisActor = transform.root.GetComponent<Actor>();

        Collider actorCollider = thisActor?.GetComponent<Collider>();
        Collider thisCollider = GetComponent<Collider>();

        if (thisCollider != null )
        {
            System.Action temp = 
                () => {
                    actorCollider.isTrigger = false;
                    thisCollider.enabled = false;
                    thisCollider.isTrigger = true;
                };

            thisActor?.damageReaction?.whenDie.Add(temp, 1);
        }
    }

}

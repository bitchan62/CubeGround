using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActorAction : MonoBehaviour
{
    private Actor actor;
    protected Actor thisActor
    {
        get
        {
            if (actor == null)
            { actor = GetComponent<Actor>(); }
            return actor;
        }
    }


    protected virtual void Awake()
    {
        // <- 나중에 뭐 생기면 
    }

}

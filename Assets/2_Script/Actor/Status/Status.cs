using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Status : IStatus
{
    protected Actor owner;

    public Status(Actor actor)
    { this.owner = actor; }

    public abstract void Enter();

    public abstract void Update();

    public abstract void Exit();
}

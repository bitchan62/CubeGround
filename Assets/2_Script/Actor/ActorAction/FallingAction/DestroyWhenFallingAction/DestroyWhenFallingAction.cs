using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DestroyWhenFallingAction : FallingAction
{
    private void OnEnable()
    {
        whenFallingEvent.Add(() => { thisActor.damageReaction.TrueTakeDamage(int.MaxValue); }, 1);
    }
}

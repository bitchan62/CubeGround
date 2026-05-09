using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BossIdleStatus : MonsterIdleStatus
{
    public BossIdleStatus(Actor owner) : base(owner)
    { thisBoss = owner as Boss; }
    Boss thisBoss;


}

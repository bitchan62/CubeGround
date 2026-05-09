using UnityEngine;

public class BossStunStatus : TriggerAnimationPlayStatus
{
    public BossStunStatus(Actor owner, Status nextStatus)
        : base(owner, nextStatus, "Hit", "DoHit")
    { thisBoss = owner as Boss; }
    private Boss thisBoss;


    public override void Enter()
    {
        thisBoss.actorAnimator.SetAnimationSpeed(thisBoss.stunAnimationSpeed);
        base.Enter();
    }

    public override void Exit()
    {
        thisBoss.actorAnimator.SetAnimationSpeed();
        base.Exit();
    }
}

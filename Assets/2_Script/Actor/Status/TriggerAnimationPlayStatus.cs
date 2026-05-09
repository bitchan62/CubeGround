

public class TriggerAnimationPlayStatus : Status
{
    protected Monster thisMonster;
    private string animationName;
    private Status nextStatus;
    private string animationParam;

    public TriggerAnimationPlayStatus(Actor owner, Status nextStatus, string animationName, string animationParam = "") : base(owner)
    {
        thisMonster = owner as Monster;
        this.animationName = animationName;
        this.nextStatus = nextStatus;
        this.animationParam = animationParam;
    }

    public override void Enter()
    {
        if (animationParam != "")
        { thisMonster.actorAnimator.SetAnimationParam(animationParam); }
    }

    public override void Update()
    {
        if (thisMonster.actorAnimator.CheckAnimationEnd(animationName))
        { thisMonster.SwitchStatus(nextStatus); }
    }

    public override void Exit() { }
}

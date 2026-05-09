using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMeleeAttackAction3 : BasicMeleeAttackAction
{
    private Boss thisBoss;

    protected override void Awake()
    {
        base.Awake();
        thisBoss = thisActor as Boss;
    }

    public void DoTripleAttack()
    { Do(); }

    public void AfterTripleAttack()
    { Cancel(); }


    [Header("트리플 어택 후 대기 설정")]
    [Tooltip("첫 번째 딜레이 애니메이션 속도")]
    public float firstWaitRate = 0.1f;
    [Tooltip("첫 번째 딜레이 애니메이션 속도 -> 두 번째 딜레이 애니메이션 속도 사이 간격")]
    public float waitDuration = 1f;
    [Tooltip("두 번째 딜레이 애니메이션 속도")]
    public float SecondWaitRate = 0.5f;

    public void WaitAfterTripleAttack()
    {
        thisActor.actorAnimator.SetAnimationSpeed(attackData.speed * firstWaitRate);
        Timer.Instance.StartTimer(this, "WaitAfterTripleAttack", waitDuration,
            () => { thisActor.actorAnimator.SetAnimationSpeed(attackData.speed * SecondWaitRate); });
    }

    protected override void Exit()
    {
        base.Exit();
        thisBoss.SwitchStatus(thisBoss.selectStatus);
    }
}

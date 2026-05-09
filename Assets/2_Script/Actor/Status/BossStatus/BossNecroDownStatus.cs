using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossNecroDownStatus : IStatus
{
    private Necromancer owner;
    private Vector3 goalPos;
    private float downSpeed;

    public BossNecroDownStatus(Necromancer actor, Transform goalPos, float downSpeed)
    {
        this.owner = actor;
        if (goalPos != null) { this.goalPos = goalPos.position; }
        else                 { this.goalPos = Vector3.zero; }
            this.downSpeed = downSpeed;
    }

    public void Enter()
    {
        owner.rigid.isKinematic = true;
        owner.actorAnimator.SetAnimationParam("IsMove", true);
    }

    public void Update()
    {
        if((goalPos - owner.transform.position).sqrMagnitude <= 0.1f)
        {
            owner.SwitchStatus(null);
        }
        else
        {
            owner.transform.position = Vector3.MoveTowards(
                owner.transform.position,
                goalPos,
                downSpeed * Time.deltaTime);
        }
    }

    public void Exit()
    {
        owner.rigid.isKinematic = false;
        owner.rigid.useGravity = true;
        owner.damageReaction.isInvincible = false;

        // 카메라 대상 바꾸기
        FollowCamera cam = Camera.main.GetComponent<FollowCamera>();

        if (cam != null)
        {
            cam.FocusChange(owner.transform);
            cam.SpeedChage(30f);
            cam.OffsetRateChage(0.5f);
        }

        Timer.Instance.StartTimer(owner, 3f, () =>
        {
            cam.FocusChange(owner.Target);
            cam.PosReset(0.7f);
            cam.SpeedChage(60f);
            cam.OffsetRateChage(1f);
        });
    }

}

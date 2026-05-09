using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEngine.UI.GridLayoutGroup;

public class BossArmAvoidAndAttackStatus : IStatus
{
    private Actor owner;
    private BossArm thisBossArm;
    private Vector3 avoidPos = new Vector3(0, 50, 0);
    private float avoidSpeed = 20f;

    public BossArmAvoidAndAttackStatus(Actor owner, Transform avoidPos)
    {
        this.owner = owner;
        if (owner is BossArm b) { thisBossArm = b; }
        if (avoidPos != null) { this.avoidPos = avoidPos.position; }
    }


    public void Enter()
    {
        owner.rigid.velocity = Vector3.zero;
        owner.rigid.isKinematic = true;
    }


    public void Update()
    {
        if ((avoidPos - owner.transform.position).sqrMagnitude <= 0.1f)
        {
            thisBossArm.SwitchStatus(thisBossArm.jumpStatus);
        }
        else
        {
            Turn();
            owner.transform.position = Vector3.MoveTowards(
                                owner.transform.position,      // 시작 위치
                                avoidPos,                      // 목표 위치
                                avoidSpeed * Time.deltaTime);  // 최대 이동 거리 (속도 * 시간)
        }
    }


    public void Exit()
    {

    }


    private void Turn()
    {
        float rotationSpeed = 3f;

        Vector3 direction = thisBossArm.Target.position - owner.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation *= Quaternion.Euler(0f, -90f, 90f);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEngine.GraphicsBuffer;

public class BossArmAvoidStatus : MonsterMoveStatus
{
    private Vector3 avoidPos = new Vector3(0, 50, 0);
    private float avoidSpeed = 20f;

    public BossArmAvoidStatus(Actor owner, Transform avoidPos) : base(owner)
    {
        if (avoidPos != null)
        { this.avoidPos = avoidPos.position; }
    }

    public override void Enter()
    {
        owner.rigid.velocity = Vector3.zero;
        owner.rigid.isKinematic = true;
    }

    public override void Update()
    {
        if ((avoidPos - owner.transform.position).sqrMagnitude <= 0.1f)
        {
            Turn();
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

    public override void Exit()
    { }


    private void Turn()
    {
        float rotationSpeed = 3f;

        Vector3 direction = thisMonster.Target.position - owner.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation *= Quaternion.Euler(0f, -90f, 90f);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

}

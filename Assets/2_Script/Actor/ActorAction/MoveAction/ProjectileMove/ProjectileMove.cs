using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ProjectileMove : MoveAction
{
    // 초기화
    protected override void Awake()
    {
        base.Awake();
        // 각종 물리 제거
        rigid.useGravity = false;
        rigid.freezeRotation = true;
    }

    // 목표 위치를 입력받는 메서드
    public void SetTarget(Vector3 targetPos)
    {
        // 방향 벡터 계산 (정규화)
        moveVec = (targetPos - transform.position).normalized;
        isMove = true;

        base.Turn(); // <- 딱 1회, 해당 방향 바라봄
    }


    // Target 자체를 입력받음
    public virtual void SetTarget(Transform target)
    {
        isMove = true;
    }
}

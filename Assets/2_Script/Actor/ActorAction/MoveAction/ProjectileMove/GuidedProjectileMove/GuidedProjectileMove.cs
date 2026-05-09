using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class GuidedProjectileMove : ProjectileMove
{
    // 회전 속도
    [SerializeField] protected float turnSpeed = 3f;

    // 가속 주기
    [SerializeField] protected float accelRate = 0.1f;

    // 가속력
    [SerializeField] protected float accelSpeed = 0.1f;

    protected Transform target;

    // 가속
    private void Start()
    { Timer.Instance.StartEndlessTimer(this, "_ProjectileMove", accelRate, () => moveSpeed += accelSpeed); }

    // 목표 대상을 입력받는 메서드
    // 끝까지 추격
    public override void SetTarget(Transform p_target)
    {
        isMove = true;
        target = p_target;
    }

    public override void Move()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, turnSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);
        moveVec = transform.forward;

        base.Move();
    }
}

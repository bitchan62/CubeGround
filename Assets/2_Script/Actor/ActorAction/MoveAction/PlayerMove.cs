using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MoveAction
{
    // 레이어 마스크
    private LayerMask canNotMoveLayer;


    protected override void Awake()
    {
        base.Awake();
        // 전방 주시 거리
        frontRayDistance = transform.localScale.z * 0.7f;

        dodgeAction = GetComponent<DodgeAction>();
        canNotMoveLayer = 1 << LayerMask.NameToLayer("Cube");
        canNotMoveLayer += 1 << LayerMask.NameToLayer("CanNotThrough");
    }


    // 닷지 중 이동 불가
    DodgeAction dodgeAction; // <- 이거를 dodge 쪽에 넣어야 되나 여기다 넣어야 되나


    // 전방 레이캐스트
    private RaycastHit frontRayHit;

    // 앞쪽 거리 판단
    protected float frontRayDistance;
    
    // 전방 확인 - 이동 가능하면 true 반환
    public virtual bool IsCanMove()
    {
        // 레이어마스크 : 큐브
        if (Physics.Raycast(transform.position, moveVec, out frontRayHit, frontRayDistance, canNotMoveLayer))
        {
            // 트리거 콜라이더는 무시 (통과 가능)
            if (frontRayHit.collider.isTrigger) { return true; }
            // 일반 콜라이더는 통과 불가
            else { return false; }
        }

        return true; // 아무것도 감지되지 않으면 이동 가능
    }


    // 이동
    public override void Move()
    {
        // dodge 중이라면
        if (dodgeAction.isDodge)
        {
            isMove = false;
            return;
        }

        // 이동
        if (IsCanMove())
        { base.Move(); }
    }
}
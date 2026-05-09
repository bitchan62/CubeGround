using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArmJumpStatus : Status
{
    public BossArmJumpStatus(Actor owner,
        float jumpHeight,
        float jumpSpeed,
        float fallSpeed,
        float delay,
        float chaseSpeed) : base(owner)
    {
        owner.damageReaction.whenDie.Add(() => { WarningPlaneSetter.DelWarning(owner, ref warning); }, 1);
        thisBossArm = owner as BossArm;
        this.jumpHeight = jumpHeight;
        this.jumpSpeed = jumpSpeed;
        this.fallSpeed = fallSpeed;
        this.delay = delay;
        this.chaseSpeed = chaseSpeed;
        this.ownerCollider = owner.GetComponent<Collider>();
    }

    // 점프 정보
    private float jumpHeight;
    private float jumpSpeed;
    private float fallSpeed;
    private float chaseSpeed;

    // 점프 사이 딜레이
    private float delay;

    // 현재 높이
    private float nowHeight;

    // <- 점프 중 경고발판
    private GameObject warning;
    BossArm thisBossArm;
    Collider ownerCollider;

    // 점프 절차
    private JumpPhase jumpPhase;
    protected enum JumpPhase
    {
        Before,
        Do,
        Fall,
        After, // 임시
        Next
    }

    public override void Enter()
    {
        jumpPhase = JumpPhase.Before;
        owner.rigid.useGravity = false;
        owner.rigid.isKinematic = false;
        nowHeight = thisBossArm.Target.position.y;
    }

    public override void Update()
    {
        switch (jumpPhase)
        {
            case JumpPhase.Before:
                thisBossArm.jumpEffectData.Instantiate(thisBossArm.gameObject); // 점프 사운드
                jumpPhase = JumpPhase.Do;
                break;

            case JumpPhase.Do:
                // 일정 고도 이하라면
                // 상승
                if (owner.transform.position.y <= jumpHeight + nowHeight)
                {
                    owner.rigid.velocity = Vector3.up * jumpSpeed;

                    Vector3 targetPos = thisBossArm.Target.position;
                    targetPos.y = thisBossArm.transform.position.y;

                    // 목표 방향 계산
                    Vector3 direction = targetPos - thisBossArm.transform.position;

                    // LookRotation으로 목표 회전 생성
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // Slerp로 부드럽게 회전
                    float rotationSpeed = 3f;
                    thisBossArm.transform.rotation = Quaternion.Slerp(thisBossArm.transform.rotation,
                        targetRotation, Time.deltaTime * rotationSpeed);
                }


                // <- 고도 충족 + 플레이어 머리 위 X이면
                // 대상 머리 위로 이동
                else if (!IsThisOnTargetHead())
                {
                    owner.rigid.velocity = Vector3.zero;

                    //  Vector3 targetPos = thisBossArm.Target.position;
                    //  targetPos.y = thisBossArm.transform.position.y;
                    //  
                    //  thisBossArm.transform.LookAt(targetPos);
                    Vector3 targetPos = thisBossArm.Target.position;
                    targetPos.y = thisBossArm.transform.position.y;

                    // 목표 방향 계산
                    Vector3 direction = (targetPos - thisBossArm.transform.position).normalized;

                    // LookRotation으로 목표 회전 생성
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // Slerp로 부드럽게 회전
                    float rotationSpeed = 5f;
                    thisBossArm.transform.rotation = Quaternion.Slerp(thisBossArm.transform.rotation,
                       targetRotation, Time.deltaTime * rotationSpeed);

                    owner.transform.position
                        = Vector3.MoveTowards(
                            owner.transform.position, // 시작 위치
                            targetPos,                // 목표 위치
                            chaseSpeed * Time.deltaTime);   // 최대 이동 거리 (속도 * 시간)
                }

                // 고도 충족 시
                // 하강 시작
                else
                {
                    //MovePosToTargetHead();
                    RaycastHit hit;
                    if (IsCanGround(out hit))
                    {
                        //Debug.Log($"{owner.name} : Start Fall");
                        jumpPhase = JumpPhase.Fall;

                        // 낙하 공격 실행
                        owner.nowAttackKey = AttackName.Monster_BossDropAttack;
                        owner.attackAction.Attack();
                        owner.actorAnimator.SetAnimationParam("DoDropAttack");
                        // 착지 시 이벤트
                        owner.foot.whenGroundEvent.Add(ReJump, 1);

                        // 착지 시 카메라 쉐이킹
                        owner.foot.whenGroundEvent.Add(CameraShaking, 1);

                        if (WarningPlanePool.Instance != null && WarningPlaneCustom.Instance != null)
                        {
                            // 경고 발판 생성
                            warning = WarningPlanePool.Instance.GetWarningPlaneFromPool(WarningShape.Circle);
                            warning.SetActive(true);
                            WarningPlaneCustom.Instance.UpdatePosition(warning, hit.point);
                            WarningPlaneCustom.Instance.UpdateSize(warning,
                                owner.attackAction.attackRange * 2,
                                owner.attackAction.attackRange * 2);
                            WarningPlaneSetter.UpdateWarningAlpha(owner, warning, 1.5f);
                        }

                        owner.rigid.useGravity = true;
                    }
                }
                break;

            case JumpPhase.Fall:
                owner.rigid.velocity = Vector3.down * fallSpeed;
                break;

            case JumpPhase.After:
                break;
        }
    }

    public override void Exit()
    {
        owner.rigid.useGravity = true;
        WarningPlaneSetter.DelWarning(owner, ref warning);
    }

    public bool IsCanGround(out RaycastHit rayHit)
    {
        Vector3 origin;  // 캐스트 시작 위치
        if (ownerCollider != null) { origin = ownerCollider.bounds.center; }
        else { origin = owner.transform.position; }

        Vector3 direction = Vector3.down;
        float maxDistance = 100f;

        int cubeLayerMask = LayerMask.GetMask("Cube");
        bool isRayHit = Physics.Raycast(origin, direction, out rayHit, maxDistance, cubeLayerMask);
#if UNITY_EDITOR
        Debug.DrawRay(origin, direction * maxDistance, Color.blue, 1f);
#endif

        // SphereCast는 CanNotThrough 레이어만 검사
        RaycastHit sphereHit;
        int sphereLayerMask = LayerMask.GetMask("CanNotThrough");
        float sphereRadius = 1f;  // 필요에 맞게 반경 조절
        bool isSphereHit = Physics.SphereCast(origin, sphereRadius, direction, out sphereHit, maxDistance, sphereLayerMask);

        if (isSphereHit)
        {
            BossArm otherArm = sphereHit.collider.GetComponent<BossArm>();
            if (otherArm != null)
            {
                if (otherArm.nowStatus != otherArm.avoidJumpStatus)
                { otherArm.SwitchStatus(otherArm.avoidJumpStatus); }
                return false;
            }
        }

        return isRayHit;
    }



    // 대상의 머리 위로 transform 이동을 하다가
    // 대상의 머리 위로 이동하면(IsThisOnTargetHead == true) IsCanGround를 검사
    // true면 fall로 이동하고 시작
    private bool IsThisOnTargetHead()
    {
        Vector3 targetPos = thisBossArm.Target.position;
        Vector3 thisPos = thisBossArm.transform.position;

        // 0으로 비교
        targetPos.y = 0;
        thisPos.y = 0;

        // 두 위치벡터의 차이
        Vector3 diff = thisPos - targetPos;

        // 거리가 일정 이하인지 체크
        return diff.sqrMagnitude <= 0.5f * 0.5f;
    }


    private void ReJump()
    {
        jumpPhase = JumpPhase.After;
        owner.rigid.velocity = Vector3.zero;

        WarningPlaneSetter.DelWarning(owner, ref warning);
        Timer.Instance.StartTimer(owner, delay, 
            () => {
                //thisBossArm.SwitchStatus(null);
                thisBossArm.NextPattern?.Invoke();
                Debug.Log($"{thisBossArm.name} : NextPattern Invoke()");
            });
    }

    private void CameraShaking()
    {
        FollowCamera fCamera = Camera.main.GetComponent<FollowCamera>();
        fCamera?.ShakeCamera();
    }

}

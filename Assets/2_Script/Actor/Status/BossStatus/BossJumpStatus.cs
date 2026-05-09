using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossJumpStatus : Status
{
    public BossJumpStatus(Actor owner,
        float jumpHeight,
        float jumpSpeed,
        float fallSpeed,
        float chaseSpeed) : base(owner)
    {
        owner.damageReaction.whenDie.Add(() => { WarningPlaneSetter.DelWarning(owner, ref warning); }, 1);
        thisBoss = owner as Boss;
        this.jumpHeight = jumpHeight;
        this.jumpSpeed = jumpSpeed;
        this.fallSpeed = fallSpeed;
        this.chaseSpeed = chaseSpeed;
    }

    // 상태 소유자 
    private Boss thisBoss;

    // 점프 정보
    private float jumpHeight;
    private float jumpSpeed;
    private float fallSpeed;
    private float chaseSpeed;

    // 현재 높이
    private float nowHeight;

    // <- 점프 중 경고발판
    private GameObject warning;


    // 점프 절차
    private JumpPhase jumpPhase;
    protected enum JumpPhase
    {
        Before,
        Do,
        Fall,
        After
    }

    public override void Enter()
    {
        jumpPhase = JumpPhase.Before;
        thisBoss.actorAnimator.SetAnimationParam("DoJump");
        thisBoss.ActionTimer("점프 전", 1f,
            () => {
                thisBoss.jumpEffectData.Instantiate(thisBoss.gameObject); // 점프 사운드
                jumpPhase = JumpPhase.Do;
            });
    }

    public override void Update()
    {
        switch (jumpPhase)
        {
            case JumpPhase.Before:
                nowHeight = thisBoss.transform.position.y;
                break;

            case JumpPhase.Do:
                // 일정 고도 이하라면
                // 상승
                if (thisBoss.transform.position.y <= jumpHeight + nowHeight)
                {
                    thisBoss.rigid.useGravity = false;
                    thisBoss.rigid.velocity = Vector3.up * jumpSpeed;
                }

                // <- 고도 충족 + 플레이어 머리 위 X이면
                // 대상 머리 위로 이동
                else if (!IsThisOnTargetHead())
                {
                    owner.rigid.velocity = Vector3.zero;

                    Vector3 targetPos = thisBoss.Target.position;
                    targetPos.y = thisBoss.transform.position.y;

                    thisBoss.transform.LookAt(targetPos);

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
                    RaycastHit hit;
                    if (IsCanGround(out hit))
                    {
                        // 다음 Phase로
                        jumpPhase = JumpPhase.Fall;
                        thisBoss.rigid.useGravity = true;
                        thisBoss.rigid.velocity = Vector3.zero;

                        // 낙하 공격 실행
                        thisBoss.nowAttackKey = AttackName.Monster_BossDropAttack;
                        thisBoss.attackAction.Attack();
                        thisBoss.actorAnimator.SetAnimationParam("DoDropAttack");
                        thisBoss.foot.whenGroundEvent.Add(() => {
                            jumpPhase = JumpPhase.After;
                            WarningPlaneSetter.DelWarning(thisBoss, ref warning);
                        }, 1);

                        // 착지 시 카메라 쉐이킹
                        owner.foot.whenGroundEvent.Add(CameraShaking, 1);

                        // 경고 발판 생성
                        if (WarningPlanePool.Instance != null && WarningPlaneCustom.Instance != null)
                        {
                            warning = WarningPlanePool.Instance.GetWarningPlaneFromPool(WarningShape.Circle);
                            warning.SetActive(true);
                            WarningPlaneCustom.Instance.UpdatePosition(warning, hit.point);
                            WarningPlaneCustom.Instance.UpdateSize(warning,
                                thisBoss.attackAction.attackRange * 2,
                                thisBoss.attackAction.attackRange * 2);
                            WarningPlaneSetter.UpdateWarningAlpha(thisBoss, warning, 1.5f);
                        }
                    }
                    else
                    {
                        Debug.Log($"{thisBoss.name} : BossJumpStatus : JumpPhase.Do : 착지 불가능한 위치로 이동됨");
                        // <- 가장 가까운 Cube 가져오기
                        // 그 위로 텔포
                    }
                }
                break;
            
            case JumpPhase.Fall:
                thisBoss.rigid.velocity = Vector3.down * fallSpeed;
                break;

            case JumpPhase.After:
                if (thisBoss.actorAnimator.CheckAnimationEnd("Drop_Attack"))
                { thisBoss.SwitchStatus(thisBoss.selectStatus); }
                break;
        }
    }


    // 대상의 머리 위로 transform 이동을 하다가
    // 대상의 머리 위로 이동하면(IsThisOnTargetHead == true) IsCanGround를 검사
    // true면 fall로 이동하고 시작
    private bool IsThisOnTargetHead()
    {
        Vector3 targetPos = thisBoss.Target.position;
        Vector3 thisPos = thisBoss.transform.position;

        // 0으로 비교
        targetPos.y = 0;
        thisPos.y = 0;

        // 두 위치벡터의 차이
        Vector3 diff = thisPos - targetPos;

        // 거리가 일정 이하인지 체크
        return diff.sqrMagnitude <= 0.5f * 0.5f;
    }


    public bool IsCanGround(out RaycastHit hit)
    {
        Vector3 origin = owner.transform.position;  // 레이캐스트가 시작되는 위치 (오브젝트의 위치)
        Vector3 direction = Vector3.down;     // 하단 방향 (y 음수 방향)
        float maxDistance = 100f;            // 레이캐스트 최대 거리
        int checkLayer = 1 << LayerMask.NameToLayer("Cube"); // Cube 레이어만 검사

        // 레이가 맞았는가 안 맞았는가
        bool isRayHit = Physics.Raycast(origin, direction, out hit, maxDistance, checkLayer);
        return isRayHit;
    }


    public override void Exit()
    { WarningPlaneSetter.DelWarning(thisBoss, ref warning); }

    private void CameraShaking()
    {
        FollowCamera fCamera = Camera.main.GetComponent<FollowCamera>();
        fCamera?.ShakeCamera();
    }
}

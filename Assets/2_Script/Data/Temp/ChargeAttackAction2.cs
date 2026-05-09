using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAttackAction2 : MeleeAttackAction2
{
    public ChargeData chargeData = new ChargeData();

    // 최초 위치
    protected Vector3 originPos = Vector3.zero;

    protected GameObject warningPlane = null;

    protected void OnDisable()
    {
        thisActor.rigid.isKinematic = false;
        thisActor.actorAnimator.SetAnimationSpeed();
    }


    protected override void Awake()
    {
        base.Awake();
        this.enabled = false;
        checkLayer = 1 << LayerMask.NameToLayer("Cube");
        //owner.damageReaction.whenHit.Add(Cancel);
        //owner.damageReaction.whenDie.Add(Cancel);
    }

    public override void Attack()
    {
        base.Attack();

        warningPlane = WarningPlaneSetter.SetWarning(this,
            2f,
            chargeData.distance,
            0.1f,
            transform.position,
            transform.forward);
    }

    public override void Do()
    {
        base.Do();
        originPos = this.transform.position;
        this.enabled = true;
        thisActor.actorAnimator.SetAnimationSpeed(0f);
        WarningPlaneSetter.DelWarning(this, ref warningPlane);
    }

    public override void Cancel()
    {
        base.Cancel();
        this.enabled = false;
        thisActor.rigid.velocity = Vector3.zero;
        thisActor.actorAnimator.SetAnimationSpeed(1f);
        WarningPlaneSetter.DelWarning(this, ref warningPlane);
    }

    protected override void Exit()
    {
        base.Exit();
        Cancel();
    }

    private LayerMask checkLayer;     // 체크할 레이어
    private float checkRadius = 0.1f; // 체크 범위
    private Collider[] cubes = new Collider[3]; // 큐브 콜라이더 캐시

    private void FixedUpdate()
    {
        // --- 착지 중: 물리 무시 ---
        if (thisActor.isRand) { thisActor.rigid.isKinematic = true; }
        else { thisActor.rigid.isKinematic = false; }

        // --- 현재 위치와 최초 위치간 거리 계산 ---
        float traveledDistance = Vector3.Distance(thisActor.rigid.position, originPos);

        // 도착 시 종료
        if (traveledDistance >= chargeData.distance)
        { Cancel(); return; }

        // ---┐
        // <- AI 제작 구간 (속도 조절)
        // --- 기본 속도: 마지막 20% 전까지는 고정, 이후 감속 ---
        float progress = traveledDistance / chargeData.distance;
        float curSpeed = chargeData.speed;

        if (progress >= 0.8f) // 마지막 20% 구간
        {
            // 0.8~1.0 구간에서 점진적 감소 (곡선은 적절히 조절)
            float slowProgress = (progress - 0.8f) / 0.2f; // 0~1로 정규화
                                                           // 부드럽게 0.3배까지 감속 (0.3은 남길 최소 속도)
            float minSpeedRate = 0.3f;
            float speedRate = Mathf.Lerp(1f, minSpeedRate, slowProgress);
            curSpeed = chargeData.speed * speedRate;
        }
        // ---┘

        // --- 다음 위치 & 방향 계산 ---
        Vector3 nextPos = thisActor.rigid.position + transform.forward * curSpeed * Time.fixedDeltaTime;
        Vector3 nextDir = (nextPos - transform.position).normalized;

        // --- 다음 위치 장애물 확인 (위쪽) ---
        int count = Physics.OverlapSphereNonAlloc(nextPos + Vector3.up * checkRadius * 2, checkRadius, cubes, checkLayer);
        if (count > 0)
        {
            Cancel();
            // <- 큐브와 충돌 시 이벤트
            return;
        }

        // --- 다음 위치 낭떠러지 여부 확인 (아래쪽) ---
        count = Physics.OverlapSphereNonAlloc(nextPos - new Vector3(0, checkRadius, 0) + nextDir * 1f,
            checkRadius, cubes, checkLayer);
        if (count == 0)
        { Cancel(); return; }

        // --- 이동 ---
        thisActor.rigid.MovePosition(nextPos);
    }


    // 돌진 중 튕겨내기
    private void OnCollisionStay(Collision collision)
    {
        if (this.enabled && collision.gameObject.CompareTag("Monster"))
        {
            Rigidbody otherRigid = collision.gameObject.GetComponent<Rigidbody>();
            if (otherRigid != null)
            {
                // 좌/우 방향벡터
                Vector3 rightDir = transform.right;
                Vector3 leftDir = -transform.right;

                // 충돌 위치와 자신의 위치 벡터
                Vector3 fromSelfToCollision = collision.transform.position - transform.position;

                // fromSelfToCollision이 오른쪽 방향인지 왼쪽 방향인지 판단
                float dot = Vector3.Dot(fromSelfToCollision, rightDir);

                // 임펄스 크기 설정
                float pushPower = 4f;

                // 충돌 대상이 오른쪽에 있으므로 오른쪽 임펄스
                if (dot > 0f)
                { otherRigid.AddForce(rightDir * pushPower, ForceMode.Impulse); }
                // 왼쪽에 있으므로 왼쪽 임펄스
                else
                { otherRigid.AddForce(leftDir * pushPower, ForceMode.Impulse); }
            }
        }
    }


}

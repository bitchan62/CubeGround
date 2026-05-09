using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ChargeAttack : BasicWeaponAttack
{
    [SerializeField] protected float chargeSpeed = 20f;    // 속도
    [SerializeField] protected float chargeDistance = 20f; // 총 거리
    [SerializeField] protected float warningWidth = 1.5f;  // 경고 발판 

    // 경고 발판
    protected GameObject warningPlane = null;

    // 최초 위치
    protected Vector3 originPos = Vector3.zero;


    protected override void Awake()
    {
        base.Awake();
        checkLayer = 1 << LayerMask.NameToLayer("Cube"); // cube를 만나면 돌진 정지
        this.enabled = false;

        // 사망 시 돌진발판 즉시 제거
        thisActor.damageReaction.whenDie.Add(CancelAttack, 1);
    }


    protected override void BeforeAttack()
    {
        // --- 경고 발판 생성 ---
        warningPlane = WarningPlaneSetter.SetWarning(this,
            warningWidth,
            chargeDistance,
            weaponBeforeDelay,
            transform.position,
            transform.forward);

        // --- 지면 상태인 경우 : 키네마틱 활성화
        if (thisActor.isRand)
        { thisActor.rigid.isKinematic = true; }
    }

    protected override void CancelAttack()
    {
        base.CancelAttack();
        WarningPlaneSetter.DelWarning(this, ref warningPlane);
    }

    protected override void DoAttack()
    {
        base.DoAttack();

        // --- 원래 위치 확인 ---
        originPos = transform.position;

        // --- 발판 반환 ---
        WarningPlaneSetter.DelWarning(this, ref warningPlane);

        // -- 사망 시 리턴 ---
        if (thisActor.damageReaction.isDie) { return; }

        // --- 물리 조정 && 돌진 활성화 ---
        StartCharge();
    }


    protected virtual void StartCharge()
    {
        this.enabled = true;
        UseWeapon();
    }

    protected virtual void EndCharge()
    {
        this.enabled = false;
        thisActor.rigid.isKinematic = false;
        thisActor.rigid.velocity = Vector3.zero;

        NotUseWeapon(); // <- 돌진 종료 시 무기 비활성화
    }

    
    protected virtual void EndChargeWhenCube()
    { EndCharge(); }



    private LayerMask checkLayer;     // 체크할 레이어
    [Tooltip("큐브/낙사 체크 시 체크 범위")] public float checkRadius = 1f; // 체크 범위
    private Collider[] cubes = new Collider[3]; // 큐브 콜라이더 캐시

    private void FixedUpdate()
    {
        // --- 착지 중: 물리 무시 ---
        if (thisActor.isRand) { thisActor.rigid.isKinematic = true; }
        else { thisActor.rigid.isKinematic = false; }

        // --- 현재 위치와 최초 위치간 거리 계산 ---
        float traveledDistance = Vector3.Distance(thisActor.rigid.position, originPos);
        if (traveledDistance >= chargeDistance)
        {
            EndCharge();
            return;
        }

        // ---┐
        // <- AI 제작 구간 (속도 조절)
        // --- 기본 속도: 마지막 20% 전까지는 고정, 이후 감속 ---
        float progress = traveledDistance / chargeDistance;
        float curSpeed = chargeSpeed;

        if (progress >= 0.8f) // 마지막 20% 구간
        {
            // 0.8~1.0 구간에서 점진적 감소 (곡선은 적절히 조절)
            float slowProgress = (progress - 0.8f) / 0.2f; // 0~1로 정규화
                                                           // 부드럽게 0.3배까지 감속 (0.3은 남길 최소 속도)
            float minSpeedRate = 0.3f;
            float speedRate = Mathf.Lerp(1f, minSpeedRate, slowProgress);
            curSpeed = chargeSpeed * speedRate;
        }
        // ---┘

        // --- 다음 위치 계산 ---
        Vector3 nextPos = thisActor.rigid.position + transform.forward * curSpeed * Time.fixedDeltaTime;
        Vector3 nextDir = (nextPos - transform.position).normalized;

        // --- 다음 위치 장애물 확인 (위쪽) ---
        int count = Physics.OverlapSphereNonAlloc(nextPos + Vector3.up * checkRadius * 2,
            checkRadius, cubes, checkLayer);
        if (count > 0)
        { EndChargeWhenCube(); return; }

        // --- 다음 위치 낭떠러지 여부 확인 (아래쪽) ---
        count = Physics.OverlapSphereNonAlloc(nextPos - new Vector3(0, checkRadius, 0) + nextDir * 2f, // 조금 넉넉하게 잡고
            checkRadius, cubes, checkLayer);
        if (count == 0)
        { EndCharge(); return; }

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
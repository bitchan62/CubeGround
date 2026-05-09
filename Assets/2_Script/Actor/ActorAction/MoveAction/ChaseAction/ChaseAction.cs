using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class ChaseAction : MoveAction
{
    // ----- ai 부분 -----
    public Transform Target
    {
        get { return TargetManager.Instance.Target; }
    }

    protected NavMeshAgent _nav;  // 네비게이션 AI
    protected NavMeshAgent nav
    {
        get { return _nav ??= GetComponent<NavMeshAgent>(); }
    }

    private NavMeshPath navPath; // 길

    // 생성
    protected override void Awake()
    {
        base.Awake();
        // 네비게이션 초기화
        //nav = GetComponent<NavMeshAgent>();

        // 위치, 회전 자동 업데이트 비활성
        nav.updatePosition = false;
        nav.updateRotation = false;

        navPath = new NavMeshPath();
    }

    private void OnEnable()
    {
        Timer.Instance.StartEndlessTimer(this, "시작", 0.2f, UpdateDestination); // 목적지 업데이트
    }

    private void OnDisable()
    {
        Timer.Instance?.StopEndlessTimer(this, "시작");
    }


    // 타겟 설정
    // 필요없
    // public void SetTarget(Transform p_target)
    // { Target = p_target; }


    // target이 입력한 거리 이내에 있는지 확인
    public bool InDistance(int distance)
    { return (this.transform.position - Target.position).sqrMagnitude <= distance * distance; }

    public bool InDistance(float distance)
    { return (this.transform.position - Target.position).sqrMagnitude <= distance * distance; }


    // public override bool isMove
    // {
    //     get => base.isMove;
    //     set
    //     {
    //         // if (value)
    //         // {
    //         //     // 1회만 실행
    //         //     if (!isMove)
    //         //     { Timer.Instance.StartEndlessTimer(this, "시작", 0.2f, UpdateDestination); }
    //         // }
    //         // else
    //         // { Timer.Instance.StopEndlessTimer(this, "시작"); }
    //         base.isMove = value;
    //     }
    // }

    public override void Move()
    {
        UpdateNextMoveDirection(); // 다음 방향 설정
        UpdateMyPositionOnNav();   // 자신 위치 갱신

        // 타겟이 존재하고,
        // 길이 존재하는 경우에만 move
        if (Target != null && nav.hasPath)
        { base.Move(); }
    }

    public bool IsCanChase()
    {
        return navPath.status == NavMeshPathStatus.PathComplete &&
            !TargetManager.Instance.targetActor.damageReaction.isDie;
    }


    // 즉각 반응 AI
    private void UpdateDestination()
    {
        if (Target == null || nav == null || !nav.isOnNavMesh) { return; }
    
        NavMeshHit hit;
        Vector3 targetPosOnNav = nav.transform.position;
    
        // Target 주변 최대 30유닛 반경 내에서
        // NavMesh 위의 가장 가까운 점을 찾음
        if (NavMesh.SamplePosition(Target.position, out hit, 30.0f, NavMesh.AllAreas))
        { targetPosOnNav = hit.position; }
    
        // 목적지로 보정된 targetPosOnNav를 넣어 경로 계산
        if (NavMesh.CalculatePath(nav.transform.position, targetPosOnNav, NavMesh.AllAreas, navPath)
            && navPath.status == NavMeshPathStatus.PathComplete)
        { nav.SetPath(navPath); }
    }


    // 다음 이동 방향
    void UpdateNextMoveDirection()
    { moveVec = nav.desiredVelocity.normalized; }


    // 네비게이션 위치와 자신 위치 동기화
    public void UpdateMyPositionOnNav()
    { if (nav.isOnNavMesh) { nav.nextPosition = rigid.position; } }


    public void ReturnToNav()
    {
        if (nav == null) { /*Debug.LogError("nav == null");*/ return; }

        // nav상 위치와 transform 위치의 괴리
        float gapDistance = (nav.nextPosition - transform.position).sqrMagnitude;

        // 자신이 navMesh 위에 없는 경우
        // 또는 nav 위에 있더라도, nav상의 자신 Pos과 괴리 발생 시
        if (!nav.isOnNavMesh || 0.02f < gapDistance)
        {
            // Debug.Log("복귀");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(this.transform.position, out hit, 0.6f, NavMesh.AllAreas))
            { nav.Warp(hit.position); }
        }

        UpdateMyPositionOnNav();
    }
    


    // 회전 속도
    [SerializeField] protected float rotationSpeed = 3f;

    // 다음 진행 방향을 향해 회전 (느리게)
    public override void Turn()
    {
        Vector3 direction = moveVec;

        // moveVec이 0인 경우의 회전
        if (moveVec == Vector3.zero || !isMove)
        {
            direction = Target.position - transform.position;
            direction.y = 0;
        }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }


    // 타겟을 향해 바라보고 있는지 판별
    // 매개변수 tolerance : 어느 정도 각도까지 '정면'으로 바라보고 있는지를 판별
    // 0에 가까울수록 널널함
    public bool IsFacingTarget(float tolerance = 0.99f)
    {
        if (Target == null) { return false; }

        Vector3 toTarget = (Target.position - transform.position);
        toTarget.y = 0f; // y값 무시
        toTarget.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f; // y값 무시
        forward.Normalize();

        float dot = Vector3.Dot(forward, toTarget);

        return dot >= tolerance;
    }


    // target까지 장애물 여부 판별
    protected bool isClearToTarget(float rayDistance = 100f)
    {
        if (Target == null) { return false; }

        // <- Vector3.down * 2f과, Vector3.up * 2f 비교하는 부분이 유연하지 못함
        // 이후 어떤 상황에서도 사용 가능한 방식으로 바꿀 것을 고려할 것
        Vector3 directionToTarget = (Target.position + Vector3.down * 2f - transform.position).normalized;
        Ray ray = new Ray(transform.position + Vector3.up * 2f, directionToTarget);

        // "Cube"와 "Player" 레이어만 포함하는 레이어마스크 생성
        // <- 다소 유연하진 못함
        // target의 레이어를 가져오기? 이것도 고려해보자
        int cubeLayer = 1 << LayerMask.NameToLayer("Cube");
        int playerLayer = 1 << LayerMask.NameToLayer("Player");
        int layerMask = cubeLayer | playerLayer;

        // <- 디버그용 레이 시각화
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        // 지정한 레이어만 Raycast 대상으로 판별
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
        {
            if ((playerLayer & (1 << hit.transform.gameObject.layer)) != 0)
            { return true; }
        }

        return false;
    }


    // 레이캐스트 0.2초마다
    private bool isClearCash = false;
    private float lastClearCheckTime = -Mathf.Infinity;
    private float clearCheckInterval = 0.2f;

    // 캐시값 반환
    public bool isClearToTargetAsCash(float rayDistance = 100f)
    {
        if (Time.time - lastClearCheckTime > clearCheckInterval)
        {
            isClearCash = isClearToTarget(rayDistance);
            lastClearCheckTime = Time.time;
        }
        return isClearCash;
    }


}
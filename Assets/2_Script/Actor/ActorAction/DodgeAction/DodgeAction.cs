using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DodgeAction : ActorAction
{

    private PlayerMove playerMove;

    protected override void Awake()
    {
        base.Awake();

        // 기존 레이어 저장
        originalLayer = this.gameObject.layer;

        if (thisActor.moveAction is PlayerMove move)
        { playerMove = move; }
    }

    private void Start()
    {
        // 비활성화 상태
        this.enabled = false;
        originalDodgeWhenJumpCount = dodgeWhenJumpCount;
        thisActor.foot.whenGroundEvent.Add(() => dodgeWhenJumpCount = originalDodgeWhenJumpCount);
    }

    // 대시 거리
    [SerializeField] protected float dodgePower = 8;

    // 닷지 시간
    [SerializeField] protected float dodgeSlideTime = 0.4f;
    [SerializeField] protected float dodgeAngle = 30f;

    // 닷지 코스트
    [field: SerializeField] public int dodgeCost { get; set; } = 1;

    // 인스펙터에서 지정해서, 실제로 회전시킬 오브젝트
    [SerializeField] private GameObject ratateObjectWhenDodge;

    // 공중에서 사용 가능 횟수
    [SerializeField] public int dodgeWhenJumpCount = 3;
    private int originalDodgeWhenJumpCount;

    [Header("공중에서 3회 이상 사용했을 경우 발생할 소리 이펙트")]
    public EffectData canNotDodgeEffect = new EffectData();


    // 땃쥐 중?
    public bool isDodge { get; protected set; } = false;

    // 닷지 가능 여부
    public bool isCanDodge = true;

    // 원래 레이어
    private int originalLayer = -1;


    public void Dodge()
    {
        // --- dodge 중에는 dodge X ---
        if (isDodge) { return; }

        // --- dodge true ---
        this.enabled = true;
        // 1. 중력 미사용
        if (!thisActor.isRand) { thisActor.rigid.useGravity = false; }
        // 2. 닷지 각도 회전
        ratateObjectWhenDodge.transform.Rotate(dodgeAngle, 0, 0);
        // 3. 레이어 변경
        this.gameObject.layer = LayerMask.NameToLayer("IgnoreOtherActor");
        // 4. 벡터 초기화
        thisActor.rigid.velocity = Vector3.zero;
        // 5. 닷지 애니메이션 시작
        thisActor.actorAnimator.SetAnimationParam("IsDodge", true);

        // --- dodge false ---
        Timer.Instance.StartTimer(this, "_Dodge", dodgeSlideTime, Cancel);
    }

    public void Cancel()
    {
        if (isDodge)
        {
            this.enabled = false;
            // 1. 중력 복구
            thisActor.rigid.useGravity = true;
            // 2. 닷지 각도 복구
            ratateObjectWhenDodge.transform.Rotate(-dodgeAngle, 0, 0);
            // 3. 레이어 복구
            this.gameObject.layer = originalLayer;
            // 4. 벡터 x, z축 초기화
            Vector3 vector = thisActor.rigid.velocity;
            vector.x = 0f;
            vector.z = 0f;
            thisActor.rigid.velocity = vector;

            Timer.Instance.StopTimer(this, "_Dodge");
        }
    }

    private void OnEnable()
    { isDodge = true; }

    private void OnDisable()
    {
        isDodge = false;
        //Timer.Instance.StartTimer(this, "_EndDodge", 0.06f, () => { isDodge = false; });
        thisActor.actorAnimator.SetAnimationParam("IsDodge", false);
    }


    private bool IsCanPlayerMove()
    {
        if (playerMove == null) { return false; }
        return playerMove.IsCanMove();
    }

    // dodge 시 이동
    // x, z로만 힘
    private void FixedUpdate()
    { 
        if (!IsCanPlayerMove())
        {
            //Debug.Log("돌진 멈춤");
            thisActor.rigid.velocity = Vector3.zero;
            return;
        }

        Vector3 currentVelocity = thisActor.rigid.velocity; // 현재 속도 저장
        Vector3 forwardVelocity = transform.forward * dodgePower; // x,z 축 속도 계산
        thisActor.rigid.velocity = new Vector3(forwardVelocity.x, currentVelocity.y, forwardVelocity.z); // y 축은 그대로 유지
    }

}

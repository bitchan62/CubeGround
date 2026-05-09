using UnityEngine;

public class BossChargeAttack : ChargeAttack
{
    // 스턴 시 이벤트
    public MyCallBacks stunEvent = new MyCallBacks();

    [Tooltip("돌진 시작 시 회전 속도")]
    [SerializeField] protected float wheelSpeedWhenCharge = 1.5f;


    [Tooltip("돌진 시작 전 가볍게 점프할 때 이펙트")]
    public EffectData beforeChargeEffectData = new EffectData();

    [Tooltip("돌진 시작 시 이펙트")]
    public EffectData startChargeEffectData = new EffectData();

    [Tooltip("스턴 시 이펙트")]
    public EffectData stunEffectData = new EffectData();


    [Tooltip("스턴 시 자기 피해")]
    [SerializeField] private int stunSelfDamage = 3;

    [Header("스턴 시 카메라 울림")]
    [SerializeField] private float shaekRange = 0.5f;
    [SerializeField] private float shaekTime = 0.2f;


    protected override void Awake()
    {
        base.Awake();

        // <- 자해뎀으로 처치될 경우 점수 안 오르는 것 때문에...
        AttackData attackData = new AttackData();
        attackData.damage = stunSelfDamage;

        stunEvent.Add(() => { stunEffectData.Instantiate(gameObject); });
        stunEvent.Add(() => { thisActor.damageReaction.TakeDamage(attackData); });

        //  // <- 어택 리팩토링을 할 때 귀찮다고 보스차지어택을 리팩토링하지 않은 자의 말로다
        //  // 앞으로 리팩토링을 할 때는 목을 칠 것
        //  var myWeapon = weapon;
        //  var bossWeapon = weapon.GetComponent<BossActorWeapon>();
    }

    private void Start()
    {
        FollowCamera camera = Camera.main.GetComponent<FollowCamera>();
        if (camera != null) { stunEvent.Add(() => { camera?.ShakeCamera(shaekRange, shaekTime); }); }
    }

    protected override void BeforeAttack()
    {
        // --- 경고발판 생성 ---
        base.BeforeAttack();

        // --- 애니메이션 실행 ---
        thisActor.actorAnimator.SetAnimationParam("IsChargeAttack", true);

        // --- 공격 활성화 ---
        UseWeapon();

        // --- 플레이어를 향해 회전 ---
        Timer.Instance.StartRepeatTimer(this, "_RotateWarning", weaponBeforeDelay * 0.8f,
            () => {
                thisActor.moveAction.Turn();
                WarningPlaneCustom.Instance?.UpdateRotation(warningPlane, transform.forward);
                Vector3 posVec = transform.position + transform.forward * (warningPlane.transform.localScale.y / 2);
                WarningPlaneCustom.Instance?.UpdatePosition(warningPlane, posVec);
            });
    }


    protected override void CancelAttack()
    {
        base.CancelAttack();
        Timer.Instance.StopTimer(this, "_RotateWarning");
    }


    protected override void DoAttack()
    {
        // --- 원래 위치 확인 ---
        originPos = transform.position;

        // --- 발판 반환 ---
        WarningPlaneSetter.DelWarning(this, ref warningPlane);

        // -- 사망 시 리턴 ---
        if (thisActor.damageReaction.isDie) { return; }

        // --- 물리 조정 && 돌진 활성화 ---
        StartCharge();
    }


    protected override void StartCharge()
    {
        this.enabled = true;
        startChargeEffectData.Instantiate(gameObject);
        thisActor.actorAnimator.SetAnimationSpeed(wheelSpeedWhenCharge);
    }

    protected override void NotUseWeapon()
    {
        base.NotUseWeapon();
        thisActor.actorAnimator.SetAnimationParam("IsChargeAttack", false);
    }

    protected override void EndCharge()
    {
        base.EndCharge();
        //Debug.Log($"{owner.name} : EndCharge");
        WarningPlaneSetter.DelWarning(this, ref warningPlane);
        // Timer.Instance.StopTimer(this, "_RotateWarning");
        thisActor.actorAnimator.SetAnimationSpeed();
    }

    //이걸 사용해서 혼란에 빠지기 전에 큐브 폭파
    protected override void EndChargeWhenCube()
    {
        base.EndChargeWhenCube();
        stunEvent.Invoke();
        Timer.Instance.StartTimer(this, 0.1f, NavMeshManager.instance.BuildFull);
    }


    public void TempEffect()
    {
        beforeChargeEffectData.Instantiate(gameObject, transform.position, transform.rotation);
    }

}

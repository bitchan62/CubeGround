using UnityEngine;
using UnityEngine.SceneManagement;


//==================================================
// 피격으로 인한 피해 반응 / 사망 시 처리
//==================================================
public class DamageReaction : ActorAction
{
    [SerializeField] public bool _isInvincible = false; // 무적 상태 여부
    [SerializeField] private int maxHp = 10;  // 최대 생명력
    [SerializeField] private int nowHp = 10;  // 현재 생명력
    [SerializeField] private bool kenematicWhenDie = false;

    // 어린이 전용 무적키
    public bool trueInvinible = false;

    public bool isInvincible
    {
        get { return _isInvincible; }
        set { _isInvincible = value; }
    }

    private int originalLayer;

    [Tooltip("피격 시 발생할 이펙트")]
    public EffectData whenHitEffectData = new EffectData();

    [Tooltip("사망 시 발생할 이펙트")]
    public EffectData whenDieEffectData = new EffectData();

    [Header("이 몬스터를 플레이어가 처치 시 스코어")]
    [SerializeField] private int score = 10;
    protected ScoreManager scoreManager;


    // 생명력 통합
    public int healthPoint
    {
        get
        { return nowHp; }
        protected set
        {
            // 최대/최소값 보정
            if (value < 0) { value = 0; }
            else if (value > maxHp) { value = maxHp; }

            nowHp = value;
            whenHealthChange.Invoke();
        }
    }

    public bool isDie
    {
        get { return healthPoint <= 0; }
    }


    // 죽었을 때 바운스 거리
    [SerializeField] protected int bouncePowerWhenDie = 1;


    // hit/die 이벤트
    public MyCallBacks whenHit = new MyCallBacks();
    public MyCallBacks whenDie = new MyCallBacks();

    // 체력 변경 시마다
    public MyEvent whenHealthChange = new MyEvent();

    protected override void Awake()
    {
        base.Awake();
        originalLayer = gameObject.layer;
        scoreManager = Resources.Load<ScoreManager>("ScoreManager");
    }

    // 재활성화 시 초기화
    private void OnEnable()
    {
        nowHp = maxHp;
        gameObject.layer = originalLayer;
    }

    // 사망 시 플래그
    protected bool isDiedFlag = false;

    // ====================================
    // TakeDamage = 플레이어 원인 (점수 O)
    // ====================================
    public virtual void TakeDamage(AttackData attackData, bool isTrue = false)
    {
        if (isInvincible) { return; }
        if (attackData == null) { Debug.Log("null attackData"); return; }

        // 음수 체크
        if (attackData.damage < 0) { return; }

        // 데미지 적용
        if (!trueInvinible) { healthPoint -= attackData.damage; }

        // --- 이펙트 발생 ---
        whenHitEffectData.Instantiate(thisActor.gameObject);

        // 히트/다이 판정
        if (isDie && !isDiedFlag)
        {
            isDiedFlag = true;
            // 플레이어가 처치 시 점수 추가
            scoreManager?.AddScore(score);
            Die();
        }
        else if (!isDie) { Hit(); }
    }

    public virtual void KnockBack(KnockBackData knockBackData, Transform center)
    {
        if (isInvincible) { return; }
        if (knockBackData == null) { Debug.Log("null knockBackData"); return; }

        // 키네마틱 상태면 return
        if (center == null || thisActor.rigid.isKinematic)
        {
            //Debug.Log("넉백 ㄴㄴ");
            return;
        }

        //Debug.Log("넉백 ㅇㅇ");
        // 넉백 준비
        Vector3 knockBackForce = (this.transform.position - center.position).normalized;
        knockBackForce *= knockBackData.power;
        knockBackForce.y = knockBackData.height + thisActor.rigid.velocity.y;
        if (27f < knockBackForce.y) { knockBackForce.y = 27f; } // 과도한 vector 조절 (현재 27f)

        // 넉백 적용 (사망 시 추가 넉백)
        thisActor.rigid.velocity = isDie ?
            knockBackForce * bouncePowerWhenDie :
            knockBackForce;
    }



    // --- 거의 안 쓰는 놈들 --- ┐
    // ====================================
    // TakeDamage (오버로드) = 플레이어 원인 (점수 O)
    // ====================================
    public virtual void TakeDamage(int damage, Actor enemy = null, float knockBackPower = 0f, float knockBackHeight = 0f)
    {
        if (isInvincible) { return; }
        // --- 음수 데미지 체크 ---
        if (damage < 0)
        {
            Debug.Log($"{enemy.gameObject.name}의 공격 데미지가 {damage}");
            return;
        }

        // --- 피해 적용 ---
        if (!trueInvinible) { healthPoint -= damage; }

        // --- 이펙트 발생 ---
        whenHitEffectData.Instantiate(thisActor.gameObject);

        // --- 피격/사망 시 처리 ---
        if (isDie && !isDiedFlag)
        {
            isDiedFlag = true;
            // 플레이어가 처치 시 점수 추가
            scoreManager?.AddScore(score);
            Die();
        }
        else if (!isDie) { Hit(); }
    }


    // ====================================
    // TrueTakeDamage = 이외 원인 (점수 X)
    // ====================================
    public virtual void TrueTakeDamage(int damage)
    {
        if (isInvincible) { return; }

        // 음수 체크
        if (damage < 0) { return; }

        // 데미지 적용
        if (!trueInvinible) { healthPoint -= damage; }

        // 이펙트 발생
        whenHitEffectData.Instantiate(thisActor.gameObject);

        // 히트/다이 판정 (점수 추가 없음)
        if (isDie)
        {
            Die(); // 점수 없이 사망
        }
        else { Hit(); }
    }


    // 넉백 따로 만들기
    public virtual void KnockBackImpulse(GameObject enemy, float knockBackPower, float knockBackHeight)
    {
        if (isInvincible) { return; }
        // 키네마틱 상태면 return
        if (enemy == null || thisActor.rigid.isKinematic) { return; }

        // 넉백 준비
        Vector3 tempVector = (this.transform.position - enemy.transform.position).normalized;

        tempVector *= knockBackPower;
        tempVector.y = knockBackHeight + thisActor.rigid.velocity.y; // 상/하 넉백
        if (27f < tempVector.y) { tempVector.y = 27f; }    // 과도한 vector 조절 (현재 27f)
        thisActor.rigid.velocity = tempVector; // 넉백 적용

        // 사망 시 추가넉백
        if (isDie)
        { thisActor.rigid.velocity = tempVector * bouncePowerWhenDie; }
    }

    // --- 거의 안 쓰는 놈들 --- ┘


    protected void Hit()
    { whenHit.Invoke(); }


    // 사망 처리
    protected virtual void Die()
    {
        // --- die 이벤트 호출 ---
        whenDie.Invoke();
        whenDieEffectData?.Instantiate(gameObject);

        // --- 물리 적용 ---
        thisActor.rigid.isKinematic = kenematicWhenDie;

        // 레이어 변경
        int targetLayer = LayerMask.NameToLayer("DieActorLayer");
        LayerChanger.ChangeLayerWithAll(this.gameObject, targetLayer);
    }

    public void Heal(int amount)
    {
        healthPoint += amount;
    }

    public int maxHealthPoint
    {
        get { return maxHp; }
    }
}
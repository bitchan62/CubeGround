using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamageReaction : DamageReaction
{
    [SerializeField] float redTimeWhenHit = 0.2f;
    [SerializeField] float redTimeWhenDie = 0.3f;
    [SerializeField] float redColor = 0.6f;


    protected override void Awake()
    {
        base.Awake();

        // 피격 시 빨개지기
        ColorChangeAction colorChangeAction = GetComponent<ColorChangeAction>();
        if (colorChangeAction == null)
        { colorChangeAction = this.gameObject.AddComponent<ColorChangeAction>(); }

        // 빨간색 세팅
        colorChangeAction.SetRed(redColor);


        // 히트/다이 이벤트 세팅
        System.Action hitRedAction = null;

        hitRedAction = () => {
                colorChangeAction.ChangeToRed();
                Timer.Instance.StartTimer(this, redTimeWhenHit, colorChangeAction.RestoreOriginalColors); };
        whenHit.Add(hitRedAction);

        hitRedAction = () => {
            colorChangeAction.ChangeToRed();
            Timer.Instance.StartTimer(this, redTimeWhenDie, colorChangeAction.RestoreOriginalColors); };
        whenDie.Add(hitRedAction);


        //  // 몬스터 리스트 Add
        //  ClearTriggerListManager.Instance.Add(thisActor);
        // whenDie : 몬스터 리스트 Remove

        // 사망 시 가라앉음 이벤트
        whenDie.Add(FallWhenDie, 1);
    }


    // public override void TakeDamage(int damage, Actor enemy, float knockBackPower = 0, float knockBackHeight = 0)
    // {
    //     Monster monster = GetComponent<Monster>();
    //     if (monster != null && enemy != null)
    //     {
    //         Transform tempTrans = monster.Target;
    //         //monster.Target = enemy.transform;
    //     }
    // 
    //     base.TakeDamage(damage, enemy, knockBackPower, knockBackHeight);
    //     // SoundManager.Instance.PlayMonsterHit();
    // }


    [Tooltip("사망 후 사라지는 시간")]
    public float remainTime = 2f;
    [Tooltip("사망 후 가라앉기 시작 시간")]
    public float fallStartTime = 1f;
    [Tooltip("가라앉는 속도")]
    public float fallSpeed = 1f;

    protected virtual void FallWhenDie()
    {
        // --- 가라앉기 / 제거 ---

        Timer.Instance.StartTimer(this, fallStartTime, 
            () => {
                int i = 0;
                foreach (Transform t in this.transform)
                {
                    i++;
                    Timer.Instance.StartRepeatTimer(this,
                        "_FallDown" + i,
                        remainTime - fallStartTime,
                        () => t.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World));
                }
            });

        //  // 가라앉기 (콜라이더를 위로 빼기)
        //  CapsuleCollider[] cols = GetComponentsInChildren<CapsuleCollider>();
        //  
        //  // 1초 뒤부터 가라앉기 시작
        //  Timer.Instance.StartTimer(this, fallStartTime,
        //      () => {
        //          // 모든 콜라이더 수집 후 일괄 작동
        //          int i = 0;
        //          foreach (var col in cols)
        //          {
        //              i++;
        //              Timer.Instance.StartRepeatTimer(this,
        //                  "_FallDown" + i,
        //                  remainTime - fallStartTime,
        //                  () => col.center -= Vector3.down * fallSpeed * Time.deltaTime);
        //          }
        //      });



        // 2초 후 비활성화
        Timer.Instance.StartTimer(this, "_WhenDie", remainTime, () => Destroy(this.gameObject));
    }

}

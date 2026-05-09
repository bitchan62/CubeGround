using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : Actor
{
    public FinalBossHp finalBossHp;

    [Tooltip("사망 시 가라앉는 정도")]
    public float fallHeight = 5f;


    private void Start()
    {
        // 보스 Hp에 등록
        finalBossHp?.Init(damageReaction);

        System.Action tempAction = () => transform.position -= Vector3.up * Time.deltaTime * fallHeight;
        damageReaction.whenDie.Add(() => {
            if (damageReaction is MonsterDamageReaction monsterReaction)
            { Timer.Instance.StartRepeatTimer(this, "Die", monsterReaction.remainTime, tempAction); }
        });
    }
}

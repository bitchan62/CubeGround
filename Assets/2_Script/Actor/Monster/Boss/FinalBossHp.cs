using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.CoreUtils;

[CreateAssetMenu(fileName = "FinalBoss", menuName = "Game Data/FinalBossHp")]
public class FinalBossHp : ScriptableObject
{
    private List<DamageReaction> reactions = new List<DamageReaction>();

    [field: SerializeField]
    [field: Tooltip("총 생명력")]
    public int originalHp { get; private set; }

    [field: SerializeField]
    [field: Tooltip("공유 HP")]
    public int sharedHp { get; private set; }

    // 피해 입었을 경우 이벤트
    public event Action whenDamaged;
    public event Action whenDied;

    public void Init(DamageReaction damageReaction)
    {
        reactions.Add(damageReaction);
        damageReaction.whenHit.Add(() => Damage(1));
    }

    public void Damage(int damage)
    {
        sharedHp -= damage;
        whenDamaged?.Invoke();
        Debug.Log($"FinalBossHp : {sharedHp}");

        // sharedHp가 다 떨어지면
        // 싹 죽여버리기
        if (sharedHp <= 0)
        {
            foreach (var reaction in reactions)
            {
                // Debug.Log("전부 죽인다");
                reaction.TrueTakeDamage(int.MaxValue);
            }
            whenDied?.Invoke();
        }
    }

    // 반드시 OnEnable에 이거 넣어야 됨 (이미 했슴)
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        reactions = new List<DamageReaction>();
        sharedHp = originalHp;
        whenDamaged = null;
        whenDied = null;
    }

}

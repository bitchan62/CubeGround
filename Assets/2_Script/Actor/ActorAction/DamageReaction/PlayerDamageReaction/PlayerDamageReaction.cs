using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageReaction : DamageReaction
{
    // 무적 시간
    [SerializeField] protected float invincibilityTime = 1f;
    // 무적 시간 중 깜빡이 주기
    [SerializeField] protected float flikerTime = 0.1f;

    // 렌더러
    private List<Renderer> targetRenderers = new List<Renderer>();
    private bool isBlinkOn = true;

    // 플레이어 사망 후, 애니메이션 종료 시 event
    public event Action whenPlayerDie;

    // scoreManager는 부모(DamageReaction)에서 상속받음

    protected void Start()
    {
        whenHit.Add(Invincibility);
        thisActor.fallingAction.whenAfterFalling += ReInvincibility;

        // 피격 시 점수 차감 (-100)
        whenHit.Add(() =>
        {
            scoreManager?.AddScore(-100);
            Debug.Log("[PlayerDamageReaction] 피격: -100점");
        });
        CollectAllRelevantRenderers();
    }

    // === mesh 깜빡임 ===
    // 모든 mesh 수집
    void CollectAllRelevantRenderers()
    {
        targetRenderers.Clear();
        // 1. 자기 자신 포함 자식 전체
        Renderer[] childrenRenderers = GetComponentsInChildren<Renderer>(true);
        targetRenderers.AddRange(childrenRenderers);
        // 2. 부모들 모두(최상위까지)
        Transform current = transform.parent;
        while (current != null)
        {
            Renderer[] parentRenderers = current.GetComponents<Renderer>();
            if (parentRenderers != null)
            { targetRenderers.AddRange(parentRenderers); }
            current = current.parent;
        }
    }

    // 모든 렌더러 활성화/비활성화
    void SetRenderersEnabled(bool enabled)
    {
        foreach (var rend in targetRenderers)
        {
            if (rend != null)
            { rend.enabled = enabled; }
        }
    }

    // === 무적 ===
    protected void Invincibility()
    {
        Timer.Instance.StartEndlessTimer(this, "_Fliker", flikerTime, () => SetRenderersEnabled(isBlinkOn = !isBlinkOn));
        isInvincible = true;
        Timer.Instance.StartTimer(this, "_Invincibility", invincibilityTime,
            () =>
            {
                isInvincible = false;
                Timer.Instance.StopEndlessTimer(this, "_Fliker");
                SetRenderersEnabled(true);
            });
    }

    protected void ReInvincibility()
    {
        Timer.Instance.StopEndlessTimer(this, "_Fliker");
        Timer.Instance.StopTimer(this, "_Invincibility");
        Invincibility();
    }

    // === TakeDamage (몬스터 공격 = 점수 차감 O) ===
    public override void TakeDamage(AttackData attackData, bool isTrue = false)
    {
        if (isInvincible) { return; }
        base.TakeDamage(attackData, isTrue);
        // SoundManager.Instance.PlayPlayerHit();
    }

    public override void TakeDamage(int damage, Actor enemy, float knockBackPower = 0, float knockBackHeight = 0)
    {
        if (isInvincible) { return; }
        base.TakeDamage(damage, enemy, knockBackPower, knockBackHeight);
        // SoundManager.Instance.PlayPlayerHit();
    }

    // === TrueTakeDamage ===
    public override void TrueTakeDamage(int damage)
    {
        if (isInvincible) { return; }

        // base의 TrueTakeDamage 호출 (점수 차감 없음)
        base.TrueTakeDamage(damage);
    }

    public void EventWhenPlayerDie()
    {
        whenPlayerDie?.Invoke();
    }
}
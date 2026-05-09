using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

abstract public class ActorWeapon : MonoBehaviour
{
    // 무기의 콜라이더
    protected Collider weaponCollider = null;

    // 이펙트 프리펩
    protected GameObject hitEffect = null;
    protected float effectDestoryTime = 1f; // <- LeftTime 설정 고려


    // 이펙트 발생
    protected void InstantHitEffect()
    {
        // Quaternion.identity : 회전값 (0, 0, 0)
        if (hitEffect != null)
        {
            GameObject effect =
                Instantiate(hitEffect,
                transform.position,
                transform.rotation);
            Destroy(effect, effectDestoryTime);
        }
    }

    // 지정한 위치에서 발생
    // 위쪽 방향을 바라보도록
    protected void InstantHitEffectAtPos(Vector3 pos)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, pos, Quaternion.LookRotation(Vector3.up));
            Destroy(effect, effectDestoryTime);
        }
    }

    // 이펙트 발생 (상대와 가장 가까운 위치)
    protected void InstantHitEffectAtClosest(Vector3 otherPosition)
    {
        Vector3 effectPos = weaponCollider.ClosestPoint(otherPosition);

        // Quaternion.identity : 회전값 (0, 0, 0)
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, effectPos, transform.rotation);
            Destroy(effect, effectDestoryTime);
        }
    }


    protected virtual void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true; // 트리거 콜라이더 활성화
            isActivate = false;  // 기본적으로 비활성 (공격 시에만 활성)
            this.enabled = false;
        }
        else
        { Debug.Log("ActorWeapon에 콜라이더 미존재 : " + this.gameObject.name); }
    }


    // ===== 무기 콜라이더 활성화 / 비활성화 =====
    public bool isActivate
    {
        set
        {
            if (weaponCollider != null)
            { weaponCollider.enabled = value; }

            this.enabled = value;

            // 활성화 시
            if (value) { hitTargets = new Dictionary<GameObject, int>(); }

            // 비활성화 시
            else { hitTargets = null; }
        }
    }

    // 공격의 판정 횟수 체크
    // 공격 대상(key) / 히트 횟수(value)
    protected Dictionary<GameObject, int> hitTargets = null;

    // attackAction으로부터 가져올 정보
    protected string targetTag = "";
    protected int attackDamage = 0;
    protected int maxHitCount = 1; // 최대 히트 횟수
    protected Actor owner = null; // 해당 무기를 소유하고 있는 개체
    protected float knockBackPower = 0f;
    protected float knockBackHeight = 0f;

    public virtual void SetWeapon(string p_targetTag, Actor p_owner)
    {
        targetTag = p_targetTag;
        owner = p_owner;
    }


    // 활성화된 횟수
    // Do/NotUse 짝을 맞춰줘야 함
    private int _activateStack = 0;
    protected int activateStack
    {
        get { return _activateStack; }
        set
        {
            if (value < 0) { value = 0; }
            _activateStack = value;
        }
    }

    public virtual void UseWeapon(
        int p_attackDamage,
        int p_maxHitCount,
        float p_knockBackPower,
        float p_knockBackHeight,
        GameObject p_hitEffect = null,
        float p_effectDestoryTime = 1f)
    {
        if (0 < ++activateStack)
        { isActivate = true; }

        // 무기 능력치 대입
        attackDamage = p_attackDamage;
        maxHitCount = p_maxHitCount;
        knockBackPower = p_knockBackPower;
        knockBackHeight = p_knockBackHeight;
        hitEffect = p_hitEffect;
        effectDestoryTime = p_effectDestoryTime;
    }

    public virtual void NotUseWeapon()
    {
        if (--activateStack <= 0)
        {
            activateStack = 0;
            isActivate = false;
        }
    }


    // <- cube가 이미 포함되어 있으면 enter X

    // ===== 트리거 엔터 기반 판정 =====
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!this.enabled) { return; }
        DamageReaction damageReaction = other.GetComponent<DamageReaction>();

        if (other.CompareTag(targetTag) && // 공격 대상 태그 일치 확인
            damageReaction != null &&      // 데미지 입히기 가능 확인
            hitTargets != null)
        { WeaponCollisionEnterAction(damageReaction); }
    }

    protected virtual void WeaponCollisionEnterAction(DamageReaction damageReaction)
    {
        int hitCount = 0;
        hitTargets.TryGetValue(damageReaction.gameObject, out hitCount);

        // 최대 히트 횟수 확인 및 처리
        if (hitCount < maxHitCount)
        {
            hitTargets[damageReaction.gameObject] = hitCount + 1; // hitCount += 1
            // --- 넉백 ---
            damageReaction.KnockBackImpulse(this.gameObject, knockBackPower, knockBackHeight);
            damageReaction.TakeDamage(attackDamage, owner); // 데미지 적용
        }
    }


}
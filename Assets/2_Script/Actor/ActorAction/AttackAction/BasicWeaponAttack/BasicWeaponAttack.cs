using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeaponAttack : AttackAction
{
    // 무기로 사용할 개체
    // 반드시 인스펙터 창에서 지정되어 있어야 함
    [SerializeField] private GameObject myWeapon = null;

    // 무기가 활성화되어있을 시간
    [SerializeField] protected float weaponActiveTime = 1f;

    // 공격 이펙트
    [Tooltip("공격 시 발생할 이펙트")]
    [SerializeField] protected GameObject attackEffect = null;
    [Tooltip("attackEffect가 발생할 위치")]
    [SerializeField] protected Transform attackEffectPos = null;


    // BasicActorWeapon 캐시
    private BasicActorWeapon _weapon = null;
    protected BasicActorWeapon weapon
    {
        get
        {
            if (_weapon == null)
            {
                _weapon = myWeapon.GetComponent<BasicActorWeapon>();
                if (_weapon == null ) { _weapon = myWeapon.AddComponent<BasicActorWeapon>(); }
            }
            return _weapon;
        }
    }


    protected override void Awake()
    {
        base.Awake();
        weapon.SetWeapon(targetTag, thisActor);
        attackRate += weaponActiveTime;
    }


    protected virtual void UseWeapon()
    { weapon.UseWeapon(attackDamage, maxHitCount, knockBackPower, knockBackHeight, hitEffect, effectDestroyTime); }

    protected virtual void NotUseWeapon()
    { weapon.NotUseWeapon(); }


    protected override void DoAttack()
    {
        base.DoAttack();

        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, attackEffectPos);
            Destroy(effect, effectDestroyTime);
        }

        // --- 무기 활성화 ---
        UseWeapon();

        // --- 일정 시간 후 무기 비활성화 ---
        Timer.Instance.StartTimer(this, weaponActiveTime, NotUseWeapon);
    }
}
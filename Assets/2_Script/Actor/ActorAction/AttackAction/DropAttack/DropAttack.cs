using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

// 점프 공격
public class DropAttack : AttackAction
{
    // 낙하 공격 rigid.AddForce
    protected Rigidbody rigid;

    // 무기로 사용할 개체
    // 반드시 인스펙터 창에서 지정되어 있어야 함
    [SerializeField] private GameObject myWeapon = null;

    // 광역 피해량
    [SerializeField] private int splashDamage = 1;

    // 낙하 속도
    [SerializeField] private float dropSpeed = 13f;

    // 낙하 가능한 최대 거리
    [SerializeField] private float maxDropDistance = 20f;


    private DropActorWeapon _weapon = null;
    protected DropActorWeapon weapon
    {
        get
        {
            if (_weapon == null)
            {
                _weapon = myWeapon.GetComponent<DropActorWeapon>();
                if(_weapon == null)
                { _weapon = myWeapon.AddComponent<DropActorWeapon>(); }
            }
            return _weapon;
        }
    }


    protected override void Awake()
    {
        base.Awake();
        weapon.SetWeapon(targetTag, attackRange, this.gameObject.layer, GetComponent<Actor>(), splashDamage);
        rigid = GetComponent<Rigidbody>();
    }


    protected override void DoAttack()
    {
        base.DoAttack();

        // 무기 사용 시작
        weapon.UseWeapon(attackDamage, maxHitCount, knockBackPower, knockBackHeight, hitEffect, effectDestroyTime);

        if (!thisActor.isRand)
        {
            // 레이캐스트 정보
            RaycastHit tempHit;
            // 낙하 시도 여부
            bool isCanUseDropAttack = Physics.Raycast(this.transform.position, Vector3.down, out tempHit, maxDropDistance);

            // --- 낙하 여부 ---
            if (isCanUseDropAttack)
            { thisActor.rigid.velocity = Vector3.down * dropSpeed; }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


// <- 몬스터라는 가정으로 제작됨 (일단 이 방식대로)
public class FireAction : AttackAction
{
    // 발사체
    [SerializeField] protected GameObject projectile;

    // 발사 위치
    [SerializeField] protected Transform firePos;

    // 발사 타입
    [SerializeField] protected FireType fireType;

    // 대상 위치
    protected Vector3 targetPos;

    // 타겟
    private Transform _target;
    public Transform target
    {
        get
        {
            if (_target == null && thisActor is Monster monster)
            { _target = monster.Target; }
            return _target;
        }
    }

    protected override void BeforeAttack()
    {
        base.BeforeAttack();
        targetPos = target.transform.position;
    }

    protected override void DoAttack()
    {
        base.DoAttack();
        Fire();
    }


    protected virtual void Fire()
    {
        if (this.projectile != null)
        {
            // 투사체 생성하기
            GameObject instantProjectile = Instantiate(this.projectile, firePos.position, this.transform.rotation); // <- 발사 position 조절

            // 투사체 이동 방식 가져옴
            ProjectileMove moveAction = instantProjectile.GetComponent<ProjectileMove>();
            if (moveAction != null)
            {
                switch(fireType)
                {
                    // 목표 위치 지정 (아쳐)
                    case FireType.Straight:
                        moveAction.SetTarget(targetPos);
                        break;

                    // 추적 대상 지정 (메이지)
                    case FireType.Guided:
                        moveAction.SetTarget(target);
                        break;
                }
            }
            else { Debug.Log("FireAction : 잘못된 Projectile object 등록됨 : " + gameObject.name); }

            // 투사체 활성화
            ProjectileWeapon tempProjectile = instantProjectile.GetComponent<ProjectileWeapon>();
            if (tempProjectile != null)
            {
                tempProjectile.SetWeapon(targetTag, GetComponent<Actor>());
                tempProjectile.UseWeapon(attackDamage, maxHitCount, knockBackPower, knockBackHeight, hitEffect, effectDestroyTime);
            }
        }
        else { Debug.Log("Projectile 지정되지 않음 : " + gameObject.name); }
    }


}

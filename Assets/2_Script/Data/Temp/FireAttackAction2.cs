using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireType
{
    Straight,
    Guided
}

public class FireAttackAction2 : AttackAction2
{
    [Tooltip("투사체 데이터")]
    public ProjectileData projectileData = new ProjectileData();

    //  [SerializeField]
    //  [Tooltip("투사체 갯수")]
    //  [Range(1, 20)]
    //  protected int projectileNum = 1;
    //  
    //  [SerializeField]
    //  [Tooltip("투사체 확산 계수. 값이 클수록 넓게 퍼집니다.")]
    //  [Range(0, 0.5f)]
    //  public float spreadFactor = 0.1f;

    protected Transform target;
    Vector3 targetPos;

    protected override void Awake()
    {
        base.Awake();
        if (thisActor is Monster monster)
        {
            target = monster.Target;
        }
    }

    public override void Attack()
    {
        base.Attack();
        targetPos = target.position;
    }

    public override void Do()
    {
        base.Do();

        if (target == null) { return; }

        // --- 투사체 생성 및 설정 ---
        GameObject obj = Instantiate(projectileData.prefab.gameObject,
            projectileData.firePos.position, this.transform.rotation);

        Projectile2 projectile = obj.GetComponent<Projectile2>();
        if (projectile == null) { return; }

        projectile.isActivate = true;

        projectile.SetWeaponOwner(thisActor);
        projectile.SetData(projectileData);
        projectile.SetData(attackData);
        projectile.SetData(knockBackData);
        projectile.SetData(hitEffect);

        projectile.SetTarget(targetPos);
        projectile.SetTarget(target);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FireGateOpenAction : AttackAction2
{
    [Tooltip("투사체 데이터")]
    public ProjectileData projectileData = new ProjectileData();

    [Tooltip("발사 위치의 부모들")]
    public Transform[] firePosParents;

    [Tooltip("발사 게이트 오픈 사이의 딜레이")]
    public float gateOpenDelay = 0.5f;

    [Tooltip("게이트 오픈 이후 발사까지의 딜레이")]
    public float fireDelay = 1f;

    [Tooltip("발사 시 이펙트 데이터")]
    public EffectData fireEffectData = new EffectData();

    protected Transform target;
    private Vector3 targetPos;
    // private Dictionary<int, List<Transform>> fireGates = new Dictionary<int, List<Transform>>();
    private List<List<Transform>> fireGates = new List<List<Transform>>();

    private int fireGatesCount = 0;
    private int FireGatesCount
    {
        get { return fireGatesCount++ % fireGates.Count; }
    }

    protected override void Awake()
    {
        base.Awake();


        for (int i = 0; i < firePosParents.Length; i++)
        {
            // firePosParents.Length만큼 new List
            fireGates.Add(new List<Transform>());

            // 각 fireGates에다가 firePos Add
            foreach (Transform firePos in firePosParents[i])
            { fireGates[i].Add(firePos); }
        }

        //  int i = 0;
        //  foreach (Transform firePosParent in firePosParents)
        //  {
        //      fireGates.Add(new List<Transform>());
        //  
        //      foreach (Transform firePos in firePosParent)
        //      { fireGates[i].Add(firePos); }
        //  
        //      // if (!fireGates.ContainsKey(i)) { fireGates.Add(i, new List<Transform>()); }
        //  
        //      // foreach (Transform firePos in firePosParent)
        //      // { fireGates[i].Add(firePos); }
        //  
        //      i++;
        //  }
    }

    private void Start()
    {
        if (thisActor is Monster monster)
        { target = monster.Target; }

        thisActor.damageReaction.whenDie.Add(() =>
        {

        });
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

        int i = 0;
        foreach (Transform fireGate in fireGates[FireGatesCount])
        {
            System.Action gateOpen = () =>
            {
                fireEffectData.effectPos = fireGate;
                fireEffectData.Instantiate(gameObject);

                System.Action fire = () =>
                {
                    Projectile2 projectile = projectileData.Instantiate(fireGate);
                    if (projectile == null) { return; }
                    
                    projectile.isActivate = true;
                    
                    projectile.SetWeaponOwner(thisActor);
                    projectile.SetData(projectileData);
                    projectile.SetData(attackData);
                    projectile.SetData(knockBackData);
                    projectile.SetData(hitEffect);
                    
                    projectile.SetTarget(targetPos);
                    projectile.SetTarget(target);
                };

                Timer.Instance.StartTimer(this, fireDelay, fire);
            };

            i += 1;
            Timer.Instance.StartTimer(this, i * gateOpenDelay, gateOpen);
        }

        //   int i = 0;
        //   foreach (var projectileData in projectileDataList)
        //   {
        //       System.Action gateOpen = () =>
        //       {
        //           // --- 이펙트 생성 ---
        //           fireEffectData.effectPos = projectileData.firePos;
        //           fireEffectData.Instantiate(gameObject);
        //   
        //           System.Action fire = () =>
        //           {
        //               // --- 투사체 생성 및 설정 ---
        //               // GameObject obj = Instantiate(projectileData.prefab.gameObject,
        //               //     projectileData.firePos.position, this.transform.rotation);
        //   
        //               Projectile2 projectile = projectileData.Instantiate();
        //               //obj.GetComponent<Projectile2>();
        //               if (projectile == null) { return; }
        //   
        //               projectile.isActivate = true;
        //   
        //               projectile.SetWeaponOwner(owner);
        //               projectile.SetData(projectileData);
        //               projectile.SetData(attackData);
        //               projectile.SetData(knockBackData);
        //               projectile.SetData(hitEffect);
        //   
        //               projectile.SetTarget(targetPos);
        //               projectile.SetTarget(target);
        //           };
        //   
        //           Timer.Instance.StartTimer(this, fireDelay, fire);
        //       };
        //   
        //       i += 1;
        //       Timer.Instance.StartTimer(this, i * gateOpenDelay, gateOpen);
        //   }


    }
}

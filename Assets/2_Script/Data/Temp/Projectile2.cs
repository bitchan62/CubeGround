using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile2 : ActorWeapon2
{
    private MoveAction moveAction;

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        if (moveAction == null)
        { Debug.Log($"{this.gameObject.name} : moveAction 부재"); }
        moveAction.isMove = true;

        // 명중 시 삭제
        whenHit.Add(() => EffectAndDestory(), 1);
    }

    private void Update()
    {
        if (owner.damageReaction.isDie)
        { EffectAndDestory(); }
    }

    private void FixedUpdate()
    {
        if (moveAction.isMove)
        { moveAction.Move(); }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Cube"))
        { EffectAndDestory(); }
    }

    public override void SetData<T>(T data)
    {
        base.SetData(data);
        if (data is ProjectileData projectile)
        {
            moveAction.moveSpeed = projectile.speed;
            Timer.Instance.StartTimer(this, "_파괴 시간", projectile.duration, () => EffectAndDestory());
        }
    }

    protected void EffectAndDestory(float time = 0f)
    {
        // <- 현재 hitEffect는 owner의 소유
        // projectile에서 sound 발생 시 owner 위치에서 발생할 가능성 있음
        // 따라서 owner과는 별개의 obj를 만들어준 뒤, 그 위치에서 effect 발생
        GameObject obj = new GameObject();
        obj.transform.position = transform.position;
        hitEffect?.Instantiate(obj, transform.position, transform.rotation);

        Destroy(this.gameObject, time);
        Destroy(obj, 2f);
    }


    public void SetTarget(Vector3 pos)
    {
        if (moveAction is ProjectileMove move)
        {
            //Debug.Log($"{name} : SetTarget(Vector3)");
            move.SetTarget(pos);
        }
    }

    public void SetTarget(Transform target)
    {
        if (moveAction is GuidedProjectileMove guided)
        {
            //Debug.Log($"{name} : SetTarget(Transform)");
            guided.SetTarget(target);
        }
    }

    protected override void OnDisable()
    {
        // null 처리 X
    }

}

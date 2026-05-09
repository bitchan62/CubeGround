using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 오브젝트가 Cube 사이에 끼어있는 경우를 판별
// <- 현재 미사용 (SandwitchedReaction이 대체 중)
public class BetweenCubeAction : ActorAction
{
    // 끼었을 경우의 피해량
    [SerializeField] protected int whenSandwichedDamage = 3;


    // 콜라이더가 접촉해온 방향
    private enum ImpactDirection
    {
        Top, Bottom,
        Left, Right,
        Forward, Back
    }

    // ImpactDirection의 반대 방향을 반환하는 함수
    private ImpactDirection GetOppositeDirection(ImpactDirection dir)
    {
        switch (dir)
        {
            case ImpactDirection.Top: return ImpactDirection.Bottom;
            case ImpactDirection.Bottom: return ImpactDirection.Top;
            case ImpactDirection.Left: return ImpactDirection.Right;
            case ImpactDirection.Right: return ImpactDirection.Left;
            case ImpactDirection.Forward: return ImpactDirection.Back;
            case ImpactDirection.Back: return ImpactDirection.Forward;
            default: return dir; // <- 존재X
        }
    }


    // 콜라이더와, 그 콜라이더가 접촉한 방향
    private Dictionary<Collider, ImpactDirection> collisionDirections = new Dictionary<Collider, ImpactDirection>();

    // collisionDirections에 반대 방향이 있는지 확인하는 함수
    //  true: 반대 방향 O
    // false: 반대 방향 X
    private bool HasOppositeDirection(ImpactDirection dir)
    {
        ImpactDirection opposite = GetOppositeDirection(dir);

        foreach (var value in collisionDirections.Values)
        { if (value == opposite) { return true; } }

        return false;
    }


    // 어디에서부터 충돌했는지 방향 구하기
    private ImpactDirection GetDirectionFromWhereToThis(Vector3 normal)
    {
        float absX = Mathf.Abs(normal.x);
        float absY = Mathf.Abs(normal.y);
        float absZ = Mathf.Abs(normal.z);

        if (absX > absY && absX > absZ)
        { return normal.x > 0 ? ImpactDirection.Right : ImpactDirection.Left; }

        if (absY > absZ)
        { return normal.y > 0 ? ImpactDirection.Top : ImpactDirection.Bottom; }

        return normal.z > 0 ? ImpactDirection.Forward : ImpactDirection.Back;
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("enter");
        EnterUpdate(collision);
    }

    private void OnCollisionExit(Collision collision)
    { ExitUpdate(collision); }


    // 콜라이더 접촉 시 추가
    private void EnterUpdate(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Cube")) { return; }

        foreach (ContactPoint contact in collision.contacts)
        {
            Collider otherCollider = contact.otherCollider;

            // --- 어디에서부터 충돌했는지 확인 ---
            ImpactDirection dir = GetDirectionFromWhereToThis(contact.normal);

            // --- 반대 위치의 충돌체가 이미 존재하는지 확인 ---
            if (HasOppositeDirection(dir))
            {
                Debug.Log("충돌 발생");
                
                DamageReaction damageReaction = GetComponent<DamageReaction>();
                Actor actor = GetComponent<Actor>();
                if (damageReaction != null && actor != null)
                { damageReaction.TakeDamage(whenSandwichedDamage, actor); }

                // <- 텔포

                break;
            }
            else
            { collisionDirections[otherCollider] = dir; }
        }
    }


    // 접촉해있었던 콜라이더 제거
    private void ExitUpdate(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Cube")) { return; }

        // 키가 있으면 제거
        Collider otherCollider = collision.collider;
        collisionDirections.Remove(otherCollider);
    }

}
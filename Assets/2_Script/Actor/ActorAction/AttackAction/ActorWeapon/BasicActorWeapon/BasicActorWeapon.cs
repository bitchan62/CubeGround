using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 무기 오브젝트에 장착 (자동 포함)
/// <summary>
/// 현재 bossChargeAttack에서만 사용 중.
/// 왜 리팩토링할 때 같이 수정하지 않았지? 똑바로 서라
/// </summary>
public class BasicActorWeapon : ActorWeapon
{
    private void Start()
    {
        // 이미 다른 BasicActorWeapon이 존재하는 경우 : 스스로 제거
        BasicActorWeapon[] weapons = GetComponents<BasicActorWeapon>();
        foreach (BasicActorWeapon weapon in weapons)
        { if (weapon != this) { Destroy(this); } }
    }


    protected override void WeaponCollisionEnterAction(DamageReaction damageReaction)
    {
        // Debug.Log("실행");
        base.WeaponCollisionEnterAction(damageReaction);
        InstantHitEffectAtClosest(damageReaction.transform.position);
    }



    // 보스웨펀 콜라이더가 큐브+플레이어를 모두 닿고 있는 동안은 TakeDamage X, 큐브는 벗어났는데 플레이어는 벗어나지 않은 경우 TakeDamage O
    // if 돌진공격 중인 경우에만
    // case 큐브 닿음 -> 플레이어 닿음 : TakeDamage X
    // case 큐브에서는 exit했는데 플레이어 닿은 상태 : TakeDamage O
    // case 플레이어 닿음 -> 큐브 닿음 : TakeDamage O

    private HashSet<DestructibleCube> cubes = new HashSet<DestructibleCube>();
    private HashSet<Actor> targetActors = new HashSet<Actor>();

    protected override void OnTriggerEnter(Collider other)
    {
        if (!this.enabled) { return; }

        var cube = other.GetComponent<DestructibleCube>();
        if (cube != null)
        {
            cubes.Add(cube);
            //Debug.Log($"{transform.root.name} : OnTriggerEnter : 큐브 갯수 : {cubes.Count}");
            return;
        }
        
        var actor = other.GetComponent<Actor>();
        if (actor != null)
        {
            targetActors.Add(actor);
            // cubes 내부에 null 항목이 있으면 제거
            cubes.RemoveWhere(c => c == null);
            if (0 < cubes.Count)
            { return; }
        }

        base.OnTriggerEnter(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!this.enabled) { return; }

        var cube = other.GetComponent<DestructibleCube>();
        if (cube != null)
        {
            cubes.Remove(cube);
            //Debug.Log($"{transform.root.name} : OnTriggerExit : 큐브 갯수 : {cubes.Count}");
            foreach (var a in targetActors)
            { WeaponCollisionEnterAction(a.damageReaction, other); }
            return;
        }

        var actor = other.GetComponent<Actor>();
        if (actor != null)
        { targetActors.Remove(actor); }
    }

    // 그냥 임시
    // 큐브랑 플레이어랑 동시에 처맞을 때 빗겨내려고
    protected void WeaponCollisionEnterAction(DamageReaction damageReaction, Collider other)
    {
        if (damageReaction.CompareTag(targetTag) && // 공격 대상 태그 일치 확인
            damageReaction != null &&               // 데미지 입히기 가능 확인
            hitTargets != null)
        { WeaponCollisionEnterAction(damageReaction); }
    }

    public override void NotUseWeapon()
    {
        base.NotUseWeapon();
        cubes.Clear();
        targetActors.Clear();
    }


#if UNITY_EDITOR

    // ===== 디버그 기즈모 =====
    void OnDrawGizmos()
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }

        // 콜라이더가 비활성화되어 있으면 표시 X
        if (weaponCollider != null && weaponCollider.enabled)
        {
            Gizmos.color = Color.red;

            // BoxCollider 렌더링
            if (weaponCollider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }

            // CapsuleCollider 렌더링
            else if (weaponCollider is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule);
            }
        }
    }

    // CapsuleCollider를 Gizmos로 그려주는 유틸리티 함수
    private void DrawWireCapsule(CapsuleCollider capsule)
    {
        // 캡슐의 매트릭스를 적용
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = capsule.transform.localToWorldMatrix;

        Vector3 center = capsule.center;
        float radius = capsule.radius;
        float height = capsule.height;
        int direction = capsule.direction; // 0=X, 1=Y, 2=Z

        // 캡슐의 시각적 길이 (구면 상하 제외한 부분)
        float sphereHeight = radius * 2f;
        float straightHeight = Mathf.Max(0f, height - sphereHeight);

        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        // 방향 보정
        switch (direction)
        {
            case 0: // X-axis
                up = capsule.transform.right;
                forward = capsule.transform.forward;
                right = capsule.transform.up;
                break;
            case 1: // Y-axis (기본)
                up = capsule.transform.up;
                forward = capsule.transform.forward;
                right = capsule.transform.right;
                break;
            case 2: // Z-axis
                up = capsule.transform.forward;
                forward = capsule.transform.up;
                right = capsule.transform.right;
                break;
        }

        Vector3 topSphere = center + (up * (straightHeight / 2f));
        Vector3 bottomSphere = center - (up * (straightHeight / 2f));

        // 구 두 개 그리기
        Gizmos.DrawWireSphere(topSphere, radius);
        Gizmos.DrawWireSphere(bottomSphere, radius);

        // 원기둥 라인 그리기
        Gizmos.DrawLine(bottomSphere + right * radius, topSphere + right * radius);
        Gizmos.DrawLine(bottomSphere - right * radius, topSphere - right * radius);
        Gizmos.DrawLine(bottomSphere + forward * radius, topSphere + forward * radius);
        Gizmos.DrawLine(bottomSphere - forward * radius, topSphere - forward * radius);

        Gizmos.matrix = oldMatrix;
    }
#endif

}
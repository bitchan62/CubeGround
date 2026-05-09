using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public class ActorWeapon2 : MonoBehaviour
{
    // 무기 주인
    protected Actor owner;
    public void SetWeaponOwner(Actor owner)
    { this.owner = owner; }

    // 무기 콜라이더
    protected Collider _weaponCollider;
    public Collider weaponCollider
    {
        get
        {
            if (_weaponCollider == null)
            {
                _weaponCollider = GetComponent<Collider>();
                if (_weaponCollider == null ) { Debug.LogError($"{owner.gameObject.name} : 무기 콜라이더 미존재"); }
                else { _weaponCollider.isTrigger = true; }
            }

            return _weaponCollider;
        }
    }


    // 데이터
    protected AttackData attackData;
    protected EffectData hitEffect;
    protected KnockBackData knockBackData;

    public virtual void SetData<T>(T data) where T : IData
    {
        if (data is AttackData attack)            { this.attackData = attack; }
        else if (data is EffectData effect)       { this.hitEffect = effect; }
        else if (data is KnockBackData knockBack) { this.knockBackData = knockBack; }
    }

    private void Awake()
    { isActivate = false; }


    // 히트 시 콜백
    public MyCallBacks whenHit { private set; get; } = new MyCallBacks();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isActivate) { return; }

        if (other.CompareTag(attackData.targetTag))
        {
            // --- 적중 위치에 hit Effect 생성 ---
            InstantHitEffect(other.transform.position);

            DamageReaction damageReaction = other.GetComponent<DamageReaction>();
            if (damageReaction != null)
            {
                // --- 최대 히트 횟수 확인 및 처리 ---
                if (IsCanMoreHit(damageReaction.gameObject))
                {
                    whenHit.Invoke();

                    // --- 넉백 적용 ---
                    damageReaction.KnockBack(knockBackData, owner.transform);
                    // --- 데미지 적용 ---
                    damageReaction.TakeDamage(attackData);
                }
            }
            
        }
    }


    // 이펙트 발생
    protected void InstantHitEffect(Vector3 otherPosition)
    {
        Vector3 effectPos = weaponCollider.ClosestPoint(otherPosition);
        hitEffect?.Instantiate(this.gameObject, effectPos, transform.rotation);
    }

    // 이펙트 발생
    protected void InstantHitEffect(Vector3 otherPosition, EffectData effectData)
    {
        Vector3 effectPos = weaponCollider.ClosestPoint(otherPosition);
        effectData?.Instantiate(this.gameObject, effectPos, transform.rotation);
    }


    // 공격의 판정 횟수 체크
    // 공격 대상(key) / 히트 횟수(value)
    protected Dictionary<GameObject, int> hitTargets = new Dictionary<GameObject, int>();
    protected bool IsCanMoreHit(GameObject target)
    {
        // --- 적중 횟수 확인 ---
        int hitCount = 0;
        hitTargets.TryGetValue(target, out hitCount);

        // --- 최대 적중 횟수를 넘었는지 확인 ---
        bool isCanMoreHit = hitCount < attackData.maxHitCount;

        // --- 히트 횟수 +1 ---
        hitTargets[target] = hitCount + 1; // hitCount += 1

        return isCanMoreHit;
    }

    public bool isActivate
    {
        get { return this.enabled; }
        set
        {
            this.enabled = value;
            weaponCollider.enabled = value;
        }
    }

    protected virtual void OnDisable()
    {
        attackData = null;
        knockBackData = null;
        hitEffect = null;
        hitTargets.Clear();
    }





    // --- 디버그용 ---
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (weaponCollider != null && weaponCollider.enabled)
        {
            Gizmos.color = Color.red;

            BoxCollider boxCollider = weaponCollider as BoxCollider;
            if (boxCollider != null)
            {
                Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }

            SphereCollider sphereCollider = weaponCollider as SphereCollider;
            if (sphereCollider != null)
            {
                Gizmos.matrix = sphereCollider.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }

            CapsuleCollider capsuleCollider = weaponCollider as CapsuleCollider; // 여기 수정
            if (capsuleCollider != null)
            {
                Gizmos.matrix = capsuleCollider.transform.localToWorldMatrix;
                DrawWireCapsule(capsuleCollider.center, capsuleCollider.radius, capsuleCollider.height, capsuleCollider.direction);
            }
        }
    }


    void DrawWireCapsule(Vector3 center, float radius, float height, int direction)
    {
        // 캡슐의 축 방향에 따라 축 설정
        Vector3 up = Vector3.up;
        switch (direction)
        {
            case 0: up = Vector3.right; break; // X축
            case 1: up = Vector3.up; break;    // Y축
            case 2: up = Vector3.forward; break; // Z축
        }

        float cylinderHeight = Mathf.Max(0, height - 2 * radius);
        Vector3 offset = up * cylinderHeight * 0.5f;

        // 위 아래 반구 위치
        Vector3 topSphere = center + offset;
        Vector3 bottomSphere = center - offset;

        // 원통 몸통 그리기: 위, 아래 원 연결하는 직선 4개
        int segments = 16;
        float angleStep = 360f / segments;

        // 반지름 방향 벡터 두 개 선택 (up과 직교하는 두 벡터)
        Vector3 axis1, axis2;
        if (up == Vector3.up)
        {
            axis1 = Vector3.right;
            axis2 = Vector3.forward;
        }
        else if (up == Vector3.right)
        {
            axis1 = Vector3.up;
            axis2 = Vector3.forward;
        }
        else
        {
            axis1 = Vector3.up;
            axis2 = Vector3.right;
        }

        for (int i = 0; i < segments; i++)
        {
            float angle0 = Mathf.Deg2Rad * i * angleStep;
            float angle1 = Mathf.Deg2Rad * ((i + 1) % segments) * angleStep;

            Vector3 pointTop0 = topSphere + radius * (Mathf.Cos(angle0) * axis1 + Mathf.Sin(angle0) * axis2);
            Vector3 pointTop1 = topSphere + radius * (Mathf.Cos(angle1) * axis1 + Mathf.Sin(angle1) * axis2);
            Vector3 pointBottom0 = bottomSphere + radius * (Mathf.Cos(angle0) * axis1 + Mathf.Sin(angle0) * axis2);
            Vector3 pointBottom1 = bottomSphere + radius * (Mathf.Cos(angle1) * axis1 + Mathf.Sin(angle1) * axis2);

            // 원 그리기
            Gizmos.DrawLine(pointTop0, pointTop1);
            Gizmos.DrawLine(pointBottom0, pointBottom1);

            // 몸통 사방 연결선
            Gizmos.DrawLine(pointTop0, pointBottom0);
        }

        // 위 아래 반구 그리기 (단순화 위해 Sphere 사용)
        Gizmos.DrawWireSphere(topSphere, radius);
        Gizmos.DrawWireSphere(bottomSphere, radius);
    }
#endif
}

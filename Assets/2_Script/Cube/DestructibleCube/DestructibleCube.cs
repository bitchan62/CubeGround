using UnityEngine;

public class DestructibleCube : MonoBehaviour
{
    [Header("파괴 설정")]
    [Tooltip("파괴할 수 있는 오브젝트 태그")]
    public string destroyerTag = "Monster";

    [HideInInspector] public Boss boss;
    private BossChargeAttack chargeAttack;
    private bool isDestroyed = false; // 중복 실행 방지 플래그

    [Header("이펙트 설정")]
    [Tooltip("파괴 시 재생할 이펙트 프리팹")]
    public GameObject destroyEffect;

    /// <summary>
    /// 외부에서 보스를 설정하는 메서드 (Boss.cs에서 호출)
    /// </summary>
    public void SetBoss(Boss bossComponent)
    {
        this.boss = bossComponent;
        ConnectToBoss();
        Debug.Log($"{gameObject.name}: {bossComponent.name} 보스가 설정되었습니다.");
    }

    private void ConnectToBoss()
    {
        if (boss != null)
        {
            chargeAttack = boss.GetComponent<BossChargeAttack>();
            if (chargeAttack != null)
            {
                // 중복 등록 방지
                chargeAttack.stunEvent.Remove(CheckDestruction);
                chargeAttack.stunEvent.Add(CheckDestruction);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: 보스에서 BossChargeAttack 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 정리
        if (chargeAttack != null)
        {
            chargeAttack.stunEvent.Remove(CheckDestruction);
        }
    }

    private void CheckDestruction()
    {
        // 이미 파괴 처리 중이거나 오브젝트가 없으면 리턴
        if (isDestroyed || this == null || gameObject == null) return;

        // <- 약간 옆으로 빗나가도 파괴될 수 있도록 직접 방향 지정
        Vector3 layDirection = (this.transform.position - boss.transform.position).normalized;

        RaycastHit hit;
        bool isHit = Physics.Raycast(
            boss.transform.position + Vector3.up * 3f,
            layDirection,
            //boss.transform.forward,
            out hit,
            5f);

        if (isHit && (hit.transform == this.transform || hit.transform.IsChildOf(this.transform)))
        {
            // 중복 실행 방지
            isDestroyed = true;

            // 이펙트 생성
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            // 이벤트에서 자신을 제거 (Destroy 전에 먼저 실행)
            if (chargeAttack != null)
            {
                chargeAttack.stunEvent.Remove(CheckDestruction);
            }

            // 오브젝트 파괴
            Destroy(gameObject);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// MonsterCube 의존성 제거한 완전 독립형 올킬 트리거
/// </summary>
public class AllKillTrigger : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = false;

    // 내부 상태 변수들
    private bool isPlayerInArea = false;
    private bool canCheckKillAll = false;
    private bool isCompleted = false;
    private Collider triggerCollider;

    // 스포너와 몬스터 독립 관리
    private List<MonsterSpawner> targetSpawners = new List<MonsterSpawner>();
    private List<Monster> areaMonsters = new List<Monster>();

    // 주기적 체크용 변수
    private float lastSpawnerCheckTime = 0f;
    private const float SPAWNER_CHECK_INTERVAL = 1f;

    // 외부 접근용 프로퍼티
    public bool IsCompleted => isCompleted;
    public bool IsPlayerInArea => isPlayerInArea;
    public bool CanCheckKillAll => canCheckKillAll;
    public int TargetSpawnerCount => targetSpawners.Count;
    public int AreaMonsterCount => areaMonsters.Count;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] AllKillTrigger에 Collider가 필요");
            this.enabled = false;
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] Collider가 Trigger로 설정되지 않음");
        }
    }

    void Update()
    {
        // === MonsterSpawner 체크 ===
        if (Time.time - lastSpawnerCheckTime >= SPAWNER_CHECK_INTERVAL)
        {
            CheckForNewMonsterSpawners();
            lastSpawnerCheckTime = Time.time;
        }

        // === 스포너 완료 체크 ===
        if (!canCheckKillAll && AllSpawnersCompleted())
        {
            canCheckKillAll = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 모든 스포너 완료! 이제 몬스터 처치 대기 중... (현재 {areaMonsters.Count}마리)");

            // 스포너 완료 후 즉시 몬스터 체크
            CheckAreaKillCondition();
        }
    }

    /// <summary>
    /// 새로운 MonsterSpawner 감지
    /// </summary>
    private void CheckForNewMonsterSpawners()
    {
        Vector3 center = triggerCollider.bounds.center;
        Vector3 halfExtents = triggerCollider.bounds.size / 2;
        Collider[] allColliders = Physics.OverlapBox(center, halfExtents, transform.rotation);

        foreach (var col in allColliders)
        {
            // MonsterSpawner 직접 체크만 사용
            MonsterSpawner spawner = FindMonsterSpawner(col);
            if (spawner != null && !targetSpawners.Contains(spawner))
            {
                targetSpawners.Add(spawner);

                // 새로운 스포너가 추가되면 올킬 상태 리셋
                if (isCompleted)
                {
                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 새로운 스포너 추가로 올킬 상태 리셋: {spawner.name}");

                    isCompleted = false;
                    canCheckKillAll = false;
                }

                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 새로운 스포너 감지: {spawner.name}");
            }
        }
    }

    /// <summary>
    /// 영역별 올킬 조건 체크 - 핵심 로직
    /// </summary>
    private void CheckAreaKillCondition()
    {
        if (isCompleted)
            return;

        areaMonsters.RemoveAll(m => m == null);

        // 1단계: 스포너가 모두 완료되었는지 확인
        if (!canCheckKillAll)
        {
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 아직 스포너가 완료되지 않음");
            return;
        }

        // 2단계: 영역 내 몬스터가 모두 제거되었는지 확인
        if (areaMonsters.Count == 0)
        {
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 스포너 완료 + 몬스터 전멸 → 올킬 달성!");

            CompleteKillAll();
        }
        else
        {
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 스포너는 완료됐지만 몬스터 {areaMonsters.Count}마리 남음");
        }
    }

    // === 트리거 이벤트 처리 ===
    void OnTriggerEnter(Collider other)
    {
        // 플레이어 진입
        if (other.CompareTag(playerTag))
        {
            isPlayerInArea = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 진입!");
        }

        // 몬스터 진입 - 영역 내 몬스터 추적
        else if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null && !areaMonsters.Contains(monster))
            {
                areaMonsters.Add(monster);

                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 몬스터 진입: {monster.name} (총 {areaMonsters.Count}마리)");

                // 몬스터 사망 이벤트 구독
                var damageReaction = monster.GetComponent<DamageReaction>();
                if (damageReaction != null)
                {
                    damageReaction.whenDie.Add(() => OnAreaMonsterDied(monster), 1);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInArea = false;
        }
        else if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null && areaMonsters.Contains(monster))
            {
                areaMonsters.Remove(monster);

                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 몬스터 퇴장: {monster.name} (남은 몬스터: {areaMonsters.Count})");
            }
        }
    }

    /// <summary>
    /// 영역 내 몬스터가 죽었을 때 호출
    /// </summary>
    private void OnAreaMonsterDied(Monster monster)
    {
        // null 체크 추가
        if (monster == null)
        {
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] null 몬스터 사망 이벤트 수신");

            areaMonsters.RemoveAll(m => m == null);
        }
        else if (areaMonsters.Contains(monster))
        {
            areaMonsters.Remove(monster);

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 몬스터 사망: {monster.name} (남은 몬스터: {areaMonsters.Count})");
        }

        CheckAreaKillCondition();
    }

    // === 유틸리티 메서드들 ===

    /// <summary>
    /// 콜라이더에서 MonsterSpawner 찾기 (부모/자식 포함)
    /// </summary>
    private MonsterSpawner FindMonsterSpawner(Collider col)
    {
        // 1. 자기 자신에서 찾기
        MonsterSpawner spawner = col.GetComponent<MonsterSpawner>();
        if (spawner != null) return spawner;

        // 2. 부모에서 찾기
        spawner = col.GetComponentInParent<MonsterSpawner>();
        if (spawner != null) return spawner;

        // 3. 자식에서 찾기
        spawner = col.GetComponentInChildren<MonsterSpawner>();
        return spawner;
    }

    private bool AllSpawnersCompleted()
    {
        if (targetSpawners.Count == 0)
            return false;

        foreach (var spawner in targetSpawners)
        {
            if (spawner == null) continue;
            if (!spawner.IsSpawnerCompleted)
                return false;
        }
        return true;
    }

    private void CompleteKillAll()
    {
        if (isCompleted) return;
        isCompleted = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}]  올킬 달성!");

    }

    public void ResetAllKillTrigger()
    {
        isPlayerInArea = false;
        canCheckKillAll = false;
        isCompleted = false;
        targetSpawners.Clear();
        areaMonsters.Clear();
        this.enabled = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] AllKillTrigger 리셋 완료");
    }
}
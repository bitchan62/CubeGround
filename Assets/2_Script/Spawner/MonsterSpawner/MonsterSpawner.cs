using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MonsterSpawner : Spawner
{
    // 생성 주기
    [SerializeField] protected float spawnRate = 2f;
    // 시작과 함께 트리거 작동시킬지 설정
    [SerializeField] bool startTrigger = false;
    // 끝없이 스폰시킬지 설정
    [SerializeField] bool isEndlessSpawn = false;

    [Tooltip("이 스포너에서 생성된 몬스터를 처치해야 다음 진행")]
    [SerializeField] bool isClearCount = false;

    // 초기화
    protected void Start()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            Debug.Log("콜라이더 존재하지 않음 : " + gameObject.name);
            return;
        }

        if (startTrigger)
        { SpawnTriggerOn(); }
    }
    // ===== 스폰 위치 =====
    // 현재 오브젝트의 콜라이더
    protected Collider myCollider;
    // 윗면 중앙 계산 (하위 콜라이더들 포함)
    protected override void SetSpawnLocation()
    {
        // 하위 콜라이더들을 모두 포함해서 윗면 정중앙 계산
        Bounds combinedBounds = GetCombinedBoundsFromChildren();
        Vector3 topCenter = combinedBounds.center + Vector3.up * combinedBounds.extents.y;
        // 추가 높이 오프셋 적용
        spawnLocation = topCenter;

        //Debug.Log($"{name} : spawnLocation : {spawnLocation}");
    }

    // 하위 오브젝트들의 모든 콜라이더 범위를 합치기
    private Bounds GetCombinedBoundsFromChildren()
    {
        // 모든 하위 콜라이더 가져오기 (자기 자신 포함)
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        if (allColliders == null || allColliders.Length == 0)
        {
            // 콜라이더가 없으면 Transform 기준으로 기본 크기 사용
            Debug.Log($"[{gameObject.name}] 하위 콜라이더를 찾을 수 없습니다. Transform 크기를 사용합니다.");
            return new Bounds(transform.position, transform.lossyScale);
        }
        // 첫 번째 콜라이더로 초기 범위 설정
        Bounds combinedBounds = allColliders[0].bounds;
        // 나머지 콜라이더들 범위 모두 합치기
        for (int i = 1; i < allColliders.Length; i++)
        { combinedBounds.Encapsulate(allColliders[i].bounds); }
        return combinedBounds;
    }

    // ===== 트리거 / 생성 / 완료 =====
    // 1. 스포너 활성화 (MonsterCube에서 호출)
    // 2. 스폰 위치 지정
    // 3. 생성 시작
    public override void SpawnTriggerOn()
    {
        // Debug.Log($"[{gameObject.attackName}] MonsterSpawner 활성화됨! 스폰을 시작합니다.");
        base.SpawnTriggerOn();
        SetSpawnLocation(); // 스폰 위치 재설정 (하위 콜라이더 기반)
        SpawnObject();
    }

    public GameObject SpawnTriggerOnAndGetSpawnObject()
    {
        base.SpawnTriggerOn();
        SetSpawnLocation(); // 스폰 위치 재설정 (하위 콜라이더 기반)
        return SpawnObject();
    }

    // 생성
    protected override GameObject SpawnObject()
    {
        // 스폰 트리거가 켜져있다면
        if (spawnTrigger)
        {
            // 몬스터 스폰 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMonsterSpawn();
            }

            // 오브젝트 생성
            // Debug.Log(PrefabIndex + "번째 몬스터 생성");
            GameObject obj = base.SpawnObject();
            if(isClearCount)
            {
                // 클리어 트리거 대상자라면 등록
                var target = obj.GetComponent<IClearTrigger>();
                if (target != null)
                { ClearTriggerListManager.Instance.Add(target); }
            }

            // 종료 체크
            CheckCompleted();
            // 종료되지 않았다면 : 다음 스폰 예약
            if (!isCompleted) { Timer.Instance.StartTimer(this, spawnRate, () => SpawnObject()); }
            return obj;
        }

        return null;
    }

    // 종료 확인
    public override void CheckCompleted()
    {
        // 모든 프리펩을 생성했다면
        if (targetPrefabs.Count <= PrefabIndex + 1)
        {
            base.CheckCompleted();
            SpawnTriggerOFF(); 
            this.enabled = false;

            // 주기적 스포너라면: 리셋 발생
            if (isEndlessSpawn)
            {
                ResetSpawner();
            }
        }
        else
        {
            // 다음 프리펩 인덱스 지정
            PrefabIndex += 1;
        }
    }

    // AllKillTriger
    public bool IsSpawnerCompleted
    {
        get { return isCompleted || !this.enabled; } // 비활성화되어도 완료로 판단
    }


}
using System;
using System.Collections.Generic;
// using Unity.VisualScripting;
using UnityEngine;


//====================
// 다양한 스폰 조건과 방식을 구현할 수 있는 추상 클래스
// 자식 클래스에서 SpawnTrigger()를 오버라이드하여 스폰 조건 구현
//====================
abstract public class Spawner : MonoBehaviour
{
    // 지정한 오브젝트 (생성할 프리팹들과 위치 지정)
    [SerializeField] protected List<GameObject> targetPrefabs = new List<GameObject>(); // 순차적으로 생성할 프리팹 목록

    // 생성할 프리팹의 인덱스 (몇 번째 프리팹인지)
    private int _prefabIndex = 0;
    public int PrefabIndex
    {
        get
        {
            return _prefabIndex % targetPrefabs.Count;
        }
        protected set
        {
            // null 검사 && 인덱스 검사
            if (targetPrefabs != null && 0 <= value)
            { _prefabIndex = value; }
        }
    }


    // ===== 생성 조건 충족 여부 =====

    // 스폰 조건 만족 여부 (true로 만들면 == 생성)
    protected bool spawnTrigger = false;

    // 생성 조건 만족 / 트리거 켜기
    public virtual void SpawnTriggerOn()
    { spawnTrigger = true; }

    // 생성 트리거 끄기
    public virtual void SpawnTriggerOFF()
    { spawnTrigger = false; }



    // ===== 완료 / 재활성 =====

    // 스포너 완료 상태 확인
    protected bool isCompleted = false;

    // 스포너 완료 조건
    public virtual void CheckCompleted()
    { isCompleted = true; }

    // 스포너 초기화 (재활성화)
    public virtual void ResetSpawner()
    {
        PrefabIndex = 0;
        isCompleted = false;
        spawnTrigger = true;
    }


    // ===== 생성 / 위치 지정 =====

    // 오브젝트를 생성할 위치
    protected Vector3 _spawnLocation;

    protected Vector3 spawnLocation
    {
        get
        { 
            // x, z축으로 약간의 난수값 추가
            float offsetX = UnityEngine.Random.Range(-0.2f, 0.2f);
            float offsetZ = UnityEngine.Random.Range(-0.2f, 0.2f);
            return new Vector3(
                _spawnLocation.x + offsetX,
                _spawnLocation.y,
                _spawnLocation.z + offsetZ
            );
        }
        set { _spawnLocation = value; }
    }

    // 오브젝트 생성
    // Instantiate 게임 오브젝트 반환값
    // 1. 싱글톤으로 몬스터 리스트를 만든다.
    // 2. Instantiate되는 게임오브젝트 몬스터를 싱글톤 몬스터 리스트에 넣는다.
    // 3. 싱글톤 몬스터 리스트가 카운트가 0이 되면 몬스터가 더 이상 없음. 
    // 4. 주의점 : 몬스터들이 디스트로이로 사라져서 몬스터 리스트에 null이 쌓일거임 
    protected virtual GameObject SpawnObject()
    {
        if (targetPrefabs.Count <= 0) { Debug.Log("스포너 프리펩 인덱스 비어있음"); return null; }

        // 뭐 스폰되는지 체크
        //Debug.Log($"{transform.root.name} : {name} : {targetPrefabs[PrefabIndex].name} : Spawn");

        // 현재 인덱스의 프리팹, 지정된 위치, 기본 회전값으로 생성
        // ObjectPool 적용 중 (테스트)
        //GameObject monster = PoolManager.GetObject(targetPrefabs[PrefabIndex]);
        //monster.transform.position = spawnLocation;
        return Instantiate(targetPrefabs[PrefabIndex], spawnLocation, Quaternion.identity);
    }

    // [아쳐] [메이지] [미] [미]
    // [null] [메이지] [미] [미]
    // [null] [null] [null] [null] -> List.Count는 이걸 몇으로 판정한다? 4로 판정한다

    // 스폰 위치 지정
    protected virtual void SetSpawnLocation()
    { spawnLocation = transform.position; }
}
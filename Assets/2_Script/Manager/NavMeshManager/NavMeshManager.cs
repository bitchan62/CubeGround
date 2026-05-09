using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshManager : MonoBehaviour
{
    [SerializeField]
    [Header("네비메시 베이크 범위 (플레이어 중심)")]
    private Vector3 updateBoundsSize = new Vector3(50f, 20f, 50f);

    [SerializeField]
    [Header("플레이어 중심 자동 bake를 할지 여부")]
    private bool isAutoUpdate = true;


    // 사용할 Agent Type의 ID를 설정 (기본값은 0, Humanoid)
    private int agentTypeID = 0;

    // 싱글톤
    public static NavMeshManager instance = null;
    // 네비게이션
    private NavMeshSurface surface = null;
    private void Awake()
    {
        // 싱글톤 초기화
        if (instance == null)
        { instance = this; }
        else { Destroy(this.gameObject); return; }

        // NavMeshSurface 설정
        surface = GetComponent<NavMeshSurface>();
        surface.agentTypeID = agentTypeID; // Agent Type 지정
        surface.collectObjects = CollectObjects.Children; // 자식 오브젝트만 맵 생성
        surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders; // 콜라이더 기반 맵 생성
        surface.layerMask = LayerMask.GetMask("Cube"); // Cube 레이어만 맵 생성

        surface.overrideVoxelSize = true;
        surface.voxelSize = 0.3f;
    }

    // private bool isRebuildResently = false;


    private void Start()
    {
        BuildFull();
        navMeshData = surface.navMeshData;
    }

    // 지형 갱신
    public void BuildFull()
    {
        //if (isRebuildResently) { return; }
        surface.BuildNavMesh();
        //isRebuildResently = true;
        //Timer.Instance.StartTimer(this, "", 1f, () => isRebuildResently = false);
    }


    // NavMeshData를 직접 관리
    private NavMeshData navMeshData = null;

    // 부분 bake를 위한 변수
    private List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();



    // 비동기 방식 프레임 드롭 최소화
    public void RebuildPartialAsync(Vector3 centerPosition)
    {
        if (surface == null) { Debug.Log("navMesh Surface null"); return; }
        if (!isAutoUpdate) { return; }


        Bounds updateBounds = new Bounds(centerPosition, updateBoundsSize);

        NavMeshBuilder.CollectSources(
            updateBounds,
            surface.layerMask,
            NavMeshCollectGeometry.PhysicsColliders,
            0,
            new List<NavMeshBuildMarkup>(),
            buildSources
        );

        if (navMeshData != null && buildSources.Count > 0)
        {
            // 비동기 업데이트 프레임 끝에서 처리됨
            AsyncOperation operation = NavMeshBuilder.UpdateNavMeshDataAsync(
                navMeshData,
                surface.GetBuildSettings(),
                buildSources,
                updateBounds
            );
        }
    }

}
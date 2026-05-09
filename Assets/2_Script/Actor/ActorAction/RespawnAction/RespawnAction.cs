using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RespawnAction : ActorAction
{
    protected override void Awake()
    {
        base.Awake();

        // 큐브 레이어만 타겟
        layerMask = LayerMask.GetMask("Cube");
    }

    // 방문한 큐브
    private HashSet<Collider> visitedCubes = new HashSet<Collider>();

    // 검사할 레이어
    protected int layerMask;


    // 안전한 위치 탐색 및 복귀
    // CubeCollapser 탐지 -> 콜라이더 확인
    protected bool TryReturnToSafePos(float searchRadius = 10f)
    {
        Vector3 centerPos = thisActor.lastestRandedPos;

        // 탐색한 큐브들
        Collider[] cubes;

        // --- 큐브 탐색 ---
        if (0 < searchRadius) // <- "Cube" 레이어 + 콜라이더 보유 Object를 수집함
        { cubes = Physics.OverlapSphere(centerPos, searchRadius, layerMask); }
        else
        {
            // 무제한 탐색: 씬 내 모든 CollapseWatcher 컴포넌트 찾기 및 그 관련 콜라이더 수집
            CollapseWatcher[] allCollapsers = Object.FindObjectsOfType<CollapseWatcher>();
            List<Collider> cubeList = new List<Collider>();
            foreach (var collapser in allCollapsers)
            {
                Collider col = collapser.GetComponent<Collider>();
                if (col != null) { cubeList.Add(col); }
            }
            cubes = cubeList.ToArray();
        }

        // --- 적절한 큐브 탐색 ---
        var newCubes = cubes.Where(c => !visitedCubes.Contains(c))
                            .OrderBy(c => Vector3.Distance(centerPos, c.transform.position))
                            .ToList();

        Collider[] hits = new Collider[1];
        foreach (Collider cube in newCubes)
        {
            visitedCubes.Add(cube);   // 방문 처리

            CollapseWatcher collapser = cube.GetComponent<CollapseWatcher>(); // <- 수집한 Object에 CollapseWatcher 보유 확인
            if (collapser != null && collapser.IsSafe)
            {
                Vector3 checkPos = collapser.transform.position + new Vector3(0, 5, 0);
#if UNITY_EDITOR
                // 기즈모를 그리기 위해 현재 검사 위치를 저장합니다.
                gizmoDrawPos = checkPos;
#endif

                int checkLayer = LayerMask.GetMask("Cube", "Monster", "CanNotThrough");
                int num = Physics.OverlapBoxNonAlloc(checkPos, new Vector3(3, 3, 3) * 0.5f, hits, Quaternion.identity, checkLayer);
                //int num = Physics.OverlapSphereNonAlloc(checkPos, 1f, hits, checkLayer);
                if (num == 0)
                {
                    // 위치 변경
                    this.transform.position = checkPos;
                    // 벡터값 zero
                    thisActor.rigid.velocity = Vector3.zero;
                    // 방문 큐브 클리어
                    visitedCubes.Clear();

                    return true; // 복귀 성공
                }
            }
        }

        // 복귀 실패
        return false;
    }


    public void ReturnToSafePos()
    {
        float radius = 10f;
        float maxDistance = 1000f;

        while (radius <= maxDistance)
        {
            // 탐색 성공
            if (TryReturnToSafePos(radius))
            { break; }

            // 탐색 실패 시
            else if (maxDistance <= radius)
            {
                // 전체 맵 탐색
                TryReturnToSafePos(0);
                break;
            }

            // 재탐색
            else
            {
                radius *= 2;
                if (maxDistance < radius) { radius = maxDistance; }
            }
        }
    }



#if UNITY_EDITOR
    // 기즈모를 그릴 위치를 저장하기 위한 변수
    private Vector3? gizmoDrawPos = null;

    /// <summary>
    /// 유니티 에디터의 씬 뷰에 기즈모를 그립니다.
    /// 이 메서드는 게임 실행 중에만 동작하며, OverlapBox의 범위를 시각적으로 표시합니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        // gizmoDrawPos에 위치가 지정되었을 때만 기즈모를 그립니다.
        if (gizmoDrawPos.HasValue)
        {
            // 기즈모의 색상을 빨간색으로 설정합니다.
            Gizmos.color = Color.red;

            // OverlapBox와 동일한 위치와 크기로 와이어프레임 박스를 그립니다.
            // Physics.OverlapBoxNonAlloc는 halfExtents(절반 크기)를 사용하지만,
            // Gizmos.DrawWireCube는 size(전체 크기)를 사용합니다.
            // 따라서 halfExtents에 2를 곱한 값을 넣어줍니다. 여기서는 Vector3(3, 3, 3)입니다.
            Gizmos.DrawWireCube(gizmoDrawPos.Value, new Vector3(3, 3, 3));
        }
    }
#endif
}



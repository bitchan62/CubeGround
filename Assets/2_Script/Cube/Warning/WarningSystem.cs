using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

/// <summary>
/// 위에서 아래로 떨어지는 큐브가 그 아래에 있는 큐브의 윗면에 빨간색 경고 표시를 보여주는 스크립트
/// 큐브가 가까워질수록 경고 표시가 더 선명해지고, 착지 직전에 부드럽게 사라짐.
/// 오브젝트 풀링으로 성능 최적화
/// Default와 Cube 레이어 모두 감지하도록 레이어마스크 수정
/// </summary>
public class WarningSystem : MonoBehaviour
{
    // 경고 표시 관련
    private GameObject _warningPlane;       // 현재 사용 중인 경고 표시 평면
    private GameObject warningPlane
    {
        get
        {
            // 할당된 경고발판이 없음 || 씬 상에서 비활성화 상태
            if (_warningPlane == null || !_warningPlane.activeInHierarchy)
            { _warningPlane = WarningPlanePool.Instance?.GetWarningPlaneFromPool(); }

            return _warningPlane;
        }
        set
        {
            if (value == null)
            {
                WarningPlanePool.Instance?.ReturnWarningPlaneToPool(_warningPlane);
                _warningPlane = null;
            }
        }
    }

    private float totalDistance;     // 시작 위치에서 목표까지 총 거리
    private Vector3 targetPosition;  // 큐브가 착지할 목표 위치
    private CubeMover cubeMover;     // 정지/이동 상태 체크

    // ==================== Unity 생명주기 메서드들 ====================


    void Start()
    {
        cubeMover = GetComponent<CubeMover>();
        totalDistance = cubeMover.startPositionOffset.magnitude;
        //  SetupWarningMaterial();
    }

    void Update()
    {
        // 일시정지 시 Update 중단
        if (Time.timeScale == 0f) return;

        // 현재 위치에서 목적지까지 남은 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 큐브가 움직이고 있는 동안 처리
        if (cubeMover.IsCurrentlyMoving)
        {
            CheckUnderCube();
            WarningPlaneCustom.Instance.UpdateColor(warningPlane, distanceToTarget / totalDistance);
        }
        // 움직이지 않게 되면: 제거
        else
        {
            this.enabled = false;
            RemoveWarning();
        }
    }
    // ==================== 경고 표시 생성 및 관리 ====================


    private void CheckUnderCube()
    {
        // Default와 Cube 레이어 모두 감지하도록 레이어마스크 확장
        //int defaultLayer = 1 << LayerMask.NameToLayer("Default");
        int cubeLayer = 1 << LayerMask.NameToLayer("Cube");
        int layerMask = cubeLayer;

        RaycastHit rayHit;
        bool isRayHit = Physics.Raycast(transform.position, Vector3.down, out rayHit, 100f, layerMask);

        if (isRayHit)
        {
            //Debug.Log("레이 히트 성공");
            // Vector3 hitPosition = rayHit.point;   // 충돌한 위치(월드 좌표)
            Renderer hitRenderer = rayHit.collider.GetComponentInChildren<Renderer>(); // <- 충돌한 개체가 직접 렌더러를 들고 있지 않을 수 있음
            if (hitRenderer != null)
            {
                //Debug.Log("렌더러 존재, 경고 발판 생성");
                CreateWarningPlane(rayHit, hitRenderer);
            }

            targetPosition = rayHit.point;
        }
    }


    /// <summary>
    /// 실제 경고 표시(빨간 평면)를 생성하고 설정하는 메서드
    /// 오브젝트 풀링을 사용해서 기존 발판 재사용
    /// </summary>
    private void CreateWarningPlane(RaycastHit hit, Renderer targetRenderer)
    {
        // 경고 표시 활성화
        warningPlane.SetActive(true);

        // 경고 표시가 생성될 위치 계산 (아래 큐브 윗면에 살짝 위)
        float targetTopY = targetRenderer.bounds.center.y + targetRenderer.bounds.extents.y;
        Vector3 planePosition = new Vector3(
            hit.point.x,                // 레이캐스트 충돌 지점 X
            targetTopY + 0.005f,        // 아래 큐브 윗면 + 5mm (겹침 방지)
            hit.point.z                 // 레이캐스트 충돌 지점 Z
        );

        // 경고 표시 위치
        warningPlane.transform.position = planePosition;

        // 경고 표시 크기를 아래 큐브 크기에 맞게 조정
        float planeSize = targetRenderer.bounds.extents.x * 2;  // 큐브 너비
        WarningPlaneCustom.Instance.UpdateSize(warningPlane, planeSize, planeSize);
    }


    // ==================== 정리 및 안전장치 메서드들 ====================

    /// <summary>
    /// 경고 표시 제거 및 풀로 반환
    /// 메모리 누수 방지를 위한 핵심 정리 메서드
    /// </summary>
    private void RemoveWarning()
    {
        if (!gameObject.scene.isLoaded) { return; }

        if (WarningPlanePool.Instance != null)
        {
            // 파괴 대신 풀로 반환 (재사용)
            warningPlane = null;
        }
    }

    /// <summary>
    /// Unity 이벤트: 오브젝트가 비활성화될 때 자동 호출
    /// 큐브가 SetActive(false)되거나 씬 전환 시 안전장치 역할
    /// </summary>
    void OnDisable()
    {
        RemoveWarning();  // 비활성화 시 경고 표시도 정리
    }

    /// <summary>
    /// Unity 이벤트: 오브젝트가 파괴될 때 자동 호출
    /// 게임 종료, 씬 전환, Destroy() 호출 시 메모리 누수 방지
    /// </summary>
    void OnDestroy()
    {
        RemoveWarning();  // 파괴 시 경고 표시도 정리 (메모리 누수 방지)
    }

    



}
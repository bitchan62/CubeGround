using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 멧돼지 큐브용 에리어 트리거
/// 플레이어 또는 큐브가 트리거 영역에 진입하면 연결된 멧돼지 큐브들을 활성화
/// CollapseTrigger와 동일한 방식으로 작동
/// </summary>
public class BoarTrigger : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("트리거 설정")]
    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Tooltip("큐브 태그")]
    public string cubeTag = "Cube";

    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = true;

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    #endregion

    #region ===== 내부 변수 =====

    // 상태 관리
    private bool hasTriggered = false;

    // 연결된 멧돼지 큐브들
    private List<BoarCube> connectedBoarCubes;

    #endregion

    #region ===== 초기화 =====

    private void Start()
    {
        FindConnectedBoarCubes();
    }

    /// <summary>
    /// 이 트리거 영역과 연결된 멧돼지 큐브들을 찾아서 저장
    /// </summary>
    private void FindConnectedBoarCubes()
    {
        // 씬의 모든 멧돼지 큐브 검색 (비활성화된 것도 포함)
        BoarCube[] allBoarCubes = FindObjectsOfType<BoarCube>(true);
        connectedBoarCubes = new List<BoarCube>();

        // 이 트리거 영역을 참조하는 AreaTrigger 모드 큐브들만 필터링
        foreach (BoarCube boarCube in allBoarCubes)
        {
            if (IsConnectedBoarCube(boarCube))
            {
                connectedBoarCubes.Add(boarCube);

                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 연결된 멧돼지 큐브 발견: {boarCube.gameObject.name}");
            }
        }

        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 총 {connectedBoarCubes.Count}개의 멧돼지 큐브와 연결됨");

            if (connectedBoarCubes.Count == 0)
                Debug.LogWarning($"[{gameObject.name}] 연결된 멧돼지 큐브가 없습니다!");
        }
    }

    /// <summary>
    /// 해당 멧돼지 큐브가 이 트리거와 연결되어 있는지 확인
    /// </summary>
    private bool IsConnectedBoarCube(BoarCube boarCube)
    {
        return boarCube != null &&
               boarCube.triggerArea == this.gameObject;
    }

    #endregion

    #region ===== 트리거 이벤트 =====

    /// <summary>
    /// 플레이어 또는 큐브가 트리거 영역에 진입했을 때 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 또는 큐브인지 확인
        if (!other.CompareTag(playerTag) && !other.CompareTag(cubeTag))
            return;

        // 이미 트리거되었고 한 번만 사용 설정이면 무시
        if (oneTimeUse && hasTriggered)
            return;

        hasTriggered = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 플레이어 감지! 멧돼지 큐브들 활성화 시작");

        ActivateConnectedBoarCubes();
    }

    /// <summary>
    /// 연결된 모든 멧돼지 큐브들을 활성화
    /// </summary>
    private void ActivateConnectedBoarCubes()
    {
        int activatedCount = 0;

        foreach (BoarCube boarCube in connectedBoarCubes)
        {
            if (boarCube == null)
                continue;

            activatedCount++;
            ActivateSingleBoarCube(boarCube);
        }

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] {activatedCount}개 멧돼지 큐브 활성화 처리 완료!");
    }

    /// <summary>
    /// 개별 멧돼지 큐브 활성화 처리
    /// </summary>
    private void ActivateSingleBoarCube(BoarCube boarCube)
    {
        // 고유 타이머 키 생성 (중복 방지)
        string uniqueKey = $"BoarActivate_{boarCube.gameObject.GetInstanceID()}_{Time.time}";

        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 타이머 시작: {boarCube.gameObject.name}, " +
                     $"딜레이: {boarCube.warningDelay}초");
        }

        // 워닝딜레이 후 큐브 활성화 및 발사
        Action activationAction = () => ActivateBoarCubeDelayed(boarCube);
        Timer.Instance.StartTimer(this, uniqueKey, boarCube.warningDelay, activationAction);
    }

    /// <summary>
    /// 워닝딜레이 후 호출되는 큐브 활성화 메서드
    /// </summary>
    private void ActivateBoarCubeDelayed(BoarCube boarCube)
    {
        if (boarCube == null)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] 멧돼지 큐브가 null입니다 (이미 파괴됨?)");
            return;
        }

        // 큐브가 비활성화되어 있다면 활성화
        if (!boarCube.gameObject.activeInHierarchy)
        {
            boarCube.gameObject.SetActive(true);

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 워닝딜레이 완료 - " +
                         $"멧돼지 큐브 [{boarCube.gameObject.name}] 활성화 후 발사");
        }

        // 발사 트리거
        boarCube.TriggerLaunch();
    }

    #endregion

    #region ===== 공개 메서드 =====

    /// <summary>
    /// 트리거 상태 리셋 (재사용을 위해)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 트리거 상태 리셋 완료");
    }

    /// <summary>
    /// 수동으로 멧돼지 큐브들 활성화 (테스트용)
    /// </summary>
    [ContextMenu("수동 트리거 실행")]
    public void ManualTrigger()
    {
        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 수동 트리거 실행");

        hasTriggered = false; // 강제로 리셋
        ActivateConnectedBoarCubes();
    }

    #endregion

    #region ===== 정보 조회 =====

    /// <summary>
    /// 연결된 멧돼지 큐브 개수 반환
    /// </summary>
    public int GetConnectedBoarCubeCount()
    {
        return connectedBoarCubes?.Count ?? 0;
    }

    /// <summary>
    /// 트리거 상태 반환
    /// </summary>
    public bool HasTriggered
    {
        get { return hasTriggered; }
    }

    #endregion
}
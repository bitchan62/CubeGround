using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 멧돼지 큐브 - 메인 컴포넌트 (MeshRenderer 제어 방식)
/// 현재 위치에서 startPositionOffset 방향으로 돌진
/// 워닝 시작과 동시에 큐브가 보이기 시작함
/// </summary>
[RequireComponent(typeof(BoarMovement))]
[RequireComponent(typeof(BoarWarning))]
[RequireComponent(typeof(BoarKnockback))]
public class BoarCube : MonoBehaviour
{
    #region ===== 트리거 설정 =====

    [Header("트리거 설정")]
    [Tooltip("발사 전 대기 시간 (초) - 워닝딜레이")]
    public float warningDelay = 1f;

    [Tooltip("트리거 영역 오브젝트")]
    public GameObject triggerArea;

    #endregion

    #region ===== 이동 및 공격 설정 =====

    [Header("이동 설정")]
    [Tooltip("돌진 방향 오프셋 (현재 위치에서 이 방향으로 돌진)")]
    public Vector3 startPositionOffset = new Vector3(10, 0, 0);

    [Tooltip("돌진 속도")]
    public float launchSpeed = 8f;

    [Tooltip("발사 완료 후 비활성화 대기 시간")]
    public float deactivateDelay = 0.5f;

    [Header("공격 범위")]
    [Tooltip("공격 폭 (0 = 큐브 크기와 동일)")]
    public float attackWidth = 0f;

    [Header("시각 효과")]
    [Tooltip("경고 표시 투명도")]
    public float warningAlpha = 0.7f;

    #endregion

    #region ===== 이펙트 데이터 =====
    [Header("발사 시 발사 위치에서 발생할 effect")]
    public EffectData lunchEffect = new EffectData();
    #endregion

    #region ===== 넉백 시스템 =====

    [Header("넉백 시스템")]
    [Tooltip("넉백 힘")]
    public float knockbackForce = 12f;

    [Tooltip("넉백 지속 시간")]
    public float knockbackDuration = 0.5f;

    [Tooltip("넉백 판정 반지름")]
    public float knockbackRadius = 0.3f;

    #endregion

    #region ===== 시각 제어 설정 =====

    [Header("시각 제어")]
    [Tooltip("시작 시 큐브를 숨길지 여부")]
    public bool startHidden = true;

    [Tooltip("워닝 시작과 동시에 큐브 표시")]
    public bool showOnWarningStart = true;

    #endregion

    #region ===== 디버그 설정 =====

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    [Header("에디터 시각화")]
    [Tooltip("씬에서 이동 경로 표시")]
    public bool showPathInScene = true;

    [Tooltip("경로 표시 색상")]
    public Color pathColor = Color.cyan;

    #endregion

    #region ===== 내부 변수 =====

    // 컴포넌트 참조
    private BoarMovement movement;
    private BoarWarning warning;
    private BoarKnockback knockback;

    // 상태 관리
    // private bool hasTriggered = false;  // 트리거 발동 여부
    private bool isLaunched = false;    // 발사 진행 여부
    private bool isVisible = true;      // 현재 표시 상태

    // 자식 큐브들 (그룹 발사용)
    private BoarCube[] childBoarCubes;

    #endregion

    #region ===== 초기화 =====

    void Awake()
    {
        InitializeComponents();
        DetermineChildCubes();

        // 시작 시 숨김 설정이 켜져있으면 MeshRenderer만 비활성화
        if (startHidden)
        {
            SetVisibility(false);
        }

        //knockbackRadius = 3f;
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        // 필수 컴포넌트 가져오기
        movement = GetComponent<BoarMovement>();
        warning = GetComponent<BoarWarning>();
        knockback = GetComponent<BoarKnockback>();

        // 각 컴포넌트 초기화
        movement.Initialize(this);
        warning.Initialize(this);
        knockback.Initialize(this);
    }

    /// <summary>
    /// 자식 큐브들 찾기
    /// </summary>
    private void DetermineChildCubes()
    {
        // 자식에 BoarCube가 있는지 확인
        childBoarCubes = GetComponentsInChildren<BoarCube>(true);

        // 자기 자신 제외
        var filteredChildren = new System.Collections.Generic.List<BoarCube>();
        foreach (var child in childBoarCubes)
        {
            if (child != this)
                filteredChildren.Add(child);
        }
        childBoarCubes = filteredChildren.ToArray();
    }

    #endregion

    #region ===== 시각 제어 메서드 =====

    /// <summary>
    /// 큐브의 시각적 표시 제어 (자식 포함)
    /// </summary>
    private void SetVisibility(bool visible)
    {
        // 상태가 같다면 중복 처리 방지
        if (isVisible == visible) return;

        isVisible = visible;

        // 자신의 MeshRenderer
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }

        // 자식들의 MeshRenderer도 모두 제어
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in childRenderers)
        {
            renderer.enabled = visible;
        }

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 큐브 표시: {(visible ? "ON" : "OFF")}");
    }

    /// <summary>
    /// 현재 표시 상태 반환
    /// </summary>
    public bool IsVisible
    {
        get { return isVisible; }
    }

    #endregion

    #region ===== 공개 메서드 - 외부 호출용 =====

    /// <summary>
    /// 에리어 트리거에서 호출하는 메서드
    /// </summary>
    public void TriggerLaunch()
    {
        Timer.Instance.StartTimer(this, "Launch", warningDelay, LaunchBoarCube);
    }

    public void StopLaunch()
    {
        Timer.Instance.StopTimer(this, "Launch");
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 시작 위치 오프셋 설정
    /// </summary>
    public void SetStartPositionOffset(Vector3 offset)
    {
        startPositionOffset = offset;
        movement.UpdatePositions(offset);
    }

    /// <summary>
    /// 멧돼지 큐브 완전 리셋
    /// </summary>
    public void ResetBoarCube()
    {
        StopAllCoroutines();
        Timer.Instance.StopTimer(this, "Launch");

        warning.ClearWarning();
        knockback.Reset();
        movement.Reset();

        isLaunched = false;

        // 시작 상태로 돌아가기 (숨김 여부에 따라)
        SetVisibility(!startHidden);

        // 자식 큐브들도 리셋
        if (HasChildCubes())
        {
            foreach (var child in childBoarCubes)
            {
                if (child != null)
                    child.ResetBoarCube();
            }
        }
    }

    // 보어큐브 종료 시 이벤트용
    public event Action ActionWhenEnd;

    #endregion

    #region ===== 내부 메서드 - 발사 로직 =====

    /// <summary>
    /// 발사 시작 (워닝딜레이 적용)
    /// </summary>
    // private void StartLaunch()
    // {
    //     if (hasTriggered && oneTimeUse) return;
    // 
    //     hasTriggered = true;
    //     Timer.Instance.StartTimer(this, "Launch", warningDelay, LaunchBoarCube);
    // }

    /// <summary>
    /// 실제 멧돼지 큐브 발사 (워닝딜레이 후 호출)
    /// </summary>
    private void LaunchBoarCube()
    {
        if (isLaunched) return;

        isLaunched = true;

        if (HasChildCubes())
        {
            StartCoroutine(GroupBoarCubeSequence());
        }
        else
        {
            StartCoroutine(BoarCubeSequence());
        }
    }

    #endregion

    #region ===== 코루틴 - 단일 발사 시퀀스 =====

    /// <summary>
    /// 단일 멧돼지 큐브 발사 시퀀스 (MeshRenderer 제어 방식)
    /// </summary>
    private System.Collections.IEnumerator BoarCubeSequence()
    {
        // 1단계: 큐브 표시 (워닝 시작과 동시에)
        if (showOnWarningStart && !isVisible)
        {
            SetVisibility(true);
        }

        lunchEffect.Instantiate(gameObject, transform.position, transform.rotation);

        // 2단계: 경고 표시
        warning.ShowWarning();

        // 3단계: 경고 시간 대기
        float warningDuration = warning.GetWarningDuration();
        yield return new WaitForSeconds(warningDuration);

        // 4단계: 경고 제거
        warning.ClearWarning();

        // 5단계: 돌진 실행 (현재 위치에서 startPositionOffset 방향으로)
        yield return StartCoroutine(movement.ExecuteLaunch());

        // 6단계: 완료 후 대기 및 숨김
        yield return new WaitForSeconds(deactivateDelay);
        ActionWhenEnd?.Invoke();
        // 다시 숨김 (재사용을 위해)
        SetVisibility(false);
    }

    /// <summary>
    /// 즉시 단일 발사 시퀀스
    /// </summary>
    /*
    private System.Collections.IEnumerator ImmediateLaunchSequence()
    {
        yield return StartCoroutine(movement.ExecuteLaunch());
        yield return new WaitForSeconds(deactivateDelay);
        SetVisibility(false);
    }
    */

    #endregion

    #region ===== 코루틴 - 그룹 발사 시퀀스 =====

    /// <summary>
    /// 그룹 멧돼지 큐브 발사 시퀀스
    /// </summary>
    private System.Collections.IEnumerator GroupBoarCubeSequence()
    {
        if (childBoarCubes == null || childBoarCubes.Length == 0)
        {
            yield break;
        }

        // 1단계: 모든 큐브 표시 (워닝 시작과 동시에)
        if (showOnWarningStart)
        {
            ShowGroupCubes();
        }

        // 2단계: 모든 자식 큐브 경고 표시
        ShowGroupWarning();

        // 3단계: 경고 시간 대기
        float warningDuration = warning.GetWarningDuration();
        yield return new WaitForSeconds(warningDuration);

        // 4단계: 경고 제거
        ClearGroupWarning();

        // 5단계: 자식 큐브들 동시 발사
        yield return StartCoroutine(SimultaneousGroupLaunch());

        // 6단계: 완료 후 대기 및 숨김
        yield return new WaitForSeconds(deactivateDelay);
        ActionWhenEnd?.Invoke();
        HideGroupCubes();
    }

    /// <summary>
    /// 즉시 그룹 발사 시퀀스
    /// </summary>
    private System.Collections.IEnumerator ImmediateGroupLaunchSequence()
    {
        yield return StartCoroutine(SimultaneousGroupLaunch());
        yield return new WaitForSeconds(deactivateDelay);
        HideGroupCubes();
    }

    /// <summary>
    /// 동시 그룹 발사
    /// </summary>
    private System.Collections.IEnumerator SimultaneousGroupLaunch()
    {
        var launchCoroutines = new System.Collections.Generic.List<Coroutine>();

        // 모든 자식 큐브 동시 발사
        foreach (var child in childBoarCubes)
        {
            if (child != null)
            {
                var childMovement = child.GetComponent<BoarMovement>();
                if (childMovement != null)
                {
                    launchCoroutines.Add(StartCoroutine(childMovement.ExecuteLaunch()));
                }
            }
        }

        // 모든 발사 완료 대기
        foreach (var coroutine in launchCoroutines)
        {
            yield return coroutine;
        }
    }

    #endregion

    #region ===== 그룹 시각 제어 =====

    /// <summary>
    /// 그룹 큐브 표시
    /// </summary>
    private void ShowGroupCubes()
    {
        foreach (var child in childBoarCubes)
        {
            if (child != null && !child.IsVisible)
            {
                lunchEffect.Instantiate(gameObject, child.transform.position, child.transform.rotation);
                child.SetVisibility(true);
            }
        }
    }

    /// <summary>
    /// 그룹 큐브 숨김
    /// </summary>
    private void HideGroupCubes()
    {
        foreach (var child in childBoarCubes)
        {
            if (child != null)
            {
                child.SetVisibility(false);
            }
        }
    }

    #endregion

    #region ===== 그룹 경고 관리 =====

    /// <summary>
    /// 그룹 경고 표시
    /// </summary>
    private void ShowGroupWarning()
    {
        foreach (var child in childBoarCubes)
        {
            if (child != null)
            {
                var childWarning = child.GetComponent<BoarWarning>();
                if (childWarning != null)
                {
                    childWarning.ShowWarning();
                }
            }
        }
    }

    /// <summary>
    /// 그룹 경고 제거
    /// </summary>
    private void ClearGroupWarning()
    {
        foreach (var child in childBoarCubes)
        {
            if (child != null)
            {
                var childWarning = child.GetComponent<BoarWarning>();
                if (childWarning != null)
                {
                    childWarning.ClearWarning();
                }
            }
        }
    }

    #endregion

    #region ===== 유틸리티 메서드 =====

    /// <summary>
    /// 자식 큐브가 있는지 확인
    /// </summary>
    private bool HasChildCubes()
    {
        return childBoarCubes != null && childBoarCubes.Length > 0;
    }

    #endregion

    #region ===== 에디터 시각화 =====

#if UNITY_EDITOR
    /// <summary>
    /// 씬에서 이동 경로 간단 표시
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showPathInScene) return;

        Vector3 currentPos = transform.position;
        Vector3 startPos = currentPos;                              // 현재 위치
        Vector3 endPos = currentPos + startPositionOffset;         // 현재 위치 + 오프셋

        // 경로 선 그리기 (반투명 청록색)
        Gizmos.color = new Color(pathColor.r, pathColor.g, pathColor.b, 0.5f);
        Gizmos.DrawLine(startPos, endPos);

        // 시작점 (작은 초록 구체) - 현재 위치
        Gizmos.color = new Color(0f, 1f, 0f, 0.7f);
        Gizmos.DrawSphere(startPos, 0.2f);

        // 목표점 (작은 빨간 구체) - 돌진 목표
        Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
        Gizmos.DrawSphere(endPos, 0.2f);

        // 간단한 화살표
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 arrowPos = Vector3.Lerp(startPos, endPos, 0.7f);

        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.3f;
        Vector3 left = -right;
        Vector3 back = -direction * 0.4f;

        Gizmos.DrawLine(arrowPos, arrowPos + back + right);
        Gizmos.DrawLine(arrowPos, arrowPos + back + left);

        // 숨김 상태 표시 (큐브 주변에 점선 박스)
        if (!isVisible && startHidden)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // 반투명 마젠타
            Gizmos.DrawWireCube(currentPos, Vector3.one * 1.2f);
        }
    }
#endif

    #endregion
}
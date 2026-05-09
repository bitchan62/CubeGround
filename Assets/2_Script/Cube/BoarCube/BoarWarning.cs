using UnityEngine;
using System.Collections;

/// <summary>
/// 멧돼지 경고 표시 관리 (하나의 발판이 점진적으로 늘어나는 방식)
/// 기존 코드 구조 유지하면서 여러 발판 대신 하나의 발판 사용
/// </summary>
public class BoarWarning : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("경고 시간 설정")]
    [Tooltip("경고 표시 지속 시간 (초)")]
    public float warningDuration = 1f;

    [Tooltip("워닝 표시 시간 비율 (0~ 0.8 전체 시간 낮을 수록 빨리 생김)")]
    [Range(0.1f, 0.8f)]
    public float expansionRatio = 0.3f;

    [Header("경고 거리 설정 (기획자 조절)")]
    [Tooltip("경고 표시 거리 (멧돼지 이동 거리와 독립적)")]
    public Vector3 warningDistance = new Vector3(10, 0, 0);

    [Header("경고 표시 설정")]
    [Tooltip("경고 평면을 바닥에서 얼마나 띄울지 (미터)")]
    public float warningHeightOffset = 0.1f;

    #endregion

    #region ===== 내부 변수 =====

    // 메인 컴포넌트 참조
    private BoarCube main;

    // 하나의 경고 발판
    private GameObject warningPlane;

    #endregion

    #region ===== 초기화 =====

    /// <summary>
    /// BoarCube에서 호출하는 초기화 메서드
    /// </summary>
    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] BoarWarning 초기화 완료");
    }

    #endregion

    #region ===== 공개 메서드 =====

    /// <summary>
    /// 경고 표시 시작 (하나의 발판이 점진적으로 확장)
    /// </summary>
    public void ShowWarning()
    {
        // 기존 경고 제거
        ClearWarning();

        // 멧돼지 경고 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBoarWarning();
        }


        // 점진적 워닝 시작
        StartCoroutine(GradualWarningCoroutine());
    }

    /// <summary>
    /// 모든 경고 표시 제거
    /// </summary>
    public void ClearWarning()
    {
        // 진행 중인 코루틴 중지
        StopAllCoroutines();
        WarningPlaneSetter.DelWarning(this, ref warningPlane);

    }

    /// <summary>
    /// 경고 지속 시간 반환
    /// </summary>
    public float GetWarningDuration()
    {
        return warningDuration;
    }

    #endregion

    #region ===== 내부 메서드 - 점진적 워닝 =====

    /// <summary>
    /// 점진적으로 워닝을 표시하는 코루틴 (하나의 발판이 늘어남)
    /// </summary>
    private IEnumerator GradualWarningCoroutine()
    {
        // 경로 계산
        Vector3 direction = GetWarningDirection();
        float pathLength = GetWarningPathLength();
        float actualWidth = GetActualAttackWidth();

        Vector3 startPos = GetWarningStartPosition();
        Vector3 endPos = GetWarningEndPosition();

        // 워닝 확장 시간 설정
        float expansionTime = warningDuration * expansionRatio;

        // 초기 발판 생성 (큐브 위치에서 시작)
        Vector3 startPoint = startPos;
        startPoint.y = transform.position.y - 1.5f + warningHeightOffset;

        warningPlane = WarningPlaneSetter.SetWarning(
            this,           // MonoBehaviour 컴포넌트
            actualWidth,    // 폭 (고정)
            0.1f,          // 길이 (아주 작게 시작)
            warningDuration, // 전체 지속 시간
            startPoint,     // 시작 위치 (큐브 위치)
            direction       // 방향
        );

        // 점진적으로 발판 크기 확장
        float currentLength = 0.1f;
        float targetLength = pathLength;
        float startTime = Time.time;

        while (currentLength < targetLength && Time.time - startTime < expansionTime)
        {
            float progress = (Time.time - startTime) / expansionTime;
            currentLength = Mathf.Lerp(0.1f, targetLength, progress);

            // 발판 크기 업데이트하면서 위치도 조정 (큐브에서부터 늘어나도록)
            if (warningPlane != null)
            {
                WarningPlaneCustom.Instance.UpdateSize(warningPlane, actualWidth, currentLength);

                // 발판 위치를 다시 계산 (큐브 위치 + 현재 길이의 절반만큼 이동)
                Vector3 newCenter = startPoint + direction * (currentLength * 0.5f);
                warningPlane.transform.position = newCenter;
            }

            yield return null;
        }

        // 최종 크기로 설정
        if (warningPlane != null)
        {
            WarningPlaneCustom.Instance.UpdateSize(warningPlane, actualWidth, targetLength);
            Vector3 finalCenter = startPoint + direction * (targetLength * 0.5f);
            warningPlane.transform.position = finalCenter;
        }

        // 워닝 완료 후 남은 시간 대기
        float remainingTime = warningDuration - expansionTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
    }

    #endregion

    #region ===== 유틸리티 메서드 - 경고용 별도 계산 =====

    /// <summary>
    /// 경고 표시용 이동 방향 계산 (오프셋 적용)
    /// </summary>
    private Vector3 GetWarningDirection()
    {
        return (GetWarningEndPosition() - GetWarningStartPosition()).normalized;
    }

    /// <summary>
    /// 경고 표시용 경로 총 길이 계산 (오프셋 적용)
    /// </summary>
    private float GetWarningPathLength()
    {
        return Vector3.Distance(GetWarningStartPosition(), GetWarningEndPosition());
    }

    /// <summary>
    /// 경고 표시용 시작 위치 계산 (현재 큐브 위치)
    /// </summary>
    private Vector3 GetWarningStartPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// 경고 표시용 목표 위치 계산 (독립적인 경고 거리)
    /// </summary>
    private Vector3 GetWarningEndPosition()
    {
        return transform.position + warningDistance;
    }

    /// <summary>
    /// 멧돼지 실제 이동 방향 계산 (기존 방식)
    /// </summary>
    private Vector3 GetMovementDirection()
    {
        return (GetEndPosition() - GetStartPosition()).normalized;
    }

    /// <summary>
    /// 멧돼지 실제 경로 총 길이 계산 (기존 방식)
    /// </summary>
    private float GetPathLength()
    {
        return Vector3.Distance(GetStartPosition(), GetEndPosition());
    }

    /// <summary>
    /// 멧돼지 실제 시작 위치 계산 (현재 큐브 위치)
    /// </summary>
    private Vector3 GetStartPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// 멧돼지 실제 목표 위치 계산 (현재 위치 + 원본 오프셋)
    /// </summary>
    private Vector3 GetEndPosition()
    {
        return transform.position + main.startPositionOffset;
    }

    /// <summary>
    /// 실제 공격 폭 계산
    /// </summary>
    private float GetActualAttackWidth()
    {
        if (main.attackWidth > 0f)
            return main.attackWidth;

        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            return Mathf.Max(cubeRenderer.bounds.size.x, cubeRenderer.bounds.size.z);
        }

        return Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    }

    #endregion

    #region ===== 정보 조회 =====

    /// <summary>
    /// 현재 활성화된 경고 평면 수 반환
    /// </summary>
    public int GetActiveWarningCount()
    {
        return warningPlane != null && warningPlane.activeInHierarchy ? 1 : 0;
    }

    /// <summary>
    /// 경고가 현재 표시 중인지 여부
    /// </summary>
    public bool IsWarningActive
    {
        get { return warningPlane != null && warningPlane.activeInHierarchy; }
    }

    #endregion

    #region ===== 생명주기 관리 =====

    /// <summary>
    /// 컴포넌트 비활성화 시 정리 (OnDestroy 대신 OnDisable 사용)
    /// </summary>
    void OnDisable()
    {
        ClearWarning();
    }

    #endregion

    #region ===== 에디터 시각화 =====

#if UNITY_EDITOR
    /// <summary>
    /// 씬에서 경고 경로 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (main == null) return;

        // 멧돼지 실제 이동 경로 (청록색)
        Vector3 boarStartPos = GetStartPosition();
        Vector3 boarEndPos = GetEndPosition();

        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawLine(boarStartPos, boarEndPos);

        // 경고 표시 경로 (균일한 빨간색)
        Vector3 warningStartPos = GetWarningStartPosition();
        Vector3 warningEndPos = GetWarningEndPosition();

        Gizmos.color = new Color(1f, 0f, 0f, 0.7f); // 균일한 빨간색
        Gizmos.DrawLine(warningStartPos, warningEndPos);

        // 시작점들 표시
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(boarStartPos, 0.2f);
        Gizmos.DrawSphere(warningStartPos, 0.2f);

        // 끝점들 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(boarEndPos, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(warningEndPos, 0.2f);
    }
#endif

    #endregion
}
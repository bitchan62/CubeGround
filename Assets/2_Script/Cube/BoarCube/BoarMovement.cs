using UnityEngine;
using System.Collections;

/// <summary>
/// 멧돼지 이동 관리
/// 현재 위치에서 startPositionOffset 방향으로 직선 돌진
/// </summary>
public class BoarMovement : MonoBehaviour
{
    #region ===== 내부 변수 =====

    // 메인 컴포넌트 참조
    private BoarCube main;

    // 위치 관리
    private Vector3 startPosition;    // 돌진 시작 위치 (현재 큐브 위치)
    private Vector3 endPosition;      // 돌진 목표 위치 (현재 위치 + 오프셋)

    // 상태 관리
    private bool isLaunching = false; // 현재 돌진 중인지 여부

    #endregion

    #region ===== 초기화 =====

    /// <summary>
    /// BoarCube에서 호출하는 초기화 메서드
    /// </summary>
    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;

        // 초기 위치 설정
        UpdatePositions(main.startPositionOffset);
    }

    /// <summary>
    /// 시작 위치와 목표 위치 계산 및 업데이트
    /// </summary>
    public void UpdatePositions(Vector3 offset)
    {
        Vector3 currentPosition = transform.position;

        // 시작 위치는 현재 큐브 위치
        startPosition = currentPosition;

        // 목표 위치는 현재 위치 + 오프셋
        endPosition = currentPosition + offset;
    }

    #endregion

    #region ===== 공개 메서드 =====

    /// <summary>
    /// 멧돼지 돌진 실행 (코루틴)
    /// </summary>
    public IEnumerator ExecuteLaunch()
    {
        if (isLaunching)
        {
            if (main.showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] 이미 돌진 중입니다!");
            yield break;
        }

        yield return StartCoroutine(PerformLaunch());
    }

    /// <summary>
    /// 상태 리셋 (재사용을 위해)
    /// </summary>
    public void Reset()
    {
        isLaunching = false;

        // 시작 위치로 복귀 (원래 위치)
        transform.position = startPosition;
    }

    #endregion

    #region ===== 내부 메서드 =====

    /// <summary>
    /// 실제 돌진 수행
    /// </summary>
    private IEnumerator PerformLaunch()
    {
        isLaunching = true;

        // 돌진 시작 전 위치 업데이트
        UpdatePositions(main.startPositionOffset);

        Vector3 launchStartPos = startPosition;  // 현재 큐브 위치
        Vector3 launchEndPos = endPosition;      // 현재 위치 + 오프셋

        float totalDistance = Vector3.Distance(launchStartPos, launchEndPos);
        float journeyProgress = 0f;



        // 멧돼지 돌진 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBoarCharge();
        }

        if (main.showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 멧돼지 돌진 시작!");
            Debug.Log($"[{gameObject.name}] 경로: {launchStartPos} → {launchEndPos} (거리: {totalDistance:F2})");
        }


       WaitForFixedUpdate waitForSeconds = new WaitForFixedUpdate(); 

        // 거리 기반 이동 (속도 일정)
        while (journeyProgress < totalDistance)
        {
            // 프레임당 이동 거리 계산
            float deltaDistance = main.launchSpeed * Time.fixedDeltaTime;
                //* Time.deltaTime;  // 변경됨
            journeyProgress += deltaDistance;

            // 진행률 계산 (0 ~ 1)
            float progress = Mathf.Clamp01(journeyProgress / totalDistance);

            // 위치 보간
            transform.position = Vector3.Lerp(launchStartPos, launchEndPos, progress);

            // 넉백 시스템 체크 (항상 활성화)
            CheckAndPerformKnockback();

            yield return waitForSeconds; //null;  // ← 여기만 변경됨
        }


        // 정확한 목표 위치로 설정
        transform.position = launchEndPos;
        isLaunching = false;

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] 멧돼지 돌진 완료!");
    }

    /// <summary>
    /// 넉백 시스템 체크 및 실행
    /// </summary>
    private void CheckAndPerformKnockback()
    {
        BoarKnockback knockbackComponent = GetComponent<BoarKnockback>();
        if (knockbackComponent != null)
        {
            knockbackComponent.CheckKnockback();
        }
        else if (main.showDebugLog)
        {
            Debug.LogWarning($"[{gameObject.name}] BoarKnockback 컴포넌트를 찾을 수 없습니다!");
        }
    }

    #endregion

    #region ===== 정보 조회 =====

    /// <summary>
    /// 현재 돌진 중인지 여부 반환
    /// </summary>
    public bool IsLaunching
    {
        get { return isLaunching; }
    }

    /// <summary>
    /// 시작 위치 반환
    /// </summary>
    public Vector3 GetStartPosition()
    {
        return startPosition;
    }

    /// <summary>
    /// 목표 위치 반환
    /// </summary>
    public Vector3 GetEndPosition()
    {
        return endPosition;
    }

    /// <summary>
    /// 총 이동 거리 반환
    /// </summary>
    public float GetTotalDistance()
    {
        return Vector3.Distance(startPosition, endPosition);
    }

    #endregion

    #region ===== 디버그 시각화 =====

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 이동 경로 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (main == null) return;

        // 시작점과 끝점 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPosition, 0.3f);

        // 이동 경로 표시
        Gizmos.color = isLaunching ? Color.yellow : Color.blue;
        Gizmos.DrawLine(startPosition, endPosition);

        // 방향 화살표
        Vector3 direction = (endPosition - startPosition).normalized;
        Vector3 arrowPos = Vector3.Lerp(startPosition, endPosition, 0.7f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(arrowPos, direction * 0.5f);

        // 화살촉
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.2f;
        Vector3 left = -right;
        Vector3 back = -direction * 0.3f;

        Gizmos.DrawLine(arrowPos + direction * 0.5f, arrowPos + direction * 0.5f + back + right);
        Gizmos.DrawLine(arrowPos + direction * 0.5f, arrowPos + direction * 0.5f + back + left);
    }

    /// <summary>
    /// 인스펙터에서 현재 상태 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (main == null || !main.showDebugLog) return;

        // 현재 위치에서 목표까지의 남은 거리 표시
        if (isLaunching)
        {
            float remainingDistance = Vector3.Distance(transform.position, endPosition);
            float totalDistance = GetTotalDistance();
            float progress = 1f - (remainingDistance / totalDistance);

            // 진행률을 텍스트로 표시 (에디터에서만)
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"돌진 진행률: {progress:P1}\n남은 거리: {remainingDistance:F1}m"
            );
        }
    }
#endif

    #endregion
}
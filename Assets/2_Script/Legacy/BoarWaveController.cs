using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 멧돼지 큐브 웨이브 패턴을 관리하는 컨트롤러 (동시 진행 + 이동 완료 감지)
/// CubeController와 동일한 절대 시간 기반 딜레이 시스템 사용
/// </summary>
public class BoarWaveController : MonoBehaviour
{
    // ===== 데이터 클래스 =====

    /// <summary>
    /// 개별 큐브와 딜레이 정보를 저장하는 클래스
    /// </summary>
    [System.Serializable]
    public class BoarCubeData
    {
        [Tooltip("발사할 BoarCube")]
        public BoarCube boarCube;

        [Tooltip("패턴 시작부터 이 큐브 발사까지의 딜레이")]
        public float delayTime = 0f;

        // 내부 상태 관리 
        [HideInInspector] public float timer = 0f;
        [HideInInspector] public bool hasActivated = false;
    }

    /// <summary>
    /// 하나의 패턴에 포함되는 큐브들과 설정을 저장하는 클래스
    /// </summary>
    [System.Serializable]
    public class BoarWavePattern
    {
        [Tooltip("이 패턴에서 발사할 큐브들과 절대 시간")]
        public List<BoarCubeData> boarCubes;
    }

    // ===== 인스펙터 설정 변수 =====

    [Header("공통 간격")]
    [Tooltip("큐브간 공통 간격 시간 (누적 적용)")]
    public float sharingDelayTime = 0f;

    [Header("웨이브 패턴")]
    [Tooltip("패턴 1, 2, 3... 순서대로 실행될 패턴들")]
    public List<BoarWavePattern> wavePatterns;

    [Header("트리거 설정")]
    [Tooltip("보스가 진입할 트리거 영역 (콜라이더 필수)")]
    public GameObject triggerArea;

    [Tooltip("감지할 보스의 태그")]
    public string bossTag = "Monster";

    [Header("리셋 설정")]
    [Tooltip("패턴 완료 후 큐브들을 리셋할 때까지의 대기 시간")]
    public float resetDelay = 3f;

    // ===== 내부 상태 변수 =====

    private bool isWaveActive = false;          // 현재 웨이브 진행 중인지 여부
    private int currentPatternIndex = 0;        // 다음에 실행할 패턴의 인덱스
    private BoarWavePattern currentPattern;     // 현재 실행 중인 패턴
    private System.Action onWaveCompleted;      // 웨이브 완료 시 호출할 콜백
    private bool hasNotifiedActivation = false; // 보스 신호 전송 여부

    // ===== 초기화 =====

    private void Start()
    {
        SetupTriggerArea();     // 트리거 영역과 연결
        ApplySharingDelay();    // 공통 딜레이 누적 계산
    }

    // ===== 트리거 시스템 설정 =====

    /// <summary>
    /// 트리거 영역과 연결
    /// </summary>
    private void SetupTriggerArea()
    {
        if (triggerArea != null)
        {
            BoarWaveTrigger waveTrigger = triggerArea.GetComponent<BoarWaveTrigger>();
            if (waveTrigger != null)
            {
                waveTrigger.SetWaveController(this);
            }
            else
            {
                Debug.LogError($"[BoarWaveController] {triggerArea.name}에 BoarWaveTrigger 컴포넌트가 없습니다!");
            }
        }
    }

    // ===== 딜레이 계산 =====

    /// <summary>
    /// 공통 간격 딜레이를 누적 방식으로 적용 
    /// </summary>
    private void ApplySharingDelay()
    {
        if (sharingDelayTime <= 0f) return;

        // 모든 패턴에 대해 딜레이 누적 계산
        foreach (var pattern in wavePatterns)
        {
            float tempTime = 0f;

            // 각 큐브의 딜레이를 누적 방식으로 재계산
            for (int i = 0; i < pattern.boarCubes.Count; i++)
            {
                tempTime += pattern.boarCubes[i].delayTime;  // 기존 개별 딜레이 추가
                tempTime += sharingDelayTime;                // 공통 간격 추가
                pattern.boarCubes[i].delayTime = tempTime;   // 최종 시간으로 덮어쓰기
            }
        }
    }

    // ===== 외부 인터페이스 =====

    /// <summary>
    /// 웨이브 완료 시 호출할 콜백 등록
    /// </summary>
    public void SetWaveCompletedCallback(System.Action callback)
    {
        onWaveCompleted = callback;
    }

    /// <summary>
    /// 보스 진입 시 호출되는 메서드
    /// </summary>
    public void OnBossEntered()
    {
        // 이미 웨이브가 진행 중이면 무시 (중복 실행 방지)
        if (isWaveActive) return;

        StartWavePattern();
    }

    /// <summary>
    /// 보스 퇴장 시 호출되는 메서드 (선택적 구현)
    /// </summary>
    public void OnBossExited()
    {
        if (isWaveActive)
        {
            Debug.Log("[BoarWaveController] 보스 퇴장으로 패턴 중단");
            StopCurrentPattern();
        }
    }

    // ===== 웨이브 실행 로직 =====

    /// <summary>
    /// 패턴을 순서대로 선택하고 웨이브 시작
    /// </summary>
    private void StartWavePattern()
    {
        if (wavePatterns.Count == 0)
        {
            Debug.LogWarning("[BoarWaveController] 설정된 패턴이 없습니다!");
            return;
        }

        // 패턴을 순서대로 선택 (1번 → 2번 → 3번 → 1번... 순환)
        currentPattern = wavePatterns[currentPatternIndex];
        currentPatternIndex = (currentPatternIndex + 1) % wavePatterns.Count;

        // 패턴 상태 초기화
        isWaveActive = true;
        hasNotifiedActivation = false; // 알림 플래그 리셋

        foreach (var cubeData in currentPattern.boarCubes)
        {
            cubeData.timer = 0f;
            cubeData.hasActivated = false;
        }

        Debug.Log($"[BoarWaveController] 패턴 {currentPatternIndex} 시작 - 큐브 {currentPattern.boarCubes.Count}개 동시 진행");
    }

    /// <summary>
    /// 현재 진행 중인 패턴 중단
    /// </summary>
    private void StopCurrentPattern()
    {
        isWaveActive = false;

        // 활성화된 큐브들 즉시 리셋
        if (currentPattern != null)
        {
            foreach (var cubeData in currentPattern.boarCubes)
            {
                if (cubeData.boarCube != null && cubeData.hasActivated)
                {
                    cubeData.boarCube.gameObject.SetActive(false);
                    cubeData.boarCube.ResetBoarCube();
                    cubeData.hasActivated = false;
                }
            }
        }
    }

    /// <summary>
    /// Update 방식으로 패턴 진행: 발사 완료 + 이동 완료를 모두 확인
    /// </summary>
    void Update()
    {
        if (!isWaveActive || currentPattern == null) return;

        bool allActivated = true;      // 모든 큐브가 발사되었는지
        bool allCompleted = true;      // 모든 큐브가 이동까지 완료했는지

        // 현재 패턴의 모든 큐브 상태 확인
        foreach (var cubeData in currentPattern.boarCubes)
        {
            if (!cubeData.hasActivated)
            {
                // 아직 발사되지 않은 큐브가 있음
                allActivated = false;
                allCompleted = false;

                // 절대 시간 기준으로 타이머 진행 (CubeController와 동일)
                cubeData.timer += Time.deltaTime;
                if (cubeData.timer >= cubeData.delayTime)
                {
                    ActivateBoarCube(cubeData);
                }
            }
            else
            {
                // 발사된 큐브의 이동 상태 확인
                BoarMovement movement = cubeData.boarCube?.GetComponent<BoarMovement>();
                if (movement != null && movement.IsLaunching)
                {
                    // 아직 이동 중인 큐브가 있음
                    allCompleted = false;
                }
            }
        }
        // 모든 큐브가 발사되었으면 보스에게 신호 (한 번만)
        if (allActivated && !hasNotifiedActivation)
        {
            hasNotifiedActivation = true;
            Debug.Log("[BoarWaveController] 모든 큐브 발사 완료");
            onWaveCompleted?.Invoke(); // 보스에게 패턴 완료 신호
        }

        // 모든 큐브가 이동까지 완료되었으면 리셋 (발사된 경우에만)
        if (allActivated && allCompleted)
        {
            Debug.Log("[BoarWaveController] 모든 큐브 이동 완료, 리셋 시작");
            CompleteWavePattern();
        }
    }

    /// <summary>
    /// 개별 BoarCube 활성화
    /// </summary>
    private void ActivateBoarCube(BoarCubeData cubeData)
    {
        if (cubeData.boarCube != null && !cubeData.hasActivated)
        {
            cubeData.boarCube.gameObject.SetActive(true);
            cubeData.boarCube.TriggerLaunch();
            cubeData.hasActivated = true;

            Debug.Log($"[BoarWaveController] 큐브 {cubeData.boarCube.name} 발사 (시간: {cubeData.timer:F2}초)");
        }
    }

    /// <summary>
    /// 패턴 완료 처리
    /// </summary>
    private void CompleteWavePattern()
    {
        isWaveActive = false;
        Debug.Log("[BoarWaveController] 패턴 완료!");

        // 이제 이동이 완료된 후 호출되므로 즉시 리셋 가능
        if (resetDelay > 0f)
        {
            Timer.Instance.StartTimer(this, "ResetCubes", resetDelay, ResetAllCubes);
        }
        else
        {
            ResetAllCubes(); // 즉시 리셋
        }
    }

    /// <summary>
    /// 모든 큐브 리셋 (재사용 준비)
    /// </summary>
    private void ResetAllCubes()
    {
        if (currentPattern == null) return;

        foreach (var cubeData in currentPattern.boarCubes)
        {
            if (cubeData.boarCube != null)
            {
                cubeData.boarCube.gameObject.SetActive(false);
                cubeData.boarCube.ResetBoarCube();
            }
        }

        Debug.Log("[BoarWaveController] 모든 큐브 리셋 완료");
    }

    // ===== 상태 조회 =====

    /// <summary>
    /// 현재 웨이브가 진행 중인지 여부 반환
    /// </summary>
    public bool IsWaveActive
    {
        get { return isWaveActive; }
    }

    /// <summary>
    /// 현재 패턴 인덱스 반환
    /// </summary>
    public int CurrentPatternIndex
    {
        get { return currentPatternIndex; }
    }

    // ===== 정리 작업 =====

    private void OnDestroy()
    {
        // Timer 정리
        if (Timer.Instance != null)
        {
            Timer.Instance.StopTimer(this, "ResetCubes");
        }
    }
}
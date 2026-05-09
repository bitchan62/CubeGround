using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events; // 이벤트 시스템 사용을 위해 추가

/// <summary>
/// 큐브 활성화와 트리거를 관리하는 컴포넌트
/// </summary>
public class CubeController : MonoBehaviour
{
    // 공간 트리거 -> 비정기적 검사 (조건이 작동할 때마다)
    // 시간 트리거 -> 정기적 검사 (매 업데이트마다)

    // -------------------- 공통 트리거 설정 --------------------

    [Header("공통 트리거 설정")]
    [Tooltip("체크하면 새로 추가되는 큐브들이 아래 설정을 자동으로 사용")]
    public bool useSharedTriggerSettings = false;

    [Space]
    [Tooltip("공통 트리거 종류")]
    public TriggerType sharedTriggerType = TriggerType.TimeTrigger;

    [Tooltip("공통 대상 태그")]
    public string sharedTargetTag = "Player";

    [Tooltip("공통 트리거 영역")]
    public GameObject sharedTriggerArea;

    [Tooltip("공통 대기 시간")]
    public float sharedDelayTime = 0f;

    // -------------------- 초기화 --------------------

    void Start()
    {
        // 공통 딜레이 저장용 임시 변수
        float tempTime = 0;

        // 시작 시 모든 큐브 확인
        foreach (var data in activationSettings)
        {
            // ----- 큐브무버 컴포넌트 애드 -----
            if (data.targetCube != null)
                // { Debug.Log("cubeMover 없음"); }

                // ----- 공통 시간 딜레이 부여 -----
                if (0 < sharingDelayTime)
                {
                    tempTime += data.GetDelayTime(this);
                    tempTime += sharingDelayTime;
                    data.delayTime = tempTime;
                }
        } // foreach
    }

    // -------------------- 컨트롤러 트리거 --------------------

    // 이 컨트롤러가 완료된 후 활성화할 다음 큐브 컨트롤러
    // CubeControllerManager에 의해 지정됨
    [HideInInspector] public CubeController nextController;

    // 다음 컨트롤러를 활성화하는 트리거 이벤트
    [HideInInspector] public UnityEvent nextCubeControllerActivate;

    // 활성화 확인
    private bool isActivated = false;

    // 다음 컨트롤러 활성화 메서드
    public void ActivateNextController()
    {
        if (nextController != null)
        {
            Debug.Log($"[{gameObject.name}] 다음 컨트롤러 [{nextController.gameObject.name}]를 활성화합니다.");
            nextController.StartController();
        }
        else
        { Debug.Log($"[{gameObject.name}] 다음 컨트롤러가 설정되지 않았습니다."); }
    }

    // 컨트롤러 시작 메서드
    public void StartController()
    {
        isActivated = true;
        //  Debug.Log($"[{gameObject.attackName}] 컨트롤러 활성화됨");
    }

    // -------------------- 큐브 트리거 --------------------

    // 트리거 조건 타입 정의
    public enum TriggerType
    {
        TimeTrigger,  // 시간 트리거: 일정 시간 경과 후 오브젝트 활성화
        AreaTrigger,  // 영역 트리거: 특정 영역에 플레이어가 들어오면 활성화
        KillTrigger,  // 킬 트리거: 특정 영역의 모든 몬스터 제거 시 활성화
        TimeOrKill,   // 시간 OR 킬 트리거: 시간 달성 또는 몬스터 전멸 시 활성화
        AreaOrKill    // 영역 OR 킬 트리거: 플레이어 진입 또는 몬스터 전멸 시 활성화
    }

    // 영역 트리거 감지 시 호출됨
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
        if (!isActivated) { return; }

        // 각 활성화 데이터를 확인
        foreach (var data in activationSettings)
        {
            // 이미 활성화된 큐브는 스킵
            if (data.hasActivated) continue;

            // 영역 트리거 또는 영역+킬 조건이고 영역과 태그가 일치하는지 확인
            if ((data.GetTriggerType(this) == TriggerType.AreaTrigger || data.GetTriggerType(this) == TriggerType.AreaOrKill) &&
                data.GetTriggerArea(this) == triggerArea &&
                other.CompareTag(data.GetTargetTag(this)))
            {
                Timer.Instance.StartTimer(this, data.GetDelayTime(this), ActivateCube, data);
            }
        }
    }

    // 모든 큐브가 활성화되었는지 확인
    private void CheckAllCubesActivated()
    {
        // 모든 큐브가 활성화되었는지 확인
        foreach (var data in activationSettings)
        {
            // 하나라도 비활성화 상태라면
            // 리턴
            if (!data.hasActivated) { return; }
        }

        // <- activatedCubeCount와 activationSettings.endCount 의 비교로 바꾸기
        //   if (activatedCubeCount < activationSettings.Count) { return; }

        // 클리어 상태가 아니면 다음 진행 X
        if (!ClearTriggerListManager.Instance.IsClear) { return; }

        // 모든 큐브가 활성화되었으면 이벤트 발생
        if (activationSettings.Count > 0)
        {
            // Debug.Log($"[{gameObject.attackName}] 모든 큐브가 활성화되었습니다. 이벤트를 발생시킵니다.");

            // 다음 컨트롤러 활성화 이벤트 발생
            nextCubeControllerActivate?.Invoke();

            GetComponentInParent<CubeControllerConnector>()?.NotifyControllerCompleted(this);
        }
    }

    // 0번이 올킬 + 타임 달성시 자동으로 딜레이 타임 가지던 1번을 위한
    private void ResetAllElementsTimer(CubeData completedData)
    {
        int currentIndex = activationSettings.IndexOf(completedData);

        // 기존 timer 방식 대신 Timer로 순차 활성화
        for (int i = currentIndex + 1; i < activationSettings.Count; i++)
        {
            float delay = (i - currentIndex) * 0.1f;
            Timer.Instance.StartTimer(this, delay, ActivateCube, activationSettings[i]);
        }
    }

    // 매 프레임마다 시간 트리거 체크
    void Update()
    {
        // 일시정지 시 Update 중단
        if (Time.timeScale == 0f) return;

        // 활성화 체크
        if (!isActivated) { return; }

        // 활성화 상태라면, 큐브 활성화 로직 처리
        foreach (var data in activationSettings)
        {
            // 이미 활성화된 큐브는 스킵
            if (data.hasActivated) { continue; }

            switch (data.GetTriggerType(this))
            {
                case TriggerType.TimeTrigger:
                    data.timer += Time.deltaTime; // Time.deltaTime이 0이 되므로 자동으로 멈춤
                    if (data.timer >= data.GetDelayTime(this))
                    { ActivateCube(data); }
                    break;

                case TriggerType.KillTrigger:
                    if (data.GetTriggerArea(this) != null)
                    {
                        AllKillTrigger killTrigger = data.GetTriggerArea(this).GetComponent<AllKillTrigger>();
                        if (killTrigger != null && killTrigger.IsCompleted)
                        {
                            ActivateCube(data);
                        }
                    }
                    break;

                // 시간 OR 킬 트리거 처리
                case TriggerType.TimeOrKill:
                    // 시간 조건 체크
                    data.timer += Time.deltaTime; // 자동으로 멈춤
                    bool timeCondition = (data.timer >= data.GetDelayTime(this));

                    // 킬 조건 체크
                    bool killCondition = false;
                    if (data.GetTriggerArea(this) != null)
                    {
                        AllKillTrigger killTrigger = data.GetTriggerArea(this).GetComponent<AllKillTrigger>();
                        killCondition = (killTrigger != null && killTrigger.IsCompleted);
                    }

                    // OR 조건: 둘 중 하나만 만족하면 활성화
                    if (timeCondition || killCondition)
                    {
                        ActivateCube(data);

                        if (killCondition)
                        {
                            ResetAllElementsTimer(data);
                        }
                    }
                    break;

                case TriggerType.AreaOrKill:
                    // 영역 조건은 OnAreaTrigger에서 처리됨
                    // 킬 조건만 여기서 체크
                    if (data.GetTriggerArea(this) != null)
                    {
                        AllKillTrigger killTrigger = data.GetTriggerArea(this).GetComponent<AllKillTrigger>();
                        if (killTrigger != null && killTrigger.IsCompleted)
                        {
                            ActivateCube(data);
                        }
                    }
                    break;
            }

            //  // 시간 트리거 처리
            //  if (data.GetTriggerType(this) == TriggerType.TimeTrigger)
            //  {
            //      data.timer += Time.deltaTime; // Time.deltaTime이 0이 되므로 자동으로 멈춤
            //      if (data.timer >= data.GetDelayTime(this))
            //      { ActivateCube(data); }
            //  }
            //  
            //  // 킬 트리거 처리
            //  if (data.GetTriggerType(this) == TriggerType.KillTrigger)
            //  {
            //      if (data.GetTriggerArea(this) != null)
            //      {
            //          AllKillTrigger killTrigger = data.GetTriggerArea(this).GetComponent<AllKillTrigger>();
            //          if (killTrigger != null && killTrigger.IsCompleted)
            //          {
            //              ActivateCube(data);
            //          }
            //      }
            //  }

            //  // 시간 OR 킬 트리거 처리
            //  if (data.GetTriggerType(this) == TriggerType.TimeOrKill)
            //  {
            //      // 시간 조건 체크
            //      data.timer += Time.deltaTime; // 자동으로 멈춤
            //      bool timeCondition = (data.timer >= data.GetDelayTime(this));
            //  
            //      // 킬 조건 체크
            //      bool killCondition = false;
            //      if (data.GetTriggerArea(this) != null)
            //      {
            //          AllKillTrigger killTrigger = data.GetTriggerArea(this).GetComponent<AllKillTrigger>();
            //          killCondition = (killTrigger != null && killTrigger.IsCompleted);
            //      }
            //  
            //      // OR 조건: 둘 중 하나만 만족하면 활성화
            //      if (timeCondition || killCondition)
            //      {
            //          ActivateCube(data);
            //  
            //          if (killCondition)
            //          {
            //              ResetAllElementsTimer(data);
            //          }
            //      }
            //  }

            //  // 영역 OR 킬 트리거 처리
            //  if (data.GetTriggerType(this) == TriggerType.AreaOrKill)
            //  {
            //      // 영역 조건은 OnAreaTrigger에서 처리됨
            //      // 킬 조건만 여기서 체크
            //      if (data.GetTriggerArea(this) != null)
            //      {
            //          AllKillTrigger killTrigger = data.GetTriggerArea(this).GetComponent<AllKillTrigger>();
            //          if (killTrigger != null && killTrigger.IsCompleted)
            //          {
            //              ActivateCube(data);
            //          }
            //      }
            //  }
        }

        // 모든 큐브 활성화 체크
        CheckAllCubesActivated();
    }

    // -------------------- 공통 간격 지정 --------------------

    [Tooltip("큐브와 큐브 간의 대기 간격")]
    public float sharingDelayTime = 0f;

    // -------------------- 활성화 --------------------

    // 큐브 활성화 설정을 저장하는 클래스
    [System.Serializable]
    public class CubeData
    {
        [Header("오브젝트 설정")]
        [Tooltip("활성화할 큐브")]
        public GameObject targetCube;

        [Header("트리거 설정")]
        [Tooltip("개별 설정 사용 (체크 해제 시 공통 설정 사용)")]
        public bool useIndividualSettings = true;

        [Space]
        [Tooltip("트리거 종류")]
        public TriggerType triggerType = TriggerType.TimeTrigger;

        [Tooltip("영역 트리거의 대상 태그 (기본: Player)")]
        public string targetTag = "Player";

        [Tooltip("트리거 영역 오브젝트")]
        public GameObject triggerArea;

        [Tooltip("대기 시간")]
        public float delayTime = 0f;

        // 실제 사용할 설정 반환 메서드들
        public TriggerType GetTriggerType(CubeController controller)
        {
            return useIndividualSettings ? triggerType : controller.sharedTriggerType;
        }

        public string GetTargetTag(CubeController controller)
        {
            return useIndividualSettings ? targetTag : controller.sharedTargetTag;
        }

        public GameObject GetTriggerArea(CubeController controller)
        {
            return useIndividualSettings ? triggerArea : controller.sharedTriggerArea;
        }

        public float GetDelayTime(CubeController controller)
        {
            return useIndividualSettings ? delayTime : controller.sharedDelayTime;
        }

        // 경과한 시간
        [HideInInspector] public float timer = 0f;

        // 활성화 여부
        [HideInInspector] public bool hasActivated = false;
    }

    [Header("큐브 활성화 설정")]
    public List<CubeData> activationSettings = new List<CubeData>();

    // 현재 활성화된 큐브의 숫자
    private int activatedCubeCount = 0;

    // 큐브 숫자 확인
    private int CheckActivatedCubeCount()
    {
        int count = 0;

        foreach (CubeData data in activationSettings)
        {
            if (data.hasActivated)
            { count++; }
        }

        return count;
    }

    // 큐브 활성화
    /// <summary>
    /// 큐브 활성화 (이미 활성화된 큐브도 처리 가능)
    /// </summary>
    private void ActivateCube(CubeData data)
    {
        // 활성화되지 않은 큐브라면
        if (data.targetCube != null && !data.hasActivated)
        {
            if (data.targetCube.activeInHierarchy)
            {
                CubeMover cubeMover = data.targetCube.GetComponent<CubeMover>();
                if (cubeMover != null)
                {
                    cubeMover.StartMovementLogic(); // 이동 로직 직접 실행
                }
            }
            else
            {
                // 비활성화 상태면 기존 로직 (활성화하면 OnEnable에서 자동 시작)
                data.targetCube.SetActive(true);
            }

            data.hasActivated = true;
            activatedCubeCount++;

            // Debug.Log($"[{gameObject.name}] 큐브 [{data.targetCube.name}]가 활성화되었습니다." +
            //     $" ({activatedCubeCount}/{activationSettings.Count})");
        }
    }

    [Header("디버그 옵션")]
    [Tooltip("씬 에디터에서 영역 트리거를 시각화")]
    public bool showTriggerAreas = true;

    // 디버그용: 씬에서 영역 트리거와 큐브를 보여줌
    void OnDrawGizmos()
    {
        if (!showTriggerAreas || activationSettings == null) return;

        foreach (var data in activationSettings)
        {
            // AreaTrigger만 표시 (KillTrigger는 KillAllTrigger에서 자체 시각화)
            if (data.GetTriggerType(this) == TriggerType.AreaTrigger && data.GetTriggerArea(this) != null)
            {
                Collider triggerCollider = data.GetTriggerArea(this).GetComponent<Collider>();
                if (triggerCollider != null)
                {
                    // 영역 트리거는 반투명 박스로 표시
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                    Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (useSharedTriggerSettings)
        {
            // 모든 개별 설정을 공통 설정으로 덮어쓰기
            foreach (var data in activationSettings)
            {
                data.triggerType = sharedTriggerType;
                data.targetTag = sharedTargetTag;
                data.triggerArea = sharedTriggerArea;
                data.delayTime = sharedDelayTime;
            }
        }
    }
#endif
}
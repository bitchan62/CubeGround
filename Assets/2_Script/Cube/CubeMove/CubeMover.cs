using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

/// <summary>
/// 큐브 이동을 관리하는 컴포넌트
/// 미리 배치된 큐브가 시작 시 꺼지고, 활성화될 때 지정한 위치에서 시작하여 원래 배치된 위치로 돌아옴
/// 이동 경로를 레이저로 시각화 (에디터에서만)
/// isTrigger 큐브는 레이어 변경하지 않음
/// 반복 이동 기능 추가
/// </summary>
public class CubeMover : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("시작 위치 (배치된 위치 기준으로 더해짐)")]
    public Vector3 startPositionOffset = new Vector3(10, 0, 0);

    [Tooltip("이동 속도 (초당 유닛)")]
    public float moveSpeed = 3f;

    [Header("시작 상태 설정")]
    [Tooltip("체크하면 처음부터 활성화되어 즉시 이동 시작")]
    public bool startActiveAndMove = false;

    [Header("반복 이동 설정")]
    [Tooltip("체크하면 시작위치 ↔ 원래위치 반복 이동")]
    public bool enableLoopMovement = false;

    [Tooltip("원래 위치에서 대기 시간 (초)")]
    public float waitTimeAtOriginal = 2f;

    [Tooltip("시작 위치에서 대기 시간 (초)")]
    public float waitTimeAtStart = 2f;

    [Header("시각화 설정")]
    [Tooltip("씬에서 이동 경로 시각화")]
    public bool showPath = true;

    // ===== 기존 이동용 프로퍼티 (외부 호환성) =====

    // 이동 상태를 외부에서 확인할 수 있는 프로퍼티 (WarningSystem에서 사용)
    public bool IsCurrentlyMoving
    {
        get
        {
            if (enableLoopMovement)
                return !isWaiting; // 반복 모드: 대기중이 아니면 움직이는 중
            else
                return isMovingToOriginal && !hasArrived; // 기존 모드
        }
    }

    // 도착 여부를 외부에서 확인할 수 있는 프로퍼티
    public bool HasArrived
    {
        get { return hasArrived; }
    }

    // ===== 기존 이동용 변수들 =====

    private Vector3 originalPosition;      // 처음 배치된 위치
    private Vector3 startPosition;         // 계산된 시작 위치
    private bool isMovingToOriginal;       // 원래 위치로 이동 중 (기존 모드용)
    private bool hasArrived;               // 원래 위치에 도착했는지 여부 (기존 모드용)
    private bool isTriggerCube;            // isTrigger 큐브인지 확인 (레이어 변경 방지용)

    // ===== 반복 이동용 변수들 =====

    private bool loopMovingToOriginal = true; // 반복 모드 이동 방향 (true: 원래위치로, false: 시작위치로)
    private bool isWaiting = false;           // 반복 모드 대기 상태

    // ===== 도착 시 이벤트 =====
    public event System.Action whenArrivedEvent;

    // 시작 시 초기화
    void Awake()
    {
        //  // <- CubeFader 추가
        //  // 조금만 더 고생해다오 큐브무버야
        //  if(gameObject.GetComponent<CubeFader>() == null)
        //  {
        //      gameObject.AddComponent<CubeFader>();
        //  }

        originalPosition = transform.position;
        startPosition = originalPosition + startPositionOffset;

        // isTrigger 체크 
        Collider col = GetComponent<Collider>();
        isTriggerCube = (col != null && col.isTrigger);

#if UNITY_EDITOR
        SetupLaserRenderer();
#endif

        if (startActiveAndMove)
        {
            // 활성화 상태로 시작하지만 시작 위치로 미리 이동
            transform.position = startPosition; // <- 아래쪽으로 미리 이동
            ActivateChildrenRecursively(transform);
            // CubeController 명령 대기
        }
        else
        {
            // 기존 로직: 비활성화 상태로 시작
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                if (!isTriggerCube)
                {
                    ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Default"));
                }
            }
        }
    }

    // 자식들도 함께 활성화하는 재귀 메서드
    private void ActivateChildrenRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.SetActive(true);
            ActivateChildrenRecursively(child);
        }
    }

    // 활성화될 때 호출됨 - CubeController 명령 대기만
    void OnEnable()
    {
        // startActiveAndMove가 false인 경우에만 기존 로직 (즉시 이동)
        if (!startActiveAndMove)
        {
            StartMovementLogic();
        }
        // startActiveAndMove가 true면 CubeController 명령 대기
    }

    // CubeController에서만 호출하는 이동 시작 메서드
    public void StartMovementLogic()
    {
        // 시작 위치로 이동
        transform.position = startPosition;

        // 큐브 이동 시작 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCubeMoveStart();
        }

        // 이동 상태 초기화
        if (enableLoopMovement)
        {
            // 반복 모드 초기화
            loopMovingToOriginal = true;
            isWaiting = false;
        }
        else
        {
            // 기존 모드 초기화
            isMovingToOriginal = true;
            hasArrived = false;
        }

        // 트리거가 아닌 경우만 레이어 변경
        if (!isTriggerCube)
        {
            ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Default"));
        }

#if UNITY_EDITOR
        // 에디터에서만 레이저 경로 업데이트
        UpdateLaserPath();
#endif

        this.enabled = true;
    }

    // 매 프레임마다 실행
    void Update()
    {
        // 일시정지 시 Update 중단
        if (Time.timeScale == 0f) return;

        if (enableLoopMovement)
        {
            HandleLoopMovement();
        }
        else
        {
            HandleSingleMovement();
        }

#if UNITY_EDITOR
        // 에디터에서만 레이저 경로 업데이트
        UpdateLaserPath();
#endif
    }

    // ===== 기존 단일 이동 로직 =====
    private void HandleSingleMovement()
    {
        // 이미 도착했으면 컴포넌트 비활성화 (성능 최적화)
        if (hasArrived)
        {
            this.enabled = false;
            return;
        }

        // 원래 위치로 이동 중일 때
        if (isMovingToOriginal && !hasArrived)
        {
            // 현재 위치에서 목표 위치로 이동
            // Time.deltaTime이 0이 되므로 자동으로 멈춤
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPosition,
                moveSpeed * Time.deltaTime
            );

            // 목표 위치에 도달했는지 확인
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
            {
                transform.position = originalPosition;  // 정확한 위치로 설정
                hasArrived = true;                      // 도착 상태로 변경
                //Debug.Log("큐브 도착");
                whenArrivedEvent?.Invoke(); // <- 도착 시 이벤트

                // 큐브 도착 사운드
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayCubeMoveEnd();
                }

                // 트리거가 아닌 경우만 레이어 변경
                if (!isTriggerCube)
                {
                    ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Cube"));
                }

                // NavMesh 리빌드-이동 끝 발판 생성
                //if (NavMeshManager.instance != null)
                //{
                //    NavMeshManager.instance.BuildFull();
                //}

                // 이동 완료 후 컴포넌트 비활성화
                this.enabled = false;
            }
        }
    }

    // ===== 새로운 반복 이동 로직 =====

    private void HandleLoopMovement()
    {
        // 대기 중이면 이동하지 않음
        if (isWaiting) return;

        Vector3 targetPos = loopMovingToOriginal ? originalPosition : startPosition;

        // 목표 위치로 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // 목표 위치에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            transform.position = targetPos; // 정확한 위치로 설정
            StartCoroutine(WaitAndSwitchDirection());
        }
    }

    // ===== 반복 이동 대기 및 방향 전환 =====

    private IEnumerator WaitAndSwitchDirection()
    {
        isWaiting = true;

        // 도착 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCubeMoveEnd();
        }

        // 레이어 변경 (원래 위치 도착 시에만)
        if (loopMovingToOriginal && !isTriggerCube)
        {
            ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Cube"));

            // NavMesh 리빌드
            //if (NavMeshManager.instance != null)
            //{
            //    NavMeshManager.instance.BuildFull();
            //}
        }
        else if (!loopMovingToOriginal && !isTriggerCube)
        {
            // 시작 위치 도착 시 Default 레이어로
            ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Default"));
        }

        // 대기 시간
        float waitTime = loopMovingToOriginal ? waitTimeAtOriginal : waitTimeAtStart;
        yield return new WaitForSeconds(waitTime);

        // 이동 시작 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCubeMoveStart();
        }

        // 방향 전환
        loopMovingToOriginal = !loopMovingToOriginal;
        isWaiting = false;
    }

    // ===== 기존 유틸리티 메서드들 =====

    private void ChangeLayersRecursively(Transform trans, int layer)
    {
        // 각 오브젝트마다 개별적으로 isTrigger 체크
        Collider col = trans.GetComponent<Collider>();
        if (col == null || !col.isTrigger)
        {
            trans.gameObject.layer = layer;  // isTrigger가 아닌 경우만 레이어 변경
        }

        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, layer);
        }
    }

    // 큐브 초기화 (재사용 목적)
    public void Reset()
    {
        if (enableLoopMovement)
        {
            // 반복 모드 리셋
            loopMovingToOriginal = true;
            isWaiting = false;
            StopAllCoroutines(); // 대기 코루틴 중지
        }
        else
        {
            // 기존 모드 리셋
            isMovingToOriginal = false;
            hasArrived = false;
        }

        this.enabled = true; // 리셋 시 다시 활성화

#if UNITY_EDITOR
        // 에디터에서만 레이저 경로 업데이트
        UpdateLaserPath();
#endif
    }

#if UNITY_EDITOR
    [Tooltip("에디터에서만 레이저 효과로 경로 표시")]
    public bool showLaserPath = true;

    [Tooltip("에디터에서 경로 미리보기 (씬 뷰 전용)")]
    public bool showEditorPreview = true;

    // 레이저 경로용 LineRenderer (에디터 전용)
    private LineRenderer pathLaser;

    // 레이저 렌더러 설정 (에디터 전용)
    private void SetupLaserRenderer()
    {
        pathLaser = GetComponent<LineRenderer>();
        if (pathLaser == null && showLaserPath)
        {
            pathLaser = gameObject.AddComponent<LineRenderer>();

            // 레이저 기본 설정
            pathLaser.positionCount = 2; // 시작점과 끝점

            // 레이저의 재질 설정
            pathLaser.material = new Material(Shader.Find("Sprites/Default"));

            // 레이저 너비 설정
            pathLaser.startWidth = 0.1f;
            pathLaser.endWidth = 0.1f;

            // 레이저 색상 설정 (기본: 파란색)
            pathLaser.startColor = Color.blue;
            pathLaser.endColor = Color.blue;
        }

        UpdateLaserPath();
    }

    // 레이저 경로 업데이트 (에디터 전용)
    private void UpdateLaserPath()
    {
        if (pathLaser != null && showLaserPath)
        {
            pathLaser.enabled = true;

            if (enableLoopMovement)
            {
                // 반복 모드: 현재 목표까지의 경로 표시
                if (!isWaiting)
                {
                    Vector3 targetPos = loopMovingToOriginal ? originalPosition : startPosition;
                    pathLaser.SetPosition(0, transform.position);
                    pathLaser.SetPosition(1, targetPos);
                }
                else
                {
                    // 대기 중에는 전체 경로 표시
                    pathLaser.SetPosition(0, startPosition);
                    pathLaser.SetPosition(1, originalPosition);
                }
            }
            else
            {
                // 기존 모드 로직
                if (isMovingToOriginal && !hasArrived)
                {
                    // 현재 위치에서 원래 위치까지
                    pathLaser.SetPosition(0, transform.position);
                    pathLaser.SetPosition(1, originalPosition);
                }
                else if (hasArrived)
                {
                    // 도착 후에는 레이저 비활성화
                    pathLaser.enabled = false;
                }
                else
                {
                    // 정지 상태일 때는 전체 경로 표시
                    pathLaser.SetPosition(0, startPosition);
                    pathLaser.SetPosition(1, originalPosition);
                }
            }
        }
        else if (pathLaser != null)
        {
            pathLaser.enabled = false;
        }
    }

    // 에디터에서 경로 미리보기 (씬 뷰에서만 표시)
    void OnDrawGizmos()
    {
        if (!showEditorPreview) return;

        // 원래 위치와 시작 위치 계산
        Vector3 startPos, endPos;

        if (Application.isPlaying)
        {
            // 실행 중일 때는 저장된 위치 사용
            startPos = originalPosition + startPositionOffset;
            endPos = originalPosition;
        }
        else
        {
            // 에디터에서는 현재 위치를 기준으로 계산
            startPos = transform.position + startPositionOffset;
            endPos = transform.position;
        }

        // 경로 선 그리기
        if (enableLoopMovement)
        {
            // 반복 모드: 양방향 화살표
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // 주황색
        }
        else
        {
            // 일반 모드: 단방향 화살표
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f); // 파란색
        }

        Gizmos.DrawLine(startPos, endPos);

        // 시작점과 끝점에 작은 구체 표시
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // 반투명 초록색
        Gizmos.DrawSphere(startPos, 0.1f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // 반투명 빨간색
        Gizmos.DrawSphere(endPos, 0.1f);

        // 화살표 표시 (방향 표시)
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 arrowPos = Vector3.Lerp(startPos, endPos, 0.5f);

        // 화살표 헤드 그리기
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.2f;
        Vector3 left = -right;
        Vector3 back = -direction * 0.4f;

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // 반투명 노란색
        Gizmos.DrawLine(arrowPos, arrowPos + back + right);
        Gizmos.DrawLine(arrowPos, arrowPos + back + left);
        Gizmos.DrawLine(arrowPos + back + right, arrowPos + back + left);

        // 반복 모드일 때 역방향 화살표도 표시
        if (enableLoopMovement)
        {
            Vector3 reverseArrowPos = Vector3.Lerp(startPos, endPos, 0.3f);
            Vector3 reverseDirection = -direction;
            Vector3 reverseBack = -reverseDirection * 0.4f;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // 주황색
            Gizmos.DrawLine(reverseArrowPos, reverseArrowPos + reverseBack + right);
            Gizmos.DrawLine(reverseArrowPos, reverseArrowPos + reverseBack + left);
            Gizmos.DrawLine(reverseArrowPos + reverseBack + right, reverseArrowPos + reverseBack + left);
        }
    }
#endif
}
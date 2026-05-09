using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CubeController끼리 연결시키는 커넥터
public class CubeControllerConnector : MonoBehaviour
{
    private List<CubeController> controllerSequence = new List<CubeController>();

    // 대기열 관리
    private CubeController currentActiveController;
    private Queue<CubeController> controllerQueue = new Queue<CubeController>();

    // 각 큐브 컨트롤러 간 관계 설정
    void Awake()
    {
        // 기존 코드 그대로 유지
        GetComponentsInChildren<CubeController>(true, controllerSequence);
        Debug.Log($"Found {controllerSequence.Count} controllers.");

        // 컨트롤러 연결
        for (int i = 0; i < controllerSequence.Count - 1; i++)
        {
            CubeController current = controllerSequence[i];
            CubeController next = controllerSequence[i + 1];

            current.nextController = next;

            // 기존 이벤트는 그대로, 추가 리스너만 등록
            current.nextCubeControllerActivate.AddListener(() => SafeActivateNext(next));
        }

        if (controllerSequence[0] != null)
        {
            controllerSequence[0].StartController(); 
            currentActiveController = controllerSequence[0]; 
        }
        else
        { Debug.Log("자식 오브젝트에 CubeController 컴포넌트 존재하지 않음."); }
    }

    // 안전한 다음 컨트롤러 활성화 
    private void SafeActivateNext(CubeController next)
    {
        if (currentActiveController != null)
        {
            // 현재 컨트롤러가 아직 실행 중이면 대기열에 추가
            if (!controllerQueue.Contains(next))
            {
                controllerQueue.Enqueue(next);
                Debug.Log($"컨트롤러 대기열 추가: {next.gameObject.name}");
            }
        }
        else
        {
            // 즉시 실행
            currentActiveController = next;
            next.StartController();
        }
    }

    // 컨트롤러 완료 신호 받기
    public void NotifyControllerCompleted(CubeController completed)
    {
        if (currentActiveController == completed)
        {
            currentActiveController = null;

            // 대기 중인 컨트롤러가 있으면 실행
            if (controllerQueue.Count > 0)
            {
                var next = controllerQueue.Dequeue();
                currentActiveController = next;
                next.StartController();
                Debug.Log($"대기열에서 실행: {next.gameObject.name}");
            }
        }
    }
}
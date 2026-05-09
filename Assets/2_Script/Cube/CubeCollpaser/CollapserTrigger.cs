using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

/// <summary>
/// 캐시 없는 간단한 CollapseTrigger
/// 워닝딜레이만 담당하고 큐브 활성화는 하지 않음
/// </summary>
public class CollapseTrigger : MonoBehaviour
{
    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = true;

    private bool hasTriggered = false;

    List<CubeCollapser> allCollapsers;


    private void Start()
    {
        // 캐시 없이 바로 검색 및 처리
        allCollapsers = FindObjectsOfType<CubeCollapser>(true).ToList();
        List<CubeCollapser> myCollapsers = new List<CubeCollapser>();

        // 분류
        foreach (CubeCollapser currentCollapser in allCollapsers)
        {
            if (currentCollapser.triggerArea == this.gameObject &&
                currentCollapser.triggerType == CubeCollapser.TriggerType.AreaTrigger)
            { myCollapsers.Add(currentCollapser); }
        }

        allCollapsers = myCollapsers;
    }



    private void OnTriggerEnter(Collider other)
    {
        // 플레이어인지 감지하고
        if (other.CompareTag(playerTag))
        {
            // 이미 감지했으면 리턴
            if (oneTimeUse && hasTriggered) return;
            hasTriggered = true;

            // if (showDebugLog)
            //     Debug.Log($"[{gameObject.attackName}] 플레이어 감지!");

            foreach (CubeCollapser currentCollapser in allCollapsers)
            {
                // if (showDebugLog)
                //     Debug.Log($"[{gameObject.attackName}] 타이머 시작: {currentCollapser.gameObject.attackName}, 딜레이: {currentCollapser.warningDelay}초");

                string uniqueKey = $"{currentCollapser.gameObject.GetInstanceID()}_{Time.time}";
                Action tempAction = () => { currentCollapser.TriggerCollapse(); };
                
                Timer.Instance.StartTimer(this, uniqueKey, currentCollapser.warningDelay, tempAction);
            }

            // if (showDebugLog)
            //     Debug.Log($"[{gameObject.attackName}] {processedCount}개 큐브 처리 완료!");
        }
    }

    // 트리거 상태 리셋
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
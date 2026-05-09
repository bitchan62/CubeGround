using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영역 트리거로 사용할 오브젝트에 붙이는 컴포넌트
/// </summary>
public class TriggerArea : MonoBehaviour
{
    [Tooltip("이 트리거를 감지할 CubeController")]
    public CubeController controller;

    // 트리거 영역에 진입했을 때 호출됨
    private void OnTriggerEnter(Collider other)
    {
        if (controller != null)
        {
            controller.OnAreaTrigger(gameObject, other.gameObject);
        }
    }
}
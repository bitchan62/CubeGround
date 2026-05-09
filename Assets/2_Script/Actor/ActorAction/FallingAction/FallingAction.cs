using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingAction : ActorAction
{
    // 떨어졌을 경우의 이벤트
    public MyCallBacks whenFallingEvent { get; set; } = new MyCallBacks();
    public event Action whenAfterFalling; // <- 떨어진 후 이벤트

    // 추락하는 경우, 그 거리
    [SerializeField] private float fallDistance = 30f;

    public float FallDistance
    {
        get { return fallDistance; }
        set { fallDistance = value; }
    }

    private void Update()
    {
        float currentY = thisActor.transform.position.y;
        float lastLandedY = thisActor.foot.lastestRandedPos.y;

        // 현재 y가 과거 착지 위치보다 충분히 낮아졌을 때만 실행
        if (currentY < lastLandedY - fallDistance)
        {
            whenFallingEvent.Invoke();
            whenAfterFalling?.Invoke();
        }
    }
}

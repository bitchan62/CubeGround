using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;



[RequireComponent(typeof(DamageReaction))]
[RequireComponent(typeof(CargoMoveAction))]
// 호위 큐브 (화물)
// Player 레이어, Player 태그 필요
public class Cargo : Actor
{
    // 이동 메커니즘
    CargoMoveAction cargoMoveAction;

    // 현재 목적지
    private CargoDestination _nowDestination;
    public CargoDestination nowDestination
    {
        get // 목적지 확인
        { return _nowDestination; }

        set // 목적지 지정
        {
            _nowDestination = value;

            // 만약 cargoMoveAction이 null상태라면, 초기화
            if (cargoMoveAction == null)
            { cargoMoveAction = moveAction as CargoMoveAction; }

            //cargoMoveAction.SetTarget(value.transform);
        }
    }

    // 목적지 도착을 판정할 distance
    // <- 기즈모
    [SerializeField] protected float distance = 2f;



    // 실행 시 초기화
    protected override void Awake()
    {
        base.Awake();
        // 다운캐스트
        cargoMoveAction = moveAction as CargoMoveAction;
        if (cargoMoveAction == null)
        { Debug.Log("CargoMoveAction 할당되지 않음 : " + gameObject.name); }

        // 목적지까지의 거리 검증
        if (distance < 0)
        { Debug.Log(gameObject.name + " : 목적지 도착 판정 거리가 음수"); distance = 1; }
    }


    // 다음 목적지 지정
    protected void SetNext()
    {
        if (nowDestination.nextDestination != null)
        {
            nowDestination = nowDestination.nextDestination;
            // Debug.Log("목적지 할당 : " + nowDestination.attackName);
        }
    }

    // 일시 정지
    bool isLoopStop = false;

    private void Update()
    {
        // 일시 정지
        if (isLoopStop) { return; }

        // 도착하지 않은 경우 : Move
        if (!cargoMoveAction.InDistance(distance))
        {
            cargoMoveAction.Move();
            cargoMoveAction.Turn();
        }

        // 목적지 도착 시
        // 다음 목적지 설정
        else
        {
            isLoopStop = true; // 일시 정지

            // 다음 목적지가 존재한다면 : 다음 목적지 설정 및 실행
            Timer.Instance.StartTimer(this, nowDestination.nextStartTimer, () => { isLoopStop = false; });
            // StartCoroutine(Timer.StartTimer(nowDestination.nextStartTimer, () => { isLoopStop = false; })); // n초후 시작
            SetNext();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//==================================================
// 이동 행동
// moveVec을 외부에서 입력하면, moveSpeed의 속도로 이동
//==================================================
[RequireComponent(typeof(Rigidbody))]
public class MoveAction : ActorAction
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

    // 초기화
    protected override void Awake()
    {
        base.Awake();

        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
        originMoveSpeed = moveSpeed;
    }


    //==================================================
    // 이동 메서드
    //==================================================

    // 이동할 방향
    // zero면 move 상태 false
    public virtual Vector3 moveVec { get; set; }

    // 현재 이동 속도
    public float moveSpeed = 5;


    // 이동 상태 여부 <- 이후 좀 더 깔@쌈하게 만들 걸 생각해보자
    protected bool _isMove = false;
    public virtual bool isMove
    {
        get { return _isMove; }
        set { _isMove = value; }
    }


    // 이동 메서드
    // 현재위치 += 방향 * 이동 간격 * 이동 간격 보정
    public virtual void Move()
    { rigid.MovePosition(rigid.position + moveVec * moveSpeed * Time.fixedDeltaTime); }

    // 회전
    public virtual void Turn()
    { transform.LookAt(transform.position + moveVec); }



    // ==========
    // 슬로우 관련 시스템
    // ==========

    // 원래 이동 속도
    protected float originMoveSpeed;

    // 슬로우 리스트
    // 인덱스(key) / 슬로우가 적용된 이동속도(value)
    private Dictionary<int, float> slowSpeedDictionary = new Dictionary<int, float>();
    private int slowSpeedKeyIndex = 0;

    // 슬로우 상태
    public void Slow(int slowStrengthPercent, float slowTime)
    {
        slowSpeedDictionary[slowSpeedKeyIndex] = GetSlowSpeed(slowStrengthPercent);

        System.Action<int> tempAction = (index) => { slowSpeedDictionary.Remove(index); SetSlowSpeed(); };
        //  StartCoroutine(Timer.StartTimer<int>(slowTime, tempAction, slowSpeedKeyIndex));
        Timer.Instance.StartTimer(this, slowTime, tempAction, slowSpeedKeyIndex);

        SetSlowSpeed();
        slowSpeedKeyIndex++;
    }

    // 느려진 이동속도 구하기 (퍼센트)
    private float GetSlowSpeed(int slowStrengthPercent)
    { return originMoveSpeed * (1f - (slowStrengthPercent / 100f)); }

    // 느려진 이동속도 구하기 (실수 단위)
    private float GetSlowSpeed(float slowStrength)
    { return originMoveSpeed * (1f - slowStrength); }

    // 슬로우 적용
    private void SetSlowSpeed()
    {
        moveSpeed = originMoveSpeed;
        foreach (float speed in slowSpeedDictionary.Values)
        {if (speed < moveSpeed) { moveSpeed = speed; } }
    }
}
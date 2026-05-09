using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Up_Down : MonoBehaviour
{
    public float amplitude = 0.5f;   // 이동 높이 (위아래 거리)
    public float speed = 1f;         // 이동 속도

    // private Vector3 localStartPos;   // 로컬 시작 위치

    private float timeOffset = 0f;   // 시간 오프셋 추가

    // 이전 프레임의 사인 값을 저장할 변수
    private float previousSinValue = 0f;

    protected virtual void Start()
    {
        // 부모 기준 로컬 위치 저장
        // localStartPos = transform.localPosition;

        previousSinValue = Mathf.Sin(timeOffset * speed) * amplitude;
    }

    protected void Update()
    {
        if (Time.timeScale == 0f) return; // 일시정지 대응

        // Time.time 대신 누적 시간 사용
        timeOffset += Time.deltaTime;

        // 사인파를 이용한 위아래 이동 (로컬 좌표 기준)
        // float newY = localStartPos.y + Mathf.Sin(timeOffset * speed) * amplitude;
        // transform.localPosition = new Vector3(localStartPos.x, newY, localStartPos.z);

        // 현재 프레임의 사인 값을 계산
        float currentSinValue = Mathf.Sin(timeOffset * speed) * amplitude;

        // 이전 프레임과 현재 프레임의 사인 값 차이(변화량)를 계산
        float deltaY = currentSinValue - previousSinValue;

        // 현재 위치에 변화량만큼 더해줌 (로컬 좌표 기준)
        transform.localPosition += new Vector3(0, deltaY, 0);

        // 현재 사인 값을 다음 프레임을 위해 저장
        previousSinValue = currentSinValue;
    }
}
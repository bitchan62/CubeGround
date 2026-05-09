using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation_Y : MonoBehaviour
{
    public float rotationSpeed = 100f; // 초당 회전 속도 (도 단위)

    void Update()
    {
        if (Time.timeScale == 0f) return; // 일시정지 대응 추가

        // 매 프레임마다 Y축으로 회전
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 목적지 체크용
public class CargoDestination : MonoBehaviour
{
    // 목적지 도착 후, 다음 목적지 출발까지 시간
    [SerializeField] public float nextStartTimer = -1f;

    // 다음 목적지
    public CargoDestination nextDestination { get; set; }
}

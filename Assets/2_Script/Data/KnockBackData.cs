using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 넉백 데이터
[System.Serializable]
public class KnockBackData : IData
{
    [Tooltip("공격 적중 시, 넉백 수평 거리")]
    public float power = 0f;
    [Tooltip("공격 적중 시, 넉백 수직 거리")]
    public float height = 0f;
}
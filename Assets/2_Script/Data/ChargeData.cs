using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ChargeData : IData
{
    [Tooltip("돌진 속도")]
    public float speed = 20f;
    [Tooltip("돌진의 총 거리")]
    public float distance = 20f;
}


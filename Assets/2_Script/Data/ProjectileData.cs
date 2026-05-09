using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ProjectileData : IData
{
    [Tooltip("발사할 투사체")]
    public Projectile2 prefab;
    [Tooltip("발사 위치")]
    public Transform firePos;
    [Tooltip("투사체 지속시간 | 0 이하 : 지속시간 무제한")]
    public float duration = 10;
    [Tooltip("투사체 속도")]
    public float speed = 3;

    public Projectile2 Instantiate(Transform otherFirePos = null)
    {
        if (otherFirePos == null) { return GameObject.Instantiate(prefab, firePos.position, firePos.rotation); }
        else                      { return GameObject.Instantiate(prefab, otherFirePos.position, otherFirePos.rotation); }
    }
}
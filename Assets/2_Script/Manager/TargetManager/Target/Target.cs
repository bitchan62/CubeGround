using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 별개의 빈 GameObject를 생성하여
// 타겟팅하고 싶은 Actor의 위치에 배치
public class Target : MonoBehaviour
{
    private void Start()
    {
        Actor actor = transform.root.GetComponent<Actor>();
        TargetManager.Instance.Targeting(this, actor);
    }

    [Header("타겟팅 우선 순위")]
    // 타겟팅 우선 정렬 순서
    public int priority = 1;
}
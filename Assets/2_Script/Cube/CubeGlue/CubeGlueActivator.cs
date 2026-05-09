using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 글루 활성화 컴포넌트
/// OnCollisionEnter로 글루 활성화, 큐브 정지 시 자동 비활성화
/// </summary>
[RequireComponent(typeof(CubeGlue))]
public class CubeGlueActivator : MonoBehaviour
{
    private CubeMover cubeMover;
    private CubeGlue cubeGlue;

    private void Awake()
    {
        cubeMover = GetComponent<CubeMover>();
        cubeGlue = GetComponent<CubeGlue>();
        if (cubeMover == null)
        {
            this.enabled = false;
            cubeGlue.enabled = false;
        }
        else
        {
            cubeMover.whenArrivedEvent += cubeGlue.WhenArrived;
        }
    }

    //  void OnCollisionEnter(Collision collision)
    //  {
    //      // 태그 확인
    //      if (activatePlag && cubeGlue != null && IsValidTarget(collision.transform))
    //      {
    //          cubeGlue.enabled = true;
    //          activatePlag = false;
    //      }
    //  }
    //  
    //  private bool IsValidTarget(Transform target)
    //  {
    //      foreach (string tag in glueTags)
    //      {
    //          if (target.CompareTag(tag))
    //          { return true; }
    //      }
    //      
    //      return false;
    //  }
    //  
    //  private void Update()
    //  {
    //      if (Time.timeScale == 0f) return;
    //  
    //      if (cubeMover.HasArrived)
    //      {
    //          Timer.Instance.StartTimer(this, "_StopGlue", 0.1f,
    //              () => {
    //                  cubeGlue.enabled = false;
    //                  this.enabled = false;
    //              });
    //      }
    //  }
}
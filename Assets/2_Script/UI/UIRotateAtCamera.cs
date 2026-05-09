using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI에 붙이기
// 메인 카메라를 향해서 돌아보도록 만듬
public class UIRotateAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}

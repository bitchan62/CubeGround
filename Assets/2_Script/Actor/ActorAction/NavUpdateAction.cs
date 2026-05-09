using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavUpdateAction : MonoBehaviour
{
    private void OnEnable()
    {
        //originPos = transform.position;
        Timer.Instance?.StartEndlessTimer(this, "_네비갱신", 0.5f, Rebuild);
    }
    
    private void OnDisable()
    {
        Timer.Instance?.StopEndlessTimer(this, "_네비갱신");
    }

    //private Vector3 originPos;
    //private float triggerDistance = 10f;

    private void Rebuild()
    {
        NavMeshManager.instance?.RebuildPartialAsync(transform.position);
    }


}

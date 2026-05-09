using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseWatcher : MonoBehaviour
{
    private CubeCollapser cubeCollapser;

    private void Awake()
    { CollectCubeCollapser(); }


    // private void Update()
    // {
    //     // 일시정지 시 Update 중단
    //     if (Time.timeScale == 0f) return;
    // }


    public bool IsSafe
    {
        get
        {
            CollectCubeCollapser();
            if (cubeCollapser == null ) { return false; }
            return cubeCollapser.IsSafe;
        }
    }


    private void CollectCubeCollapser()
    {
        if (cubeCollapser == null)
        {
            CubeCollapser[] Childcollapsers = GetComponentsInChildren<CubeCollapser>();

            foreach (var collapser in Childcollapsers)
            { if (collapser.enabled) { cubeCollapser = collapser; break; } }

            if (cubeCollapser == null)
            {
                CubeCollapser[] parentCollapsers = GetComponentsInParent<CubeCollapser>();
                foreach (var collapser in parentCollapsers)
                { if (collapser.enabled) { cubeCollapser = collapser; break; } }
            }
        }
    }


}

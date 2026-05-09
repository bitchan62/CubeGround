using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGlue : MonoBehaviour
{
    // private void Start()
    // {
    //     beforePos = this.transform.position;
    // }

    private List<Transform> list = new List<Transform>();

    private void OnCollisionEnter(Collision collision)
    {
        if (!this.enabled) { return; }

        // Player, Monster, Item 태그 허용
        if (!collision.transform.CompareTag("Player") &&
            !collision.transform.CompareTag("Monster") &&
            !collision.transform.CompareTag("Item"))
        { return; }

        list.Add(collision.transform);
        collision?.transform.SetParent(transform, true);
    }

    // private void OnCollisionExit(Collision collision)
    // {
    //     if (!this.enabled) { return; }
    // 
    //     list.Remove(collision.transform);
    //     collision.transform.SetParent(null, true);
    // }

    public void WhenArrived()
    {
        foreach (Transform obj in list)
        { obj?.SetParent(null); }
        this.enabled = false;
        list.Clear();
    }

}
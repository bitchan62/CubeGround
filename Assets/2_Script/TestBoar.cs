using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBoar : MonoBehaviour
{
    public BoarCube cube;

    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            cube.TriggerLaunch();
        }


        if(Input.GetKey(KeyCode.LeftShift))
        {
            cube.ResetBoarCube();
        }
    }


}

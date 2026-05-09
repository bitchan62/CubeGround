using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatus
{
    /// <summary>
    /// 진입 시 1회 실행
    /// </summary>
    void Enter();

    /// <summary>
    /// 매 프레임마다 실행
    /// </summary>
    void Update();

    /// <summary>
    /// 나갈 때 실행
    /// </summary>
    void Exit();
}

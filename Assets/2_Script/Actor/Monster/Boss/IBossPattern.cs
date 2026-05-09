using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 네크로 보스의 패턴으로 사용할 메서드
/// </summary>
public interface IBossPattern
{
    // 보스패턴 시작
    // switchStatus 실행
    void PatternStart();
    
    // 보스 패턴 종료
    // NextPattern?.Invoke() 실행
    System.Action NextPattern { get; set; }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossPatternContext
{
    [HideInInspector]
    public IBossPattern bossPattern;

    [Header("보스 패턴")]
    [Tooltip("몇 번째 보스 페이즈에서 사용할 것인가")]
    public BossPatternManager.BossPhase bossPhase;

    [Tooltip("각 보스페이즈의 몇 번째 패턴으로 사용할 것인가")]
    public int patternOrder = 1;

    [Tooltip("패턴당 반복 횟수")]
    [SerializeField]
    [Range(1, 10)]
    private int originalCount;
    private int currentCount;


    public void Initialize(IBossPattern pattern)
    {
        this.bossPattern = pattern;
        ResetCount();
    }

    public bool IsOneMore()
    {
        bool isOneMore = (0 < --currentCount);
        return isOneMore;
    }

    public void ResetCount()
    { currentCount = originalCount; }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;



public class BossPatternManager : SingletonT<BossPatternManager>
{
    public enum BossPhase
    {
        First,  // 피 70% 초과일 때 패턴  (BossPhase.First)
        Second, // 피 70% ~ 30%일 때 패턴 (BossPhase.Second)
        Third   // 피 30% 미만일 때 패턴  (BossPhase.Third)
    }

    public FinalBossHp finalBossHp;


    private bool IsSecondPhase
    {
        get
        {
            return finalBossHp.sharedHp <= finalBossHp.originalHp * 0.7f;
        }
    }

    private bool IsThirdPhase
    {
        get
        {
            return finalBossHp.sharedHp <= finalBossHp.originalHp * 0.3f;
        }
    }


    // 모든 패턴 목록
    //  private Dictionary<BossPhase, SortedList<int, BossPatternContext>> patterns
    //      = new Dictionary<BossPhase, SortedList<int, BossPatternContext>>();

    private bool isStart = false; // <- 나중에 다시 활성화시킬 수도? 있으려나?

    private BossPhase nowPhase;            // 현재 페이즈
    // private BossPatternContext nowPattern; // 현재 패턴
    private int nowPatternIndex;           // 현재 패턴의 인덱스


    // 모든 패턴 목록
    // 페이즈(BossPhase) -> 페이즈당 순서(int) -> 순서에서 작동할 모든 패턴 정보(List<BossPatternContext>)
    private Dictionary<BossPhase, SortedList<int, List<BossPatternContext>>> allPatterns = new();
    private int endCount = 0; // 패턴 end 갯수 (0 되면 nextPattern)
    private List<BossPatternContext> nowPatterns;




    protected override void Awake()
    {
        base.Awake();

        //  // 각 BossPhase에 대해 SortedList를 미리 초기화
        //  foreach (BossPhase phase in System.Enum.GetValues(typeof(BossPhase)))
        //  { patterns[phase] = new SortedList<int, BossPatternContext>(); }

        foreach (BossPhase phase in System.Enum.GetValues(typeof(BossPhase)))
        { allPatterns[phase] = new SortedList<int, List<BossPatternContext>>(); }

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    //public void Init()
    //{
    //    foreach (BossPhase phase in System.Enum.GetValues(typeof(BossPhase)))
    //    { allPatterns[phase] = new SortedList<int, List<BossPatternContext>>(); }

    //    // 씬 로드 이벤트 구독
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    // 씬이 로드될 때마다 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 모든 상태 초기화
        ResetPatternManager();
        
    }

    // 패턴 매니저 초기화 메서드
    private void ResetPatternManager()
    {
        isStart = false;                  // 시작 플래그 초기화
        nowPhase = BossPhase.First;       // 페이즈 초기화
        nowPatternIndex = 0;              // 패턴 인덱스 초기화
        endCount = 0;                     // 종료 카운트 초기화
        nowPatterns = null;               // 현재 패턴 초기화

        // 패턴 딕셔너리 초기화
        allPatterns.Clear();
        foreach (BossPhase phase in System.Enum.GetValues(typeof(BossPhase)))
        { allPatterns[phase] = new SortedList<int, List<BossPatternContext>>(); }

        Debug.Log("BossPatternManager 초기화 완료");
    }

    protected override void OnDestroy()
    {
        // 씬 로드 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 실행 중인 모든 코루틴 중지
        StopAllCoroutines();

        base.OnDestroy();
    }


    public void AddPattern(BossPatternContext pattern)
    {
        BossPhase phase = pattern.bossPhase;
        int order = pattern.patternOrder;

        // 각 페이즈별 초기화
        if (!allPatterns.ContainsKey(pattern.bossPhase))
        { allPatterns[pattern.bossPhase] = new SortedList<int, List<BossPatternContext>>(); }

        // 각 순서별로 등록되어있는지 확인
        // 이미 등록되어 있다면 : 동시 발생용 list에 Add
        if (allPatterns[phase].ContainsKey(order))
        { allPatterns[phase][order].Add(pattern); }
        else
        {
            List<BossPatternContext> contexts = new List<BossPatternContext>();
            contexts.Add(pattern);
            allPatterns[phase].Add(order, contexts);
        }
    }

    /// <summary>
    /// 이 메서드를 작동시키면 = 보스 패턴 시작
    /// </summary>
    public void PatternStartTrigger()
    {
        // 이미 시작한 상태면 리턴
        if (isStart) { return; }
        else { isStart = true; }

        // 1페이즈 0번 패턴부터 시작
        nowPhase = BossPhase.First;
        nowPatternIndex = 0;
        nowPatterns = allPatterns[nowPhase].Values[nowPatternIndex];
            
        // 패턴 시작
        DoPattern();
    }



    private void EndPattern()
    {
        if (--endCount <= 0)
        { SelectNextPattern(); }
    }


    private void DoPattern()
    {
        foreach (var pattern in nowPatterns)
        {
            endCount++;
            var actionPattern = pattern;

            System.Action action = null;    
            action = () => {
                // <- finalBossHp <= 0이라면 : return
                if (finalBossHp.sharedHp <= 0) { return; }

                if (actionPattern != null &&
                    actionPattern.bossPattern != null)
                {
                    actionPattern.bossPattern.PatternStart();

                    // 1번 더 하면: DoPattern (endCount++만 제외)
                    if (actionPattern.IsOneMore())
                    { actionPattern.bossPattern.NextPattern = action; }
                    // 끝났으면 : 다음 패턴
                    else
                    { actionPattern.bossPattern.NextPattern = EndPattern; }
                }
            };

            action();
        }

    }


    private void SelectNextPattern()
    {
        endCount = 0;

        // 일단 방금까지 사용 중이던 nowPattern 횟수 초기화
        // nowPattern.ResetCount();
        foreach (var pattern in nowPatterns)
        { pattern.ResetCount(); }

        //Debug.Log("BossPatternManager : SelectNextPattern");

        // 페이즈 전환 확인
        BossPhase prevPhase = nowPhase;

        // 페이즈 결정 (체력에 따라)
        if (IsThirdPhase)
        {
            //Debug.Log("BossPhase.Third 선택");
            nowPhase = BossPhase.Third;
        }
        else if (IsSecondPhase)
        {
            //Debug.Log("BossPhase.Second 선택");
            nowPhase = BossPhase.Second;
        }
        else
        {
            //Debug.Log("BossPhase.First 선택");
            nowPhase = BossPhase.First;
        }

        // 페이즈가 바뀌었는지 확인
        if (prevPhase != nowPhase)
        {
            // 새 페이즈로 전환되었을 때: 0번 인덱스부터 시작
            nowPatternIndex = 0;
            Debug.Log($"[BossPattern] 페이즈 전환 : {prevPhase} → {nowPhase}");
        }
        else
        {
            // 같은 페이즈 내에서: 다음 패턴으로 이동 (원형 순환)
            nowPatternIndex += 1;
            nowPatternIndex %= allPatterns[nowPhase].Count;
        }

        // Values[인덱스]로 접근
        //nowPattern = patterns[nowPhase].Values[nowPatternIndex];
        nowPatterns = allPatterns[nowPhase].Values[nowPatternIndex];

        // 패턴 실행
        DoPattern();
    }



}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoarWaveManager : MonoBehaviour
{
    /// <summary>
    /// 유니티 인스펙터용 래퍼
    /// </summary>
    [System.Serializable]
    public class BoarCubes : IBossPattern
    {
        [Header("웨이브 패턴 시 발사될 큐브 목록을 묶은 Transform(=GameObject)")]
        public Transform wave;

        [Header("보스 패턴 정보 : 이 웨이브가 어느 시점에서 활성화될 것인가")]
        public BossPatternContext patternContext = new BossPatternContext();

        // 보어큐브
        private BoarCube[] boars;

        // 다음 패턴 플래그
        private bool isPatternEnd = false;

        // 패턴 시작 시 Action (현재 BossArm avoid 트리거로 사용 중)
        public event Action whenDoPattern;

        public System.Action NextPattern { get; set; }

        private FinalBossHp finalBossHp;

        public bool Init(FinalBossHp finalBossHp)
        {
            if (wave == null)
            {
                Debug.LogError("BoarWaveManager : 보어큐브 묶음 wave가 할당되지 않음");
                return false;
            }
            if (finalBossHp == null) { return false; }

            boars = wave.GetComponentsInChildren<BoarCube>();
            this.finalBossHp = finalBossHp;

            foreach (var boar in boars)
            { finalBossHp.whenDied += boar.StopLaunch; }

            return true;
        }

        private void PatternEnd()
        {
            if (isPatternEnd) { return; }
            isPatternEnd = true;
            NextPattern?.Invoke();

            // Debug.Log("BoarWaveManager : PatternEnd");
        }

        // 1번 쓸 때마다 다음 패턴 사용
        void IBossPattern.PatternStart()
        {
            isPatternEnd = false;

            // <- event : bossArm 이동
            whenDoPattern?.Invoke();

            if (finalBossHp.sharedHp <= 0) { return; }

            foreach (var boar in boars)
            {
                boar.ActionWhenEnd -= PatternEnd; // event 중복 등록 방지
                boar.ResetBoarCube();
                boar.TriggerLaunch(); // <- finalBossHp.sharedHp <= 0 이용해서 발사된 보어큐브가 즉시 멈추도록
                boar.ActionWhenEnd += PatternEnd; // 보어가 완료되었을 경우에 대해서 NextPattern 콜백 등록
            }
        }

    }


    [SerializeField]
    [Header("웨이브 패턴 목록")]
    public List<BoarCubes> wavePatterns = new List<BoarCubes>();
    public FinalBossHp finalBossHp;


    private void Start()
    {
        foreach (BoarCubes boars in wavePatterns)
        {
            if (!boars.Init(finalBossHp)) { continue; }
            boars.patternContext.Initialize(boars);
            BossPatternManager.Instance.AddPattern(boars.patternContext);
        }
    }

}
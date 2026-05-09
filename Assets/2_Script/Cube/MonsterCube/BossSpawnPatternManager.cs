using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class BossSpawnPatternManager : MonoBehaviour
{
    //  [System.Serializable]
    //  public class SpawnerPattern : IBossPattern
    //  {
    //      [Header("스폰 패턴 시 스폰시킬 MonsterSpawner를 자식으로 가진 Transform(=GameObject)")]
    //      public Transform spawnerParent;
    //      
    //      [Header("보스 패턴 정보 : 이 웨이브가 어느 시점에서 활성화될 것인가")]
    //      public BossPatternContext patternContext = new BossPatternContext();
    //  
    //      [Header("다음 패턴 시작까지 걸리는 딜레이")]
    //      [Range(0.1f, 10f)]
    //      public float nextPatternDelay = 0.1f;
    //  
    //      private MonsterSpawner[] spawners;
    //      private FinalBossHp finalBossHp;
    //  
    //      public bool Init(FinalBossHp finalBossHp)
    //      {
    //          if (spawnerParent == null) { return false; }
    //          if (finalBossHp == null) { return false; }
    //  
    //          spawners = spawnerParent.GetComponentsInChildren<MonsterSpawner>();
    //          this.finalBossHp = finalBossHp;
    //  
    //          return true;
    //      }
    //  
    //      public Action NextPattern { get; set; }
    //  
    //      public void PatternStart()
    //      {
    //          if (finalBossHp.sharedHp <= 0) { return; }
    //  
    //          foreach (var spawner in spawners)
    //          {
    //              spawner.ResetSpawner();
    //              spawner.SpawnTriggerOFF();
    //              spawner.SpawnTriggerOn();
    //          }
    //  
    //          // 명의 빌려쓰기
    //          Timer.Instance.StartTimer(BossPatternManager.Instance, nextPatternDelay, () => { NextPattern?.Invoke(); } );
    //      }
    //  }
    //  
    //  [SerializeField]
    //  [Header("스폰 패턴 목록")]
    //  public List<SpawnerPattern> spawnPatternParents = new List<SpawnerPattern>();
    //  public FinalBossHp finalBossHp;
    //  
    //  
    //  private void Start()
    //  {
    //      if (!this.enabled) { return; }
    //  
    //      foreach (var spawnPattern in spawnPatternParents)
    //      {
    //          if (!spawnPattern.Init(finalBossHp)) { continue; }
    //          spawnPattern.patternContext.Initialize(spawnPattern);
    //          BossPatternManager.Instance.AddPattern(spawnPattern.patternContext);
    //      }
    //  }




    [System.Serializable]
    public class SpawnerPattern
    {
        [Header("스포너들의 부모. 자식 스포너들은 모두 Area트리거로 설정할 것 (Time X)")]
        public Transform spawnerParent;
        [Header("다음 스포너가 호출되기까지의 딜레이")]
        public float nextDelay = 5f;

        private List<MonsterSpawner> spawners = new List<MonsterSpawner>();
        private SpawnerPattern nextSpawnPattern;
        private FinalBossHp finalBossHp;

        public void Init(SpawnerPattern next, FinalBossHp finalBossHp)
        {
            this.nextSpawnPattern = next;
            this.finalBossHp = finalBossHp;

            if (spawnerParent == null) { return; }
            foreach (Transform t in spawnerParent)
            {
                MonsterSpawner spawner = t.GetComponent<MonsterSpawner>();
                if (spawner != null)
                { spawners.Add(spawner); }
            }
        }

        public async void Invoke()
        {
            foreach (MonsterSpawner spawner in spawners)
            {
                if (finalBossHp.sharedHp <= 0) { return; }

                spawner.ResetSpawner();
                spawner.SpawnTriggerOFF();
                GameObject obj = spawner.SpawnTriggerOnAndGetSpawnObject();
                Monster monster = obj.GetComponent<Monster>();
                if (monster != null)
                {
                    System.Action action = () => monster.damageReaction.TrueTakeDamage(int.MaxValue);
                    monster.damageReaction.whenDie.Add(() => finalBossHp.whenDied -= action, 1);
                    finalBossHp.whenDied += action;

                    await Task.Delay(100);
                }
            }

            Timer.Instance.StartTimer(BossPatternManager.Instance, nextDelay, nextSpawnPattern.Invoke);
        }

    }



    [SerializeField]
    [Header("순환형 스폰 패턴 목록")]
    public List<SpawnerPattern> spawnPatternParents = new List<SpawnerPattern>();
    [Header("첫 스폰 딜레이")]
    public float firstSpawnDelay = 5f;
    public FinalBossHp finalBossHp;

    private void Start()
    {
        for (int i = 0; i < spawnPatternParents.Count; i++)
        {
            int nextNum = (i + 1) % spawnPatternParents.Count;
            spawnPatternParents[i].Init(spawnPatternParents[nextNum], finalBossHp);
        }

        Timer.Instance.StartTimer(this, firstSpawnDelay, () =>
        {
            if (0 < spawnPatternParents.Count)
            { spawnPatternParents[0].Invoke(); }
        });
    }

}

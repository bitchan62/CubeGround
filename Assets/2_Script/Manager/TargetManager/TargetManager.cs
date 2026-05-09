using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TargetManager : SingletonT<TargetManager>
{
    // 타겟 (위치)
    private Transform _target = null;
    public Transform Target
    {
        get
        {
            if (_target == null)
            {
                if (sortedTargets.Count != 0)
                { _target = sortedTargets.Values[0].transform; }
                if (_target == null)
                {
                    //Debug.Log("TargetManager : _target == FindObjectOfType");
                    var t = FindObjectOfType<Target>();
                    Targeting(t, t.GetComponentInParent<Actor>());
                    _target = sortedTargets.Values[0].transform;
                }
            }
            //Debug.Log($"{_target}");
            return _target;
        }
    }

    public Actor targetActor
    {
        get { return targets[sortedTargets.Values[0]]; }
    }

    // 타겟 리스트
    public Dictionary<Target, Actor> targets = new Dictionary<Target, Actor>();
    public SortedList<int, Target> sortedTargets = new SortedList<int, Target>(); // Target.priority 순서로 정렬

    public void Targeting(Target target, Actor actor)
    {
        if (targets.ContainsKey(target)) { return; }
        
        targets[target] = actor;
        while (sortedTargets.ContainsKey(target.priority)) { target.priority++; }
        sortedTargets[target.priority] = target;

        // foreach (var forTarget in sortedTargets.Values)
        // { Debug.Log($"등록된 Target : {forTarget.transform.root.name}"); }
    }


    /// <summary>
    /// null인 타겟들을 정리하는 메서드
    /// </summary>
    private void CleanupNullTargets()
    {
        // Dictionary에서 null인 Target이나 Actor를 찾아 제거
        List<Target> keysToRemove = new List<Target>();

        foreach (var kvp in targets)
        {
            if (kvp.Key == null || kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        // Dictionary에서 제거
        foreach (var key in keysToRemove)
        {
            targets.Remove(key);
        }

        // SortedList에서 null인 Target을 찾아 제거
        List<int> sortedKeysToRemove = new List<int>();

        foreach (var kvp in sortedTargets)
        {
            if (kvp.Value == null || !targets.ContainsValue(targets.FirstOrDefault(x => x.Key == kvp.Value).Value))
            {
                sortedKeysToRemove.Add(kvp.Key);
            }
        }

        // SortedList에서 제거 (역순으로 제거하여 인덱스 문제 방지)
        foreach (var key in sortedKeysToRemove.OrderByDescending(x => x))
        {
            sortedTargets.Remove(key);
        }

        // 현재 target이 null이면 초기화
        if (_target == null || targets.Count == 0)
        {
            _target = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"{typeof(TargetManager)} : OnSceneLoaded");

        // 기존 타겟들 중 null인 것들 정리
        CleanupNullTargets();

        // 필요시 완전히 초기화하려면 아래 주석 해제
        // targets = new Dictionary<Target, Actor>();
        // sortedTargets = new SortedList<int, Target>();
    }

    private void OnEnable()
    { SceneManager.sceneLoaded += OnSceneLoaded; }

    // OnDisable에서 이벤트 해제
    void OnDisable()
    { SceneManager.sceneLoaded -= OnSceneLoaded; }

}

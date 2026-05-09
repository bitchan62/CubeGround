using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.Universal;


public class Timer : SingletonT<Timer>
{
    // 현재 실행 중인 타이머 저장
    // 단일 실행 보장용
    // key = (gameObject.GetInstanceID(), string)
    private Dictionary<(int, string), Coroutine> continuousTimers = new Dictionary<(int, string), Coroutine>();


    // ===== 키 생성 =====

    // MonoBehaviour 기반 타이머키 생성
    protected (int, string) GetTimerKeyAsMono(MonoBehaviour component, string key)
    {
        var tupleKey = (component.GetInstanceID(), key);
        return tupleKey;
    }

    // 무한반복 타이머용 키 생성
    protected (int, string) GetEndlessTimerKey((int, string) key)
    {
        var endlessKey = (key.Item1, key.Item2 + "_Endless");
        return endlessKey;
    }



    // ===== 단일 타이머 (중복 방지) =====

    // ----- 타이머 시작 -----
    protected void StartTimer((int, string) key, MonoBehaviour component, float duration, Action callback)
    {
        if (continuousTimers.ContainsKey(key)) { return; }

        Coroutine timer = StartCoroutine(TimerCoroutine(key, component, duration, callback));
        continuousTimers[key] = timer;
    }

    // this, string 방식으로 호출
    public void StartTimer(MonoBehaviour component, string key, float duration, Action callback)
    { StartTimer(GetTimerKeyAsMono(component, key), component, duration, callback); }

    // ----- 제네릭 단일 타이머 시작 -----
    protected void StartTimer<T>((int, string) key, MonoBehaviour component, float duration, Action<T> callback, T param)
    {
        if (continuousTimers.ContainsKey(key)) { return; }

        Coroutine timer = StartCoroutine(TimerCoroutine(key, component, duration, callback, param));
        continuousTimers[key] = timer;
    }

    public void StartTimer<T>(MonoBehaviour component, string key, float duration, Action<T> callback, T param)
    { StartTimer(GetTimerKeyAsMono(component, key), component, duration, callback, param); }


    // ----- 타이머 조기 종료 -----
    protected void StopTimer((int, string) key)
    {
        if (!continuousTimers.ContainsKey(key)) { return; }
        Coroutine routine = continuousTimers[key];
        if (routine != null)
        { StopCoroutine(routine); }
        continuousTimers.Remove(key);
    }



    public void StopTimer(MonoBehaviour component, string key)
    { StopTimer(GetTimerKeyAsMono(component, key)); }

    // ----- 타이머 코루틴 ------
    // 개선
    protected IEnumerator TimerCoroutine((int, string) key, MonoBehaviour component, float duration, Action callback)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (component == null)
            {
                // Debug.Log($"{key.ToString()} : 타이머 일시 중지");
                continuousTimers.Remove(key);
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        callback?.Invoke();
        continuousTimers.Remove(key);
    }

    // ----- 제네릭 단일 타이머 코루틴 -----
    protected IEnumerator TimerCoroutine<T>((int, string) key, MonoBehaviour component, float duration, Action<T> callback, T param)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (component == null)
            {
                continuousTimers.Remove(key);
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        callback?.Invoke(param);
        continuousTimers.Remove(key);
    }



    // ===== 무한반복 타이머 (중복방지) =====

    // ----- 중복 실행 방지 EndlessTimer 시작 -----
    protected void StartEndlessTimer((int, string) key, MonoBehaviour component, float duration, Action callback)
    {
        key = GetEndlessTimerKey(key);
        if (continuousTimers.ContainsKey(key)) { return; }

        Coroutine timer = StartCoroutine(EndlessTimerCoroutine(key, component, duration, callback));
        continuousTimers[key] = timer;
    }

    public void StartEndlessTimer(MonoBehaviour component, string key, float duration, Action callback)
    { StartEndlessTimer(GetTimerKeyAsMono(component, key), component, duration, callback); }

    // ----- EndlessTimer 중지 -----
    protected void StopEndlessTimer((int, string) key)
    {
        key = GetEndlessTimerKey(key);
        if (!continuousTimers.ContainsKey(key)) { return; }
        Coroutine routine = continuousTimers[key];
        if (routine != null)
        { StopCoroutine(routine); }
        continuousTimers.Remove(key);
    }

    public void StopEndlessTimer(MonoBehaviour component, string key)
    { StopEndlessTimer(GetTimerKeyAsMono(component, key)); }

    // ----- EndlessTimer 코루틴 (중복 실행 방지) -----
    private IEnumerator EndlessTimerCoroutine((int, string) key, MonoBehaviour component, float duration, Action callback)
    {
        if (duration <= 0f) { Debug.Log($"{key}의 호출 대기시간이 {duration}");  yield break; }

        WaitForSeconds waitForSeconds = new WaitForSeconds(duration);
        while (true)
        {
            if (component == null)
            {
                continuousTimers.Remove(key);
                yield break;
            }
            yield return waitForSeconds;
            callback?.Invoke();
        }
    }


    // ===== 타이머 (중복 가능) =====

    // MonoBehaviour를 파라미터로 받는 버전
    public void StartTimer(MonoBehaviour component, float duration, Action callback)
    {
        StartCoroutine(TimerCoroutine(component, duration, callback));
    }

    private IEnumerator TimerCoroutine(MonoBehaviour component, float duration, Action callback)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (component == null)
            { yield break;}

            elapsed += Time.deltaTime;
            yield return null;
        }
        callback?.Invoke();
    }

    // --- 제네릭 버전 ---
    public void StartTimer<T>(MonoBehaviour component, float duration, Action<T> callback, T param)
    {
        StartCoroutine(TimerCoroutine(component, duration, callback, param));
    }

    private IEnumerator TimerCoroutine<T>(MonoBehaviour component, float duration, Action<T> callback, T param)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (component == null)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        callback?.Invoke(param);
    }



    // ===== 지속시간 중 매 프레임마다 호출하는 타이머 =====

    // 일정 시간 동안 지속하며, 매 프레임마다 callback 호출
    public void StartRepeatTimer(MonoBehaviour component, string key, float duration, Action callback)
    {
        var timerKey = GetTimerKeyAsMono(component, key);
        if (continuousTimers.ContainsKey(timerKey)) { return; }
        if (duration <= 0) { return; }
        Coroutine timer = StartCoroutine(RepeatTimerCoroutine(timerKey, component, duration, callback));
        continuousTimers[timerKey] = timer;
    }

    private IEnumerator RepeatTimerCoroutine((int, string) key, MonoBehaviour component, float duration, Action callback)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (component == null)
            {
                continuousTimers.Remove(key);
                yield break;
            }
            callback?.Invoke();
            elapsed += Time.deltaTime;
            yield return null;
        }

        continuousTimers.Remove(key);
    }

}
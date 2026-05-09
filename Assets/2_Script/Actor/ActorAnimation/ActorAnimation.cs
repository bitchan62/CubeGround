using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class ActorAnimation : ActorAction
{
    // 애니메이터
    protected Animator animator;

    public Animator Animator
    {
        get
        {
            if (animator == null)
            { animator = GetComponent<Animator>(); }
            return animator;
        }
    }


    private Dictionary<AttackName, MyCallBacks> attackExitCallbacks = new Dictionary<AttackName, MyCallBacks>();

    public void RegisterExitCallback(AttackName attackName, System.Action callback)
    {
        MyCallBacks myCallBacks = new MyCallBacks();
        myCallBacks.Add(callback);
        attackExitCallbacks[attackName] = myCallBacks;
    }

    public void InvokeCallback(AttackName attackName)
    {
        if (attackExitCallbacks.TryGetValue(attackName, out var callbacks))
        {
            // Debug.Log($"{name}의 애니메이션 exit 호출 : {attackName.ToString()}");
            callbacks.Invoke();
        }
        else
        {
            // Debug.Log($"{name}의 애니메이션 exit 호출 실패 : {attackName.ToString()}");
        }
    }




    // 레이어까지 체크하는
    // 재생 중 애니메이션 이름 확인
    public virtual bool CheckAnimationName(string animationStateName, int layer = 0)
    {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.IsName(animationStateName);
    }


    // 레이어에서 재생 중인 애니메이션의 종료 확인
    // Update에서 체크
    public bool CheckAnimationEnd(string animationStateName, bool isLooped = false, int layer = 0)
    {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(layer);
        bool isAnimationName = stateInfo.IsName(animationStateName);

        bool isAnimationEnded = false;
        // 애니메이션 이름 체크 + 재생 완료 확인
        if (!isLooped) { isAnimationEnded = 1.0f <= stateInfo.normalizedTime; }

        // 혹은 현재 상태가 해당 애니메이션이지만 트랜지션 중일 수도 있음
        bool isInTransition = Animator.IsInTransition(layer);

        return isAnimationName && (isAnimationEnded || isInTransition);
    }





    // === 애니메이션 재생 ===

    // SetBool 재생
    public virtual void SetAnimationParam(string parameterName, bool p_bool)
    { Animator.SetBool(parameterName, p_bool); }

    // SetTrigger 재생
    public virtual void SetAnimationParam(string parameterName)
    { Animator.SetTrigger(parameterName); }

    public virtual void SetAnimationParam(string parameterName, int p_num)
    { Animator.SetInteger(parameterName, p_num); }

    // GetBool 확인
    public virtual bool GetAnimationParam(string parameterName)
    { return Animator.GetBool(parameterName); }

    public void SetAnimationSpeed(float speed = 1f)
    { Animator.speed = speed; }
}
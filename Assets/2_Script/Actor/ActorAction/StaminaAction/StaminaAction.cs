using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaAction : ActorAction
{
    public enum HowUse
    {
        Jump,
        Dodge
    }

    [SerializeField] public HowUse howUse;

    [SerializeField] private int maxStamina = 3;
    [SerializeField] private int nowStamina = 0;
    [SerializeField] private float staminaRecupRate = 1f;
    [SerializeField] private int recupStamina = 1;

    //이벤트용 UI
    public MyEvent whenStaminaChanged = new MyEvent();


    public int stamina
    {
        get
        { return nowStamina; }
        protected set
        {
            // 최대치 초과 회복 방지
            if (maxStamina <= value)
            {
                value = maxStamina;
                Timer.Instance.StopEndlessTimer(this, "_Recup");
            }

            // 0 미만 떨어짐 방지
            else if (value < 0)
            { value = 0; }

            // (기존 스태미나가 max였을 경우) 스태미나 회복 타이머 시작
            else if (nowStamina == maxStamina && value < maxStamina)
            {
                Timer.Instance.StartEndlessTimer(this, "_Recup",
                    staminaRecupRate,
                    () => stamina += recupStamina);
            }

            // --- 스태미나 적용 ---
            nowStamina = value;

            whenStaminaChanged.Invoke();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        stamina = maxStamina;
    }


    public bool UseStamina(int cost)
    {
        // --- 소모값 검사 ---
        if (cost < 0) { cost = 0; Debug.Log($"{this.gameObject.name} : 스태미나 소모값이 음수 상태"); }

        // --- 스태미나 부족 ---
        if (stamina < cost) { return false; }

        // --- 스태미나 충분 ---
        else
        {
            stamina -= cost;
            return true;
        }
    }

    public void RecoverStamina(int amount)
    {
        stamina += amount; // 내부에서는 protected set 사용 가능
    }

    public int maxStaminaValue { get { return maxStamina; } }

}

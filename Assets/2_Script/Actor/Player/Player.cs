using System.Collections;
using System.Collections.Generic;
// using System.Numerics; // <- Vector3 모호한 참조 오류
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerInputManager))]
[RequireComponent(typeof(JumpAction))]
//[RequireComponent(typeof(BasicWeaponAttack))]
[RequireComponent(typeof(DamageReaction))]
public class Player : Actor
{
    protected PlayerInputManager input;
    protected JumpAction jumpAction;
    protected DodgeAction dodgeAction;

    protected StaminaAction jumpStamina;
    protected StaminaAction dodgeStamina;

    // 생성 초기화
    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<PlayerInputManager>();
        jumpAction = GetComponent<JumpAction>();
        dodgeAction = GetComponent<DodgeAction>();

        var staminas = GetComponents<StaminaAction>();
        foreach (var stamina in staminas)
        {
            switch (stamina.howUse)
            {
                case StaminaAction.HowUse.Jump:
                    jumpStamina = stamina; break;
                case StaminaAction.HowUse.Dodge:
                    dodgeStamina = stamina; break;
            }
        }

        // 사망 시 애니메이션 등록 / 조작해도 Player 동작X
        damageReaction.whenDie.Add(() =>
        {
            // 사망 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayPlayerDeath();
            }

            actorAnimator.SetAnimationParam("DoDie");
            this.enabled = false;
        }, 1);

        //Debug.Log($"플레이어 연결 : GameOverUI.Instance.ConnectToPlayer(gameObject)");
        GameOverUI.Instance.player = this.gameObject;
    }

    private void Start()
    {
        FallingAction fallingAction = GetComponent<FallingAction>();
        FollowCamera followCamera = Camera.main.GetComponent<FollowCamera>();
        fallingAction.whenAfterFalling += () => followCamera.PosReset(0.7f);
        fallingAction.whenAfterFalling += () => {
            this.enabled = false;
            foot.whenGroundEvent.Add(() => { if (!damageReaction.isDie) { this.enabled = true; } }, 1);
        };
    }


    private void FixedUpdate()
    {
        // --- 이동 ---
        if (input.isMoveKeyDown && !dodgeAction.isDodge)
        {
            moveAction.moveVec = input.moveVec;
            moveAction.isMove = true;
            moveAction.Move();
            moveAction.Turn();
        }
        else { moveAction.isMove = false; }
    }


    // 프레임당 업데이트
    protected virtual void Update()
    {
        if (Time.timeScale == 0) { return; }

        // 어린이용 무적커맨드
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I))
        { damageReaction.trueInvinible = !damageReaction.trueInvinible; }

        // --- 입력 ---
        input.SetInput();

        // --- 점프 ---
        if (input.isJumpKeyDown)
        { jumpAction.Jump(); }


        // --- 특정 애니메이션 중 닷지 && 어택 실행 불가 ---
        if (isAnimatePlay) { return; }


        // --- 닷지 ---
        if (input.isDodgeKeyDown && !dodgeAction.isDodge && !isAttacking && dodgeAction.isCanDodge)
        {
            if (dodgeStamina.UseStamina(dodgeAction.dodgeCost))
            {
                if (foot.isRand || 0 < dodgeAction.dodgeWhenJumpCount--)
                {
                    // Debug.Log("닷지");
                    actorAnimator.SetAnimationParam("DoDodge");  // 애니메이션 트리거
                    dodgeAction.Dodge();

                    // 닷지 시 공격 활성화
                    nowAttackKey = AttackName.Player_WhenDodge;
                    attackAction.Attack();
                }
                else
                {
                    // <- 여기에 3회 이상 사용 후, 사용 불가 경고 소리
                    dodgeAction.canNotDodgeEffect.Instantiate(gameObject);
                }
            }
        }


        // --- 공격 ---
        if (input.isAttackKeyDown &&  // 키 누름
            !dodgeAction.isDodge &&   // Dodge 중 아님
            !isAttacking)             // 공격 중 아님
        {
            if (isRand) { nowAttackKey = AttackName.Player_BasicAttack; }
            else        { nowAttackKey = AttackName.Player_JumpComboAttack; }

            // 스테미나 사용 여부
            if (jumpStamina.UseStamina(attackAction.attackCost))
            { attackAction.Attack(); }
        }
    }


    protected void LateUpdate()
    {
        // --- bool 애니메이션 처리 ---
        actorAnimator.SetAnimationParam("IsMove", moveAction.isMove);
        actorAnimator.SetAnimationParam("IsJump", jumpAction.isJump);
        actorAnimator.SetAnimationParam("IsDodge", dodgeAction.isDodge);
    }


    // 애니메이션 재생 시작 시 true
    protected bool isAnimatePlay
    {
        get
        {
            return actorAnimator.CheckAnimationName("Attack_Dodge", 1) ||
                   actorAnimator.CheckAnimationName("Attack_Jump", 1);
        }
    }
}
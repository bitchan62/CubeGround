using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public Camera mainCamera; // 인스펙터에서 참조하거나, 코드로 할당

    private void Start()
    {
        mainCamera = Camera.main;
    }


    //==========
    // 방향 입력
    //==========

    // 입력받는 방향
    protected float moveHorizontal;
    protected float moveVertical;
    public Vector3 moveVec { get; protected set; }

    // 이동 키 입력
    public bool isMoveKeyDown
    { get { return moveVec != Vector3.zero; } }

    // 입력 데드존
    [SerializeField] protected float inputDeadZone = 0.1f;


    // 이동 방향 입력
    protected void InputWASD()
    {
        // 입력(WASD, ↑↓←→)으로 방향 지정
        // 정규화된(모든 방향으로 크기가 1인) 방향벡터 생성
        moveHorizontal = Input.GetAxisRaw("Horizontal"); // x축 (좌우)
        moveVertical = Input.GetAxisRaw("Vertical");     // z축 (앞뒤)

        // 데드존 검사
        if (Mathf.Abs(moveHorizontal) < inputDeadZone) moveHorizontal = 0;
        if (Mathf.Abs(moveVertical) < inputDeadZone) moveVertical = 0;

        // 카메라 기준 forward/right의 y성분 제거
        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // 입력을 카메라 축에 적용
        Vector3 desiredMove = (camForward * moveVertical + camRight * moveHorizontal).normalized;
        moveVec = desiredMove;
    }





    //==========
    // 점프 입력
    //==========

    // 점프 입력 여부
    public bool isJumpKeyDown { get; protected set; }

    // 점프 여부 입력
    // 스페이스 바
    protected void InputJump()
    { isJumpKeyDown = Input.GetButtonDown("Jump") || Input.GetKey(KeyCode.X); }



    //==========
    // 공격 입력
    //==========

    // 공격 입력 여부
    public bool isAttackKeyDown { get; protected set; }

    // 공격 입력
    // 좌클릭
    protected void InputAttack()
    { isAttackKeyDown = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.C); }


    //==========
    // 대시 입력
    //==========

    public bool isDodgeKeyDown { get; protected set; }
    protected void InputDodge()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Z))
        { isDodgeKeyDown = true; }
        else
        { isDodgeKeyDown= false; }
    }



    //==========
    // 통합 입력
    //==========

    // 각종 입력 대응
    // WASD || ↑↓←→
    // Jump(Space Bar)
    // AttackAction(좌클릭)
    public void SetInput()
    { InputWASD(); InputJump(); InputAttack(); InputDodge(); }
}
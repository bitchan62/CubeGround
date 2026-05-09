using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


// 공격 명칭
public enum AttackName
{
    Player_BasicAttack,
    Player_JumpComboAttack,
    Player_DodgeComboAttack,

    Player_WhenDodge,

    Monster_MinionNormalAttack,
    Monster_ShieldChargeAttack,
    Monster_ArcherFireAttack,
    Monster_MageSpellAttack,

    Monster_BossNormalAttack,
    Monster_BossChargeAttack,
    Monster_BossDropAttack,
    Monster_BossTripleAttack,
}



[System.Serializable]
public class AttackData : IData
{
    [Tooltip("공격명. 여러 종류의 공격을 보유한 개체가, 어떤 공격을 하려는지 파악하기 위한 조치")]
    public AttackName attackName;

    [Tooltip("공격 대상의 태그")]
    public string targetTag = "";


    // --- 기본 데이터 ---
    [Tooltip("공격력, 공격 대상에게 입히는 피해량")]
    public int damage = 1;
    
    [Tooltip("공격 애니메이션 속도, 애니메이션 배율로 사용, 높을수록 빠름")]
    public float speed = 1f;

    [Tooltip("공격 사거리/공격 판정 범위. Monster의 사거리 요소로도 사용")]
    public float range = 3f;

    [Tooltip("공격의 스태미나 코스트")]
    public int cost = 0;


    // --- 세부 데이터 ---
    [Tooltip("공격 한 번당, 대상에 대한 최대 히트 수")]
    public int maxHitCount = 1;

    [Tooltip("슈퍼아머 여부. 피격당했을 경우, 이 공격을 취소할 것인가? true면 피격당해도 공격 취소 안됨")]
    public bool superArmor = false;
}

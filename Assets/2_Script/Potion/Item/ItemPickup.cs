using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ItemType
{
    Coin,           // 코인
    HealthPotion    // 체력 물약
}

public class DropItem : MonoBehaviour, IClearTrigger
{
    [Header("아이템 타입")]
    [SerializeField] private ItemType itemType = ItemType.Coin;

    [Header("점수 설정")]
    [SerializeField] private int scoreValue = 10;  // 획득 시 점수

    [Header("회복량 설정")]
    [SerializeField] private int healthHealAmount = 50;    // 체력 회복량

    [Header("효과")]
    public GameObject pickupEffect;  // 픽업 시 파티클 효과

    private ScoreManager scoreManager;

    private void Awake()
    {
        // ScoreManager 로드
        scoreManager = Resources.Load<ScoreManager>("ScoreManager");
        if (scoreManager == null)
        {
            Debug.LogWarning("[DropItem] ScoreManager를 찾을 수 없다");
        }

        // 아이템 태그 안넣었을 때
        if (!gameObject.CompareTag("Item"))
        {
            Debug.LogWarning($"{this.gameObject.name} : 아이템의 태그가 일치하지 않음 (현재 : {this.gameObject.tag})");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ApplyItemEffect(other.gameObject))
            {
                // 점수 추가
                if (scoreManager != null)
                {
                    scoreManager.AddScore(scoreValue);
                    Debug.Log($"[DropItem] {itemType} 획득: +{scoreValue}점");
                }

                PlayPickupEffects();
                Destroy(gameObject);
            }
        }
    }

    private bool ApplyItemEffect(GameObject player)
    {
        DamageReaction damageReaction = player.GetComponent<DamageReaction>();

        // 아이템 타입에 따라 다른 효과 적용
        switch (itemType)
        {
            case ItemType.Coin:
                // 코인은 회복 없이 점수만
                return true;

            case ItemType.HealthPotion:
                // 체력 회복
                if (damageReaction != null)
                {
                    damageReaction.Heal(healthHealAmount);
                    return true;
                }
                break;
        }

        return false;
    }

    private void PlayPickupEffects()
    {
        // 파티클 효과
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // 아이템 타입별 사운드 재생
        if (SoundManager.Instance != null)
        {
            switch (itemType)
            {
                case ItemType.Coin:
                    SoundManager.Instance.PlayItemCoinPickup();
                    break;

                case ItemType.HealthPotion:
                    SoundManager.Instance.PlayItemHealPickup();
                    break;
            }
        }
    }
}
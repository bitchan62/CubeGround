using UnityEngine;
using System.Collections.Generic;


public class MonsterDropSystem : MonoBehaviour
{
    [Header("드롭할 포션들")]
    public List<GameObject> itemPrefeb;

    [Header("물리 설정")]
    public float dropHeight = 0.5f;         // 드롭 높이
    public float scatterRadius = 1f;        // 흩어짐 반경
    public Vector2 bounceForce = new Vector2(2f, 5f); // 튀어나오는 힘 (min, max) // <- 나중에 체크 좀


    private void Awake()
    {
        DamageReaction damageReaction = GetComponent<DamageReaction>();
        if (damageReaction != null)
        { damageReaction.whenDie.Add(DropPotions, 1); }
    }


    // 몬스터 사망 시 호출 - 체크된 포션들 무조건 드롭
    public void DropPotions()
    {
        Vector3 basePosition = transform.position + Vector3.up * dropHeight;

        foreach (var item in itemPrefeb)
        {
            CreateDroppedItem(item, basePosition + Vector3.left * 0.3f);
        }

        // Debug.Log($"{gameObject.attackName}이(가) 포션을 드롭");
    }


    private void CreateDroppedItem(GameObject itemPrefab, Vector3 position)
    {
        // 아이템 생성
        GameObject droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);

        // Item 태그 확실히 설정 (혹시 프리팹에서 빠졌을 경우 대비)
        if (!droppedItem.CompareTag("Item"))
        {
            Debug.Log($"{itemPrefab.name}에 Item 태그 부재.");
        }

        // 아이템을 Item 레이어로 설정 (몬스터와 충돌 방지용)
        LayerChanger.ChangeLayerWithAll(droppedItem, "Item");

        // 튀어나오는 효과
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-0.5f, 0.5f),
                1f,
                Random.Range(-0.5f, 0.5f)
            ).normalized;

            float force = Random.Range(bounceForce.x, bounceForce.y);
            rb.AddForce(randomDirection * force, ForceMode.Impulse);
        }
    }
}
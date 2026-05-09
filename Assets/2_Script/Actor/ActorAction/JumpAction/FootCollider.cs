using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    private void Awake()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        { Debug.LogError($"{gameObject.name}에 착지 판정용 콜라이더 부재"); }
        else
        { collider.isTrigger = true; }

        // 마지막으로 착지 중이었던 위치
        lastestRandedPos = this.transform.position;

        // 보조 판정용 레이어
        groundLayer = 1 << LayerMask.NameToLayer("Cube");
    }

    // 현재 접촉 중인 지형들
    private HashSet<Collider> rands = new HashSet<Collider>();
    public bool isRand
    {
        get
        {
            rands.RemoveWhere(collider => collider == null || !collider.gameObject.activeInHierarchy);
            return 0 < rands.Count || isRandByOverlap;
        }
    }


    // 착지 판정 시 이벤트
    // public MyEvent whenGround = new MyEvent(); // <- remove 추가 후 다시 써보자

    public MyCallBacks whenJumpEvent { get; set; } = new MyCallBacks();

    public MyCallBacks whenGroundEvent { get; set; } = new MyCallBacks();


    // 레이어 체크를 위한 헬퍼 메서드
    // 특정 게임오브젝트가 지정된 LayerMask에 포함되는지 확인
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }


    private void OnTriggerEnter(Collider other)
    {
        bool isCubeTag = other.CompareTag("Cube");
        bool isCubeLayer = IsInLayerMask(other.gameObject, groundLayer);

        // 큐브인 경우
        if ((isCubeTag || isCubeLayer) && !other.isTrigger)
        {
            //Debug.Log("착지");
            rands.Add(other);

            // 이벤트 일괄 발생
            //Debug.Log($"{transform.root.gameObject.name} : {other.name}에 착지");
            whenGroundEvent.Invoke();
        }
    }


    // 마지막으로 착지해 있었던 위치
    public Vector3 lastestRandedPos { get; protected set; }

    // 지형에서 벗어날 경우의 판정
    private void OnTriggerExit(Collider other)
    {
        bool isCubeTag = other.CompareTag("Cube");
        bool isCubeLayer = IsInLayerMask(other.gameObject, groundLayer);

        if ((isCubeTag || isCubeLayer) && !other.isTrigger)
        {
            // 현재 위치 저장
            lastestRandedPos = this.transform.position; // 마지막 위치

            // 착지 중인 콜라이더 목록에서 삭제
            rands.Remove(other);

            // 공중에 뜸 상태라면
            // 마지막 위치를 저장
            // 점프 시 이벤트 발생
            if (!isRand)
            { whenJumpEvent.Invoke(); }
        }
    }



    // --- 판정 보조 ---

    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private float groundCheckOffset = 0.1f;
    private Collider[] overlaps = new Collider[1];
    private LayerMask groundLayer;

    private bool isRandByOverlap
    {
        get
        {
            Vector3 checkPos = transform.position + Vector3.down * groundCheckOffset;
            int hitCount = Physics.OverlapSphereNonAlloc(
                checkPos,
                groundCheckRadius,
                overlaps,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );
            return hitCount > 0;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        // 판정 보조 영역을 초록색 구체 형태의 와이어로 표시
        Gizmos.color = Color.green;
        Vector3 checkPos = transform.position + Vector3.down * groundCheckOffset;
        Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
    }
#endif

}
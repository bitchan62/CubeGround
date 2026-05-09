using UnityEngine;

/// <summary>
/// 오브젝트 풀 Key 관리를 위한 컴포넌트
/// </summary>
public class PooledObject : MonoBehaviour
{
    public GameObject originalPrefab { get; set; }

    // 자가반환
    private void OnDisable()
    { 
        if (!gameObject.activeInHierarchy)
        { PoolManager.ReturnObject(this); }
    }
}
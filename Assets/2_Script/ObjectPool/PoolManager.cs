using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Singleton ObjectPool의 null 안정성을 위한 유틸리티 rapping 클래스
/// </summary>
public class PoolManager
{
    static public T GetTObject<T>(
        T prefab,
        Vector3 position,
        Quaternion rotation) where T : MonoBehaviour
    {
        if (prefab == null) { return null; }
        return ObjectPool.Instance?.GetTObject(prefab, position, rotation);
    }

    static public GameObject GetObject(GameObject prefab)
    {
        if (prefab == null) { return null; }
        return ObjectPool.Instance?.GetObject(prefab);
    }


    static public GameObject GetObject(GameObject prefab, Transform trans)
    {
        if (prefab == null) { return null; }
        return ObjectPool.Instance?.GetObject(prefab, trans);
    }

    static public GameObject GetObject(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (prefab == null) { return null; }
        return ObjectPool.Instance?.GetObject(prefab, position, rotation);
    }

    static public void ReturnObject(PooledObject obj)
    { ObjectPool.Instance?.ReturnObject(obj); }
}

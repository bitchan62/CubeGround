using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectPool : SingletonT<ObjectPool>
{
    // === Data === //

    // Prefab을 Key로, 해당 오브젝트들의 Queue를 Value로 관리
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();


    // === Methods === //
    private void CreateKey(GameObject prefab)
    {
        // 해당 프리팹에 대한 풀이 없으면 생성
        if (!poolDictionary.ContainsKey(prefab))
        { poolDictionary[prefab] = new Queue<GameObject>(); }
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내 반환
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public GameObject GetObject(GameObject prefab, Transform trans = null)
    {
        // 키 생성
        CreateKey(prefab);

        // 풀에서 꺼낼 오브젝트
        GameObject obj;

        // 풀에 오브젝트가 없으면 새로 생성
        if (poolDictionary[prefab].Count == 0)
        {
            if (trans == null) { obj = Instantiate(prefab); }
            else               { obj = Instantiate(prefab, trans); }

            PooledObject pooledObject = obj.AddComponent<PooledObject>();
            pooledObject.originalPrefab = prefab;
        }
        // 있으면 꺼내기
        else
        { obj = poolDictionary[prefab].Dequeue(); }

#if UNITY_EDITOR
        //Debug.Log($"{prefab.name} : 생성 성공");
#endif

        //if (obj == null)
        //{ Debug.Log($"{prefab.name} : 생성 실패"); }

        // 활성화 후 반환
        obj.SetActive(true);

        return obj;
    }


    public GameObject GetObject(GameObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        GameObject obj = GetObject(prefab);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    /// <summary>
    /// 풀에서 T타입 오브젝트 꺼내기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public T GetTObject<T>(T prefab,
        Vector3 position,
        Quaternion rotation) where T : MonoBehaviour
    {
        GameObject obj = GetObject(prefab.gameObject, position, rotation);
        return obj.GetComponent<T>();
    }


    /// <summary>
    /// 풀으로 오브젝트 반환
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnObject(PooledObject obj)
    {
        // 비활성화 후 풀에 반환
        CreateKey(obj.originalPrefab);
        poolDictionary[obj.originalPrefab].Enqueue(obj.gameObject);
    }


    /*
    /// <summary>
    /// 풀 클리어
    /// </summary>
    /// <param name="prefab">대상 프리펩</param>
    public void ClearPool(GameObject prefab = null)
    {
        // prefab 없음 = 전부 삭제
        if (prefab == null)
        {
            foreach (var queue in poolDictionary.Values)
            { ClearPoolQueue(queue); }
            poolDictionary.Clear();
        }

        // prefab 있음 = 그 prefab에 대응하는 pool만 삭제
        else if (poolDictionary.ContainsKey(prefab))
        {
            var queue = poolDictionary[prefab];
            ClearPoolQueue(queue);
            poolDictionary.Remove(prefab);
        }
#if UNITY_EDITOR
        else { Debug.LogWarning($"{prefab.name} : 이 프리팹에 대한 풀은 존재하지 않음"); }
#endif
    }

    private void ClearPoolQueue(Queue<GameObject> queue)
    {
        while (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            if (obj != null)
            { Destroy(obj); }
        }
    }
    */

}
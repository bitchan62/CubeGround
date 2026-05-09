using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerChanger
{
    /// <summary>
    /// 지정된 게임오브젝트와 그 모든 자식 오브젝트들의 레이어를 변경합니다.
    /// </summary>
    /// <param attackName="obj">변경 대상 GameObject</param>
    /// <param attackName="layerName">변경할 레이어 이름</param>


    // ----- 자식 오브젝트의 레이어 변경 -----
    public static void ChangeLayerWithChildren(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"[LayerNameChanger] 잘못된 레이어 이름입니다: '{layerName}'");
            return;
        }

        SetLayerRecursive(obj.transform, layer);
    }

    public static void ChangeLayerWithChildren(GameObject obj, int layerNum)
    {
        if (layerNum == -1)
        {
            Debug.LogError($"[LayerNameChanger] 잘못된 레이어 이름입니다: '{layerNum}'");
            return;
        }

        SetLayerRecursive(obj.transform, layerNum);
    }

    private static void SetLayerRecursive(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        foreach (Transform child in trans)
        {
            SetLayerRecursive(child, layer);
        }
    }


    // ----- 부모 오브젝트의 레이어 변경 -----
    public static void ChangeLayerWithParents(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"[LayerNameChanger] 잘못된 레이어 이름입니다: '{layerName}'");
            return;
        }

        Transform current = obj.transform;
        while (current != null)
        {
            current.gameObject.layer = layer;
            current = current.parent;
        }
    }

    public static void ChangeLayerWithParents(GameObject obj, int layerNum)
    {
        if (layerNum == -1)
        {
            Debug.LogError($"[LayerNameChanger] 잘못된 레이어 이름입니다: '{layerNum}'");
            return;
        }

        Transform current = obj.transform;
        while (current != null)
        {
            current.gameObject.layer = layerNum;
            current = current.parent;
        }
    }



    // ----- 부모/자식 모든 오브젝트 레이어 변경 -----
    public static void ChangeLayerWithAll(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"[LayerNameChanger] 잘못된 레이어 이름입니다: '{layerName}'");
            return;
        }

        // 부모쪽 레이어 변경
        Transform current = obj.transform.parent;
        while (current != null)
        {
            current.gameObject.layer = layer;
            current = current.parent;
        }

        // 자기 자신 + 자식들 레이어 변경
        SetLayerRecursive(obj.transform, layer);
    }

    public static void ChangeLayerWithAll(GameObject obj, int layerNum)
    {
        if (layerNum == -1)
        {
            Debug.LogError($"[LayerNameChanger] 잘못된 레이어 번호입니다: '{layerNum}'");
            return;
        }

        // 부모 변경
        Transform current = obj.transform.parent;
        while (current != null)
        {
            current.gameObject.layer = layerNum;
            current = current.parent;
        }

        // 자기 자신 + 자식 변경
        SetLayerRecursive(obj.transform, layerNum);
    }

}

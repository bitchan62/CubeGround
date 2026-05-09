using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클리어 상태를 체크하고 이벤트 발생
/// </summary>
public class ClearTriggerListManager : SingletonT<ClearTriggerListManager>
{
    private HashSet<IClearTrigger> clearTargetList = new HashSet<IClearTrigger>();

    public bool IsClear
    {
        get
        {
            clearTargetList.RemoveWhere(t => t == null);
            bool isClear = clearTargetList.Count <= 0;
            return isClear;
        }
    }

    public void Add(IClearTrigger target)
    {
        clearTargetList.Add(target);
    }

    public void Remove(IClearTrigger target)
    {
        clearTargetList.Remove(target);
    }

}

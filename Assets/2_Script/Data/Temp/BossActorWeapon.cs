using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossActorWeapon : ActorWeapon2
{
    public EffectData whenCollideEffect = new EffectData();

    // 보스웨펀 콜라이더가 큐브+플레이어를 모두 닿고 있는 동안은 TakeDamage X, 큐브는 벗어났는데 플레이어는 벗어나지 않은 경우 TakeDamage O
    // if 돌진공격 중인 경우에만
    // case 큐브 닿음 -> 플레이어 닿음 : TakeDamage X
    // case 큐브에서는 exit했는데 플레이어 닿은 상태 : TakeDamage O
    // case 플레이어 닿음 -> 큐브 닿음 : TakeDamage O
    //private HashSet<DestructibleCube> cubes = new HashSet<DestructibleCube>();
    //private HashSet<Actor> targetActors = new HashSet<Actor>();


    protected override void OnTriggerEnter(Collider other)
    {
        if (!isActivate) { return; }

        if (other.CompareTag("Cube"))
        {
            // --- 적중 위치에 hit Effect 생성 ---
            InstantHitEffect(other.transform.position, whenCollideEffect);
        }

        // var cube = other.GetComponent<DestructibleCube>();
        // if (cube != null)
        // {
        //     cubes.Add(cube);
        //     //Debug.Log($"{transform.root.name} : OnTriggerEnter : 큐브 갯수 : {cubes.Count}");
        //     return;
        // }
        // 
        // var actor = other.GetComponent<Actor>();
        // if (actor != null)
        // {
        //     targetActors.Add(actor);
        //     // cubes 내부에 null 항목이 있으면 제거
        //     cubes.RemoveWhere(c => c == null);
        //     if (0 < cubes.Count)
        //     { return; }
        // }

        base.OnTriggerEnter(other);
    }


    // protected virtual void OnTriggerExit(Collider other)
    // {
    //     if (!this.enabled) { return; }
    // 
    //     var cube = other.GetComponent<DestructibleCube>();
    //     if (cube != null)
    //     {
    //         cubes.Remove(cube);
    //         //Debug.Log($"{transform.root.name} : OnTriggerExit : 큐브 갯수 : {cubes.Count}");
    //         base.OnTriggerEnter(other);
    //         return;
    //     }
    // 
    //     var actor = other.GetComponent<Actor>();
    //     if (actor != null)
    //     { targetActors.Remove(actor); }
    // }


    // protected override void OnDisable()
    // {
    //     cubes.Clear();
    //     targetActors.Clear();
    // }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 호위 큐브의 목적지를 순서대로 정렬
public class CargoDestinationManager : MonoBehaviour
{
    // 호위 큐브
    Cargo cargo;

    // 공통 시작 시간
    [Header("각 CargoDestination의 nextStartTimer를 미설정 시, 해당 값으로 재조정")]
    [SerializeField] float nextStartTimer = 0;

    void Start()
    {
        // ----- 호위 화물 저장 -----

        // 호위 화물을 가져와서 저장
        Cargo[] cargos = FindObjectsOfType<Cargo>(); // <- 나중에 빼기. 인스펙터 창에 직접 화물 대입
        foreach (var tempCargo in cargos)
        { cargo = tempCargo; }


        // ----- 목적지 간 관계 설정 -----
        
        // 모든 자식 오브젝트로부터
        // Destination 오브젝트 추출 후 저장
        // 위 -> 아래 순서
        List<CargoDestination> destinations = new List<CargoDestination>();
        GetComponentsInChildren<CargoDestination>(true, destinations);


        for (int i = 0; i < destinations.Count - 1; i++)
        {
            // 각 CargoDestination 간에 연결
            destinations[i].nextDestination = destinations[i + 1];

            // 각 목적지의 재출발 대기시간 (공통) 설정
            if (destinations[i].nextStartTimer < 0)
            { destinations[i].nextStartTimer = nextStartTimer; }
        }

        // ----- 첫 번째 목적지를 입력 -----
        cargo.nowDestination = destinations[0];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum WarningShape
{
    Quad,
    Circle
}


// 경고 표시
public class WarningPlanePool : SingletonT<WarningPlanePool>
{
    // 재사용할 발판들을 보관하는 풀
    private List<GameObject> warningQuadPool = new List<GameObject>();

    // 원형 발판
    private List<GameObject> warningCirclePool = new List<GameObject>();
    [SerializeField] private GameObject warningCircle;

    protected override void Awake()
    {
        base.Awake();

        if (warningCircle == null)
        { warningCircle = Resources.Load<GameObject>("WarningPlane/WarningCircle"); }

        CreateWarningPlanes(3);

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 씬이 로드될 때마다 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 풀의 모든 오브젝트 비활성화
        DeactivateAllWarningPlanes();
    }

    // 모든 경고 표시 비활성화
    private void DeactivateAllWarningPlanes()
    {
        // Quad 풀의 모든 오브젝트 비활성화
        foreach (GameObject warning in warningQuadPool)
        {
            if (warning != null)
            {
                warning.SetActive(false);
            }
        }

        // Circle 풀의 모든 오브젝트 비활성화
        foreach (GameObject warning in warningCirclePool)
        {
            if (warning != null)
            {
                warning.SetActive(false);
            }
        }

        //Debug.Log("WarningPlanePool 모든 오브젝트 비활성화 완료");
    }

    // 복수 생성
    private void CreateWarningPlanes(int num)
    {
        for (int i = 0; i < num; i++)
        { CreateWarningPlane(WarningShape.Quad); }

        for (int i = 0; i < num; i++)
        { CreateWarningPlane(WarningShape.Circle); }
    }


    private GameObject CreateWarningPlane(WarningShape shape = WarningShape.Quad)
    {
        GameObject warning;

        switch (shape)
        {
            case WarningShape.Quad:
                warning = GameObject.CreatePrimitive(PrimitiveType.Quad);
                warningQuadPool.Add(warning);
                warning.name = "WarningQuad_" + warningQuadPool.Count;
                break;

            case WarningShape.Circle:
                warning = Instantiate(warningCircle);
                warningCirclePool.Add(warning);
                warning.name = "WarningCircle_" + warningCirclePool.Count;
                break;

            default:
                return null;
        }

        // 기본 상태 세팅
        warning.layer = LayerMask.NameToLayer("IgnoreAll");

        // Pool의 자식으로 설정
        warning.transform.SetParent(this.transform);

        WarningPlaneCustom.Instance.SetBase(warning);

        // 비활성화해서 풀에 보관 (화면에 보이지 않음)
        warning.SetActive(false);

        return warning;
    }



    // 공용 메서드

    // 오브젝트 풀 사용
    public GameObject GetWarningPlaneFromPool(WarningShape shape = WarningShape.Quad)
    {
        // 풀에서 비활성화된(사용 안 중인) 경고 표시 찾기
        // 비활성화 상태 = 재사용 가능
        switch (shape)
        {
            case WarningShape.Quad:
                foreach (GameObject warning in warningQuadPool)
                {
                    // null 체크 추가 (씬 전환 중 파괴된 경우 대비)
                    if (warning != null && !warning.activeInHierarchy)
                    { return warning; }
                }
                return CreateWarningPlane(WarningShape.Quad);


            case WarningShape.Circle:
                foreach (GameObject warning in warningCirclePool)
                {
                    // null 체크 추가 (씬 전환 중 파괴된 경우 대비)
                    if (warning != null && !warning.activeInHierarchy)
                    { return warning; }
                }
                return CreateWarningPlane(WarningShape.Circle);
        }


        return null;
    }


    // 반환
    public void ReturnWarningPlaneToPool(GameObject warning)
    {
        if (warning != null)
        {
            WarningPlaneCustom.Instance.SetBase(warning);

            // Pool의 자식으로 다시 설정
            warning.transform.SetParent(this.transform);

            warning.SetActive(false);
        }
    }

    // OnDestroy에서 정리
    protected override void OnDestroy()
    {
        // 씬 로드 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;

        base.OnDestroy();
    }

}

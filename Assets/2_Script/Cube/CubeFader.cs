using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라로 가려지는 큐브인 경우, 투명화
/// </summary>
public class CubeFader : MonoBehaviour
{
    // 이 오브젝트 및 자식 오브젝트들의 모든 렌더러를 저장할 리스트
    private List<Renderer> objectRenderers = new List<Renderer>();

    // 각 렌더러의 원래 머티리얼들을 저장할 딕셔너리
    // (하나의 렌더러가 여러 개의 머티리얼을 가질 수 있음을 대비)
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    // 현재 투명화된 상태인지 여부를 나타내는 플래그
    public bool isFaded = false;

    public MyCallBacks whenFadeIn = new MyCallBacks();

    // 게임이 시작되기 전, 초기화 단계에서 호출됩니다.
    void Awake()
    {
        // // 초기화
        // objectRenderers = new List<Renderer>();
        // originalMaterials = new Dictionary<Renderer, Material[]>();

        // 이 게임 오브젝트와 모든 자식 오브젝트에 포함된 렌더러 컴포넌트를 전부 찾아옵니다.
        GetComponentsInChildren<Renderer>(true, objectRenderers);

        // 찾아온 모든 렌더러에 대하여
        foreach (Renderer renderer in objectRenderers)
        {
            // 나중에 원래 상태로 되돌리기 위해, 현재의 머티리얼들을 딕셔너리에 저장합니다.
            originalMaterials[renderer] = renderer.materials;
        }
    }

    // 오브젝트를 투명하게 만드는 함수
    public void FadeOut(Material transparentMat)
    {
        // 이미 투명하다면 아무것도 하지 않습니다.
        if (isFaded) { return; }
        isFaded = true;

        // 관리하고 있는 모든 렌더러를 순회하며
        foreach (Renderer renderer in objectRenderers)
        {
            if (renderer == null) { continue; }

            // 임시로 사용할 투명 머티리얼 배열을 생성합니다. (렌더러의 머티리얼 개수만큼)
            int materialCount = originalMaterials[renderer].Length;
            Material[] tempMaterials = new Material[materialCount];

            // 모든 머티리얼을 투명 머티리얼로 교체합니다.
            for (int i = 0; i < materialCount; i++)
            { tempMaterials[i] = transparentMat; }

            renderer.materials = tempMaterials;
        }
    }

    // 오브젝트를 원래의 불투명한 상태로 되돌리는 함수
    public void FadeIn()
    {
        // 투명 상태가 아니라면 아무것도 하지 않습니다.
        if (!isFaded) return;
        isFaded = false;

        whenFadeIn?.Invoke();

        // 관리하고 있는 모든 렌더러를 순회하며
        foreach (Renderer renderer in objectRenderers)
        {
            if (renderer == null) { continue; }

            // Awake에서 저장해두었던 원래의 머티리얼로 되돌립니다.
            renderer.materials = originalMaterials[renderer];
        }
    }

}

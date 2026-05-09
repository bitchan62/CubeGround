using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterials : MonoBehaviour
{
    public void SetAllMaterialsToFadeOut()
    {
        // 모든 자식의 Renderer 컴포넌트 가져오기
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            materials.AddRange(renderer.materials);
        }

        foreach (var mat in materials)
        { SetMaterialToFade(mat); }

        FadeOut();
    }

    // 머티리얼을 Fade 모드로 설정하는 함수
    void SetMaterialToFade(Material mat)
    {
        if (mat == null) { return; }
        if (mat.shader.name != "Standard") { return; }

        mat.SetFloat("_Mode", 2); // 2 = Fade, 3 = Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        mat.renderQueue = 3000;

        // 알파값을 1로 초기화하여 완전 불투명하게 설정
        Color color = mat.color;
        color.a = 1f;
        mat.color = color;
    }


    void FadeOut()
    {
        // ===== 2초에 걸쳐 투명화 =====

        // 모든 자식의 Renderer 컴포넌트 가져오기
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        // 각 Renderer의 머티리얼 리스트 생성
        List<Material> materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            // 머티리얼이 여러 개인 경우도 고려
            materials.AddRange(renderer.materials);
        }


        //
        System.Action<float> action = (t) =>
        {
            foreach (var mat in materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    mat.color = color;
                }
            }
        };
    }


}


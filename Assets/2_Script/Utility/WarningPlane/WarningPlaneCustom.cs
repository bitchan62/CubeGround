using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class WarningPlaneCustom : SingletonT<WarningPlaneCustom>
{
    private Material planeBaseMaterial;
    private MaterialPropertyBlock propBlock;

    private readonly Color warningColor = Color.red; // 경고 색상 (빨간색)
    private readonly Quaternion baseRotation = Quaternion.Euler(90, 0, 0);
    private const float startAlpha = 0.1f;           // 초기 투명도 (30% 불투명)
    private const float maxAlpha = 0.5f;             // 최대 투명도 (80% 불투명)
    private const float emissionIntensity = 1f;      // 발광 강도
    private const float intensityCurve = 1f;         // 색상 변화 곡선 (1 = 선형)
    private float warningStartRate = 0.8f;           // 변화가 시작하는 임계값

    protected override void Awake()
    {
        base.Awake();

        SetBaseMaterial();
        propBlock = new MaterialPropertyBlock();
    }


    /// <summary>
    /// 경고 표시의 머티리얼(색상, 투명도, 발광 등) 설정
    /// 빨간색 반투명 발광 효과 적용
    /// 최초 초기화
    /// </summary>
    private void SetBaseMaterial()
    {
        // 새 머티리얼 생성 (Unity 표준 셰이더 사용)
        planeBaseMaterial = new Material(Shader.Find("Standard"));

        // 반투명 설정 (알파 블렌딩)
        planeBaseMaterial.SetFloat("_Mode", 3); // Transparent 모드
        planeBaseMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        planeBaseMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        planeBaseMaterial.SetInt("_ZWrite", 0);
        planeBaseMaterial.DisableKeyword("_ALPHATEST_ON");
        planeBaseMaterial.EnableKeyword("_ALPHABLEND_ON");
        planeBaseMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        planeBaseMaterial.renderQueue = 3000;

        // 초기 색상 설정 (빨간색, 30% 불투명)
        Color color = warningColor;
        color.a = startAlpha;
        planeBaseMaterial.color = color;

        // 발광 효과 설정 (빨간 빛이 나도록)
        planeBaseMaterial.EnableKeyword("_EMISSION");
        planeBaseMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);
    }


    // 기본 마테리얼로 설정
    private void SetBaseMaterial(GameObject warningPlane)
    {
        Renderer planeRenderer = warningPlane.GetComponentInChildren<Renderer>();
        if (planeRenderer != null)
        {
            // 머티리얼을 경고 표시에 적용
            planeRenderer.material = planeBaseMaterial;
            planeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            planeRenderer.receiveShadows = false;
        }
    }

    private void SetBaseRotation(GameObject warningPlane)
    {
        // 바닥에 평행
        // 90도 돌려주기
        warningPlane.transform.rotation = baseRotation;
    }



    public void SetBase(GameObject warningPlane)
    {
        SetBaseMaterial(warningPlane);
        SetBaseRotation(warningPlane);
    }


    // 비율에 따라 색이 점점 진해지도록
    // 0이 최소, 1이 최대
    public void UpdateColor(GameObject warningPlane, float rate)
    {
        if (rate < 0) { rate = 0; }

        // 임계값보다 비율이 클 때(멀리 있을 때)는 변화 없음, 초기상태 유지
        if (rate <= warningStartRate)
        {
            float interp = Mathf.InverseLerp(warningStartRate, 0f, rate); // 임계~0 사이를 0~1로 변환
            float alpha = Mathf.Lerp(startAlpha, maxAlpha, Mathf.Pow(interp, intensityCurve));
            Color color = warningColor;
            color.a = alpha;
            Color emissionColor = warningColor * alpha * emissionIntensity;

            Renderer renderer = warningPlane.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", color);
                propBlock.SetColor("_EmissionColor", emissionColor);
                renderer.SetPropertyBlock(propBlock);
            }
        }
        else
        {
            // 변화구간 밖(멀리 있음): 항상 초기상태 유지
            Color color = warningColor;
            color.a = startAlpha;
            Color emissionColor = warningColor * startAlpha * emissionIntensity;

            Renderer renderer = warningPlane.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", color);
                propBlock.SetColor("_EmissionColor", emissionColor);
                renderer.SetPropertyBlock(propBlock);
            }
        }
    }


    public void UpdateSize(GameObject warningPlane, float width, float length)
    {
        warningPlane.transform.localScale = new Vector3(width, length, 0.02f);
    }


    public void UpdateRotation(GameObject warningPlane, Vector3 direction)
    {
        // 이동 벡터에서 각도(Yaw) 계산 (월드 Y축 기준)
        float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        // => (0,1)기준 앞, (1,0)기준 오른쪽, (-1,0)기준 왼쪽

        // 평면이 '바닥에 평행'하게 하기 위해 기본은 (90, angleY, 0)
        warningPlane.transform.rotation = Quaternion.Euler(90f, angleY, 0f);
    }


    public void UpdatePosition(GameObject warningPlane, Vector3 pos)
    {
        pos.y += 0.05f;
        warningPlane.transform.position = pos;
    }

}

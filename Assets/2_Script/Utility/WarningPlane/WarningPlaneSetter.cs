using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class WarningPlaneSetter
{
    public static GameObject SetWarning(
        MonoBehaviour component,
        float width, float length,
        float warningTime,
        Vector3 posVec, Vector3 rotationVec)
    {
        GameObject warningPlane = WarningPlanePool.Instance.GetWarningPlaneFromPool();

        // --- 활성화 ---
        warningPlane.SetActive(true);

        // --- 크기 ---
        WarningPlaneCustom.Instance.UpdateSize(warningPlane, width, length);

        // --- 방향 ---
        WarningPlaneCustom.Instance.UpdateRotation(warningPlane, rotationVec);

        // --- 위치 ---
        posVec = posVec + rotationVec * (warningPlane.transform.localScale.y / 2);
        WarningPlaneCustom.Instance.UpdatePosition(warningPlane, posVec);

        // --- 경고 진해지기 ---
        UpdateWarningAlpha(component, warningPlane, warningTime);

        return warningPlane;
    }


    // 경고 발판 지우기
    public static void DelWarning(MonoBehaviour component, ref GameObject warningPlane)
    {
        if (!component.gameObject.scene.isLoaded) { return; }
        if (WarningPlaneCustom.Instance == null) { return; }
        if (WarningPlanePool.Instance == null) { return; }

        if (warningPlane == null) { return; }

        Timer.Instance?.StopTimer(component, "_Warning");
        WarningPlaneCustom.Instance.SetBase(warningPlane);
        WarningPlanePool.Instance.ReturnWarningPlaneToPool(warningPlane);

        warningPlane = null;
    }


    public static void UpdateWarningAlpha(MonoBehaviour component, GameObject warningPlane, float warningTime)
    {
        if (WarningPlaneCustom.Instance == null) { return; }

        float warningAlpha = 1f;
        float opacityRate = 1f / (warningTime * 0.8f);
        System.Action tempAction = () =>
        {
            WarningPlaneCustom.Instance.UpdateColor(warningPlane, warningAlpha);
            warningAlpha -= opacityRate * Time.deltaTime;
            if (warningAlpha <= 0f)
            {
                warningAlpha = 0f;
                Timer.Instance.StopTimer(component, "_Warning");
            }
        };

        Timer.Instance.StartRepeatTimer(component, "_Warning", warningTime, tempAction);
    }



}

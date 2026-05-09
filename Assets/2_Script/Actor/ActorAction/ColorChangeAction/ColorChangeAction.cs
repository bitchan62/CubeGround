using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeAction : ActorAction
{
    private Renderer[] renderers;
    private List<Color[]> originalColors = new List<Color[]>();
    private Color redColor;




    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // 원래 색상 저장
        foreach (Renderer rend in renderers)
        {
            Color[] colors = new Color[rend.materials.Length];
            for (int i = 0; i < rend.materials.Length; i++)
            {
                colors[i] = rend.materials[i].color;
            }
            originalColors.Add(colors);
        }

        SetRed();
    }


    public void SetRed(float color = 0.6f)
    { redColor = new Color(1f, color, color); }


    // 빨간색으로 변경
    public void ChangeToRed()
    {
        foreach (Renderer rend in renderers)
        {
            if (rend == null) { continue; }
            foreach (var mat in rend.materials)
            {
                Color color = redColor;
                color.a = mat.color.a;
                mat.color = color;
            }
        }
    }

    // 원래 색상으로 되돌리기
    public void RestoreOriginalColors()
    {
        for (int r = 0; r < renderers.Length; r++)
        {
            if (renderers[r] == null) { continue; }

            Renderer rend = renderers[r];
            Color[] saved = originalColors[r];

            for (int m = 0; m < rend.materials.Length; m++)
            {
                if (rend.materials[m] == null) { return; }
                Color color = saved[m];
                color.a = rend.materials[m].color.a;
                rend.materials[m].color = color;
            }
        }
    }


}

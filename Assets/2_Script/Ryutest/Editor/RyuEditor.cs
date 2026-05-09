using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RYURYU))]
public class RyuEditor : Editor
{
    private RYURYU m_Target = null;
    private void OnEnable()
    {
        m_Target = (RYURYU)target;
    }
    public override void OnInspectorGUI()
    {

        EditorGUILayout.Space(100);
        m_Target.Test = EditorGUILayout.IntField("테스트 변수", m_Target.Test);
    }

    
}

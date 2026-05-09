using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class ReadOnlyAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;  // 비활성화해서 수정 불가로 만듦
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;   // 다시 활성화 상태로 복구
    }
}
#endif
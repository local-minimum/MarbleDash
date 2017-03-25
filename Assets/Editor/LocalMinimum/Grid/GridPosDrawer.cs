using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LocalMinimum.Grid
{

    [CustomPropertyDrawer(typeof(GridPos))]
    public class GridPosDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            SerializedProperty xProp = property.FindPropertyRelative("x");
            SerializedProperty yProp = property.FindPropertyRelative("y");
            EditorGUI.indentLevel = 0;
            contentRect.width *= 0.7f;
            contentRect.width = (contentRect.width) / 2;
            EditorGUIUtility.labelWidth = 14f;
            xProp.intValue = EditorGUI.IntField(contentRect, new GUIContent("X"), xProp.intValue);
            contentRect.x += contentRect.width;
            yProp.intValue = EditorGUI.IntField(contentRect, new GUIContent("Y"), yProp.intValue);
            EditorGUI.EndProperty();
        }
    }


}
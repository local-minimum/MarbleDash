using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BallPath))]
public class BallPathEditor : Editor {
    public override void OnInspectorGUI()
    {
        var prop = serializedObject.FindProperty("boardGrid");

        prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, "Settings");

        if (prop.isExpanded)
        {
            EditorGUI.indentLevel++;
            base.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }

        if (GUILayout.Button("Generate New"))
        {
            BallPath myTarget = target as BallPath;
            myTarget.GeneratePath(serializedObject.FindProperty("connectPrevious").boolValue);
           
        }
    }
}

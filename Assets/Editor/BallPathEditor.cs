using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BallPath))]
public class BallPathEditor : Editor {
    public override void OnInspectorGUI()
    {
        //Just pick one that is not a list
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

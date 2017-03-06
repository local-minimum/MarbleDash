using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BallPath))]
public class BallPathEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate New"))
        {
            BallPath myTarget = target as BallPath;
            myTarget.GeneratePath(serializedObject.FindProperty("connectPrevious").boolValue);
           
        }
    }
}

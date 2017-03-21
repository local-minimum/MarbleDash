using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Destructable), false)]
public class DestructableEditor : Editor {

    public override void OnInspectorGUI()
    {
        if (EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth")))
        {
            serializedObject.ApplyModifiedProperties();
        }
        base.OnInspectorGUI();
    }
}

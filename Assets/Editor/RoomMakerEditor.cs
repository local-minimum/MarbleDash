using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomMaker))]
public class RoomMakerEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Rooms"))
        {
            (target as RoomMaker).GenerateRooms();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace LocalMinimum.Boolean.Editor
{
    [CustomEditor(typeof(ArrayRepresentation))]
    public class ArrayRepresentationEditor : UnityEditor.Editor
    {
        bool edgeBorderAsEdge;
        bool distanceEdgeBorderAsEdge;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ArrayRepresentation myTaget = target as ArrayRepresentation;

            if (GUILayout.Button("Make array / place elements")) {
                myTaget.Generate();
            }

            if (GUILayout.Button("Invert (bool)"))
            {
                myTaget.Invert();
            }

            EditorGUILayout.BeginHorizontal();
            edgeBorderAsEdge = GUILayout.Toggle(edgeBorderAsEdge, "Border is edge");
            if (GUILayout.Button("Edge (bool)"))
            {
                myTaget.Edge(edgeBorderAsEdge);
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            distanceEdgeBorderAsEdge = GUILayout.Toggle(distanceEdgeBorderAsEdge, "Border is edge");
            if (GUILayout.Button("Dist to Edge (bool -> int)"))
            {
                myTaget.DistanceToEdge(distanceEdgeBorderAsEdge);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Label (bool -> int)"))
            {
                myTaget.Label();
            }

        }
    }

}
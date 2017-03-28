using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugMesher : MonoBehaviour {

#if UNITY_EDITOR

    [SerializeField]
    Texture2D[] textures;

    Mesh[] positiveNumberMeshes;
    Mesh negativeOneMesh;
    Mesh plusMesh;
    Mesh yesMesh;
    Mesh noMesh;

    private void Awake()
    {
        //TODO this makes no sense what am i doing
        GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Mesh>();
    }

    public Mesh GetMesh(int value)
    {
        if (value < 0)
        {
            return negativeOneMesh;
        } else if (value < positiveNumberMeshes.Length)
        {
            return positiveNumberMeshes[value];
        } else
        {
            return plusMesh;
        }
    }

    public Mesh GetMesh(bool value)
    {
        if (value)
        {
            return yesMesh;
        } else
        {
            return noMesh;
        }
    }

#endif    
}

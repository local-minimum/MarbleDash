using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.Mesh {

    [System.Serializable]
    public struct MeshDataMinimal {

        public int[] tris;
        public Vector3[] verts;
        public Vector2[] uv;
        public Vector2[] uv2;
        public Vector2[] uv3;
        public Vector2[] uv4;
        bool createdFromMesh;
        int meshId;
        int meshHash;

        public MeshDataMinimal(UnityEngine.Mesh m)
        {
            tris = m.triangles;
            verts = m.vertices;
            uv = m.uv;
            uv2 = m.uv2;
            uv3 = m.uv3;
            uv4 = m.uv4;
            meshId = m.GetInstanceID();
            meshHash = m.GetHashCode();
            createdFromMesh = true;

        }

        public bool SyncData(UnityEngine.Mesh m)
        {
            if (!createdFromMesh && m.GetInstanceID() != meshId && m.GetHashCode() != meshHash)
            {
                tris = m.triangles;
                verts = m.vertices;
                uv = m.uv;
                uv2 = m.uv2;
                uv3 = m.uv3;
                uv4 = m.uv4;
                meshId = m.GetInstanceID();
                meshHash = m.GetHashCode();
                createdFromMesh = true;

                return true;
            }
            return false;
        }
    }

}
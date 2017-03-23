using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootFarts : MonoBehaviour {

    [SerializeField]
    Renderer fartTarget;

    private void Awake()
    {
        //Just to be sure for now
        Material m = fartTarget.material;
        fartTarget.material = Instantiate(m);
    }


    private void OnCollisionStay(Collision collision)
    {
        ContactPoint cPoint = collision.contacts[0];
        Renderer r = collision.gameObject.GetComponent<Renderer>();

        Color c = GetUVColor(cPoint, r.material);
        
        fartTarget.material.color = c;
    }

    static Color GetUVColor(ContactPoint cPoint, Material mat)
    {
        Mesh m = cPoint.otherCollider.GetComponent<MeshFilter>().mesh;
        Vector3 otherLocalPt = cPoint.otherCollider.transform.InverseTransformPoint(cPoint.point);
        int[] closest = GetTwoClosestPoints(m.vertices, otherLocalPt);
        int bestTri = BestTriStart(m.triangles, m.vertices, closest[0], closest[1], otherLocalPt);
        Vector2 uvPos = GetUVCoordinate(m.triangles, m.uv, m.vertices, otherLocalPt, bestTri);
        Texture2D tex = (mat.mainTexture as Texture2D);
        return tex.GetPixel(Mathf.FloorToInt(uvPos.x * tex.width), Mathf.FloorToInt(uvPos.y * tex.height));
    }

    static int[] GetTwoClosestPoints(Vector3[] verts, Vector3 pt)
    {
        int closest = -1;
        int second = -1;
        float sqDistClosest = 0;
        float sqDistSecond = 0;

        for (int i = 0; i < verts.Length; i++)
        {
            float curSqDist = Vector3.SqrMagnitude(verts[i] - pt);
            if (curSqDist < sqDistClosest || closest < 0)
            {
                sqDistSecond = sqDistClosest;
                second = closest;
                sqDistClosest = curSqDist;
                closest = i;
            } else if (curSqDist < sqDistSecond || second < 0)
            {
                second = i;
                sqDistSecond = curSqDist;
            }

        }

        return new int[] { closest, second };
    }

    static int BestTriStart(int[] tris, Vector3[] verts, int closest, int second, Vector3 pt)
    {
        float sqDist = 0f;
        int best = -1;
        for (int i = 0; i < tris.Length; i += 3)
        {
            if (tris[i] == closest && tris[i + 1] == second || tris[i + 1] == closest && tris[i] == second)
            {
                float cur = Vector3.SqrMagnitude(verts[tris[i + 2]] - pt);
                if (cur < sqDist || best < 0)
                {
                    sqDist = cur;
                    best = i;
                }
            } else if (tris[i] == closest && tris[i + 2] == second || tris[i + 2] == closest && tris[i] == second)
            {
                float cur = Vector3.SqrMagnitude(verts[tris[i + 1]] - pt);
                if (cur < sqDist || best < 0)
                {
                    sqDist = cur;
                    best = i;
                }
            }
            else if (tris[i + 2] == closest && tris[i + 1] == second || tris[i + 1] == closest && tris[i + 2] == second)
            {
                float cur = Vector3.SqrMagnitude(verts[tris[i]] - pt);
                if (cur < sqDist || best < 0)
                {
                    sqDist = cur;
                    best = i;
                }
            }
        }
        return best;
    }

    static Vector2 GetUVCoordinate(int[] tris, Vector2[] uvs, Vector3[] verts, Vector3 pt, int bestTri)
    {
        Vector2 a = uvs[tris[bestTri]];
        Vector2 b = uvs[tris[bestTri + 1]];
        Vector2 c = uvs[tris[bestTri + 2]];

        Vector2 ba = b - a;
        Vector2 ca = c - a;

        Vector3 A = verts[tris[bestTri]];
        Vector3 B = verts[tris[bestTri + 1]];
        Vector3 C = verts[tris[bestTri + 2]];

        Vector3 BA = B - A;
        Vector3 CA = C - A;
        Vector3 ptA = pt - A;

        float dBA = Vector3.Dot(ptA, BA) / BA.magnitude;
        float dCA = Vector3.Dot(ptA, CA) / CA.magnitude;

        return a + (ba * dBA + ca * dCA);
    }
}


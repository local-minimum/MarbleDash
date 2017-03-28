using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Mesh;

public class FootFarts : MonoBehaviour {

    [SerializeField]
    Renderer fartTarget;

    [SerializeField]
    ParticleSystem ps;

    [SerializeField]
    int particles;

    [SerializeField]
    float emitProb = 0.5f;
    
    private void Awake()
    {
        if (fartTarget)
        {
            //Just to be sure for now
            Material m = fartTarget.material;
            fartTarget.material = Instantiate(m);
        }
    }

    ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint cPoint = collision.contacts[0];
        Renderer r = collision.gameObject.GetComponent<Renderer>();

        Color c = GetUVColor(cPoint, r.material);

        if (fartTarget)
        {
            fartTarget.material.color = c;
        }

        if (ps && Random.value < emitProb)
        {
            emitParams.startColor = c;
            ps.Emit(emitParams, particles);
        }
    }

    Color GetUVColor(ContactPoint cPoint, Material mat)
    {
        Mesh m = cPoint.otherCollider.GetComponent<MeshFilter>().mesh;
        Vector3 otherLocalPt = cPoint.otherCollider.transform.InverseTransformPoint(cPoint.point);
        int[] closest = GeometryTools.GetTwoClosestVertsToPoint(m.vertices, otherLocalPt);
        int bestTri = GeometryTools.GetClosestTriStartIndex(m.triangles, m.vertices, closest[0], closest[1], otherLocalPt);
        Vector2 uvPos = GeometryTools.TranslateMeshPointToUV(m.triangles, m.uv, m.vertices, otherLocalPt, bestTri);
        Texture2D tex = (mat.mainTexture as Texture2D);
        return tex.GetPixel(Mathf.FloorToInt(uvPos.x * tex.width), Mathf.FloorToInt(uvPos.y * tex.height));
    }

}


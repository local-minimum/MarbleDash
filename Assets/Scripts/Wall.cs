using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    GridPos pos;
    BoardGrid board;
    Occupancy wallType;

    public void SetPosition(BoardGrid board, GridPos pos)
    {
        gameObject.SetActive(true);
        this.board = board;
        this.pos = pos;
        transform.localPosition = board.GetLocalPosition(pos);
        wallType = board.GetOccupancy(pos);
        Generate();
    }

    [SerializeField]
    Mesh m;

    int[,] context;

    void Generate()
    {
        Vector2 shape = board.TileShape;
        context = board.GetOccupancyContext(pos, Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory);

        if (m == null)
        {
            m = new Mesh();
            m.name = "Generated Wall Segment";
            GetComponent<MeshFilter>().mesh = m;
           
        }
        else
        {
            m.Clear();
        }

        List<int> tris;
        List<Vector3> verts = GetVerts(shape, out tris);
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.SetUVs(0, GetUV(shape, verts));
        m.RecalculateBounds();
        m.RecalculateNormals();
    }

    [SerializeField]
    float height = 1f;

    [SerializeField, Range(0, 1)]
    float innerSquareTop = .75f;

    [SerializeField, Range(0, 1)]
    float innerSquareBase = .8f;

    List<Vector3> GetVerts(Vector2 shape, out List<int> tris)
    {
        float halfX = shape.x / 2f;
        float halfZ = shape.y / 2f;
        float halfInnerBase = innerSquareBase / 2f;
        float halfInnerTop = innerSquareTop / 2f;
        List<Vector3> verts = new List<Vector3>();
        tris = new List<int>();       

        List<int> partTris;
        verts.AddRange(Center(halfX, halfZ, halfInnerTop, halfInnerBase, out partTris));
        tris.AddRange(partTris);
        

        //Connect to north;
        if (context[1, 0] == 1 || true)
        {
            
            verts.AddRange(NorthOut(true, verts.Count, halfX, halfZ, halfInnerTop, halfInnerBase, out partTris));
            tris.AddRange(partTris);
        }

        if (context[1, 2] == 1 || true)
        {
            verts.AddRange(SouthOut(true, verts.Count, halfX, halfZ, halfInnerTop, halfInnerBase, out partTris));
            tris.AddRange(partTris);
        }

        if (context[0, 1] == 1 || true)
        {
            verts.AddRange(WestOut(true, verts.Count, halfX, halfZ, halfInnerTop, halfInnerBase, out partTris));
            tris.AddRange(partTris);
        }

        if (context[0, 1] == 1 || true)
        {
            verts.AddRange(EastOut(true, verts.Count, halfX, halfZ, halfInnerTop, halfInnerBase, out partTris));
            tris.AddRange(partTris);
        }

        return verts;
    }

    List<Vector3> Center(float halfX, float halfZ, float halfInnerTop, float halfInnerBase, out List<int> tris)
    {
        List<Vector3> verts = new List<Vector3>();
        tris = new List<int>();

        verts.Add(new Vector3(halfX * halfInnerBase, 0, halfZ * halfInnerBase));    //0
        verts.Add(new Vector3(halfX * halfInnerTop, height, halfZ * halfInnerTop)); //1

        verts.Add(new Vector3(halfX * -halfInnerBase, 0, halfZ * halfInnerBase));   //2
        verts.Add(new Vector3(halfX * -halfInnerTop, height, halfZ * halfInnerTop));//3

        verts.Add(new Vector3(halfX * -halfInnerBase, 0, -halfZ * halfInnerBase));   //4
        verts.Add(new Vector3(halfX * -halfInnerTop, height, -halfZ * halfInnerTop));//5

        verts.Add(new Vector3(halfX * halfInnerBase, 0, -halfZ * halfInnerBase));    //6
        verts.Add(new Vector3(halfX * halfInnerTop, height, -halfZ * halfInnerTop)); //7

        tris.AddRange(new int[]{
            3, 1, 5,
            1, 7, 5                
                });

        return verts;
    }

    List<Vector3> NorthOut(bool capped, int index, float halfX, float halfZ, float halfInnerTop, float halfInnerBase, out List<int> tris)
    {
        List<Vector3> verts = new List<Vector3>();
        tris = new List<int>();
        verts.Add(new Vector3(halfX * halfInnerTop, height, halfZ));                //0
        verts.Add(new Vector3(halfX * halfInnerBase, 0, halfZ));                    //1
        verts.Add(new Vector3(halfX * -halfInnerTop, height, halfZ));               //2
        verts.Add(new Vector3(halfX * -halfInnerBase, 0, halfZ));                   //3

        tris.AddRange(new int[]{
                index + 0, index + 1, 0,
                index + 0, 0, 1,

                index + 2, 2, index + 3,
                index + 2, 3, 2,

                index + 0, 1, 3,
                index + 0, 3, index + 2
                });

        if (capped)
        {
            tris.AddRange(new int[]
            {
                index + 1, index + 0, index + 2,
                index + 2, index + 3, index + 1
            });
        }

        return verts;
    }

    List<Vector3> SouthOut(bool capped, int index, float halfX, float halfZ, float halfInnerTop, float halfInnerBase, out List<int> tris)
    {
        List<Vector3> verts = new List<Vector3>();
        tris = new List<int>();
        verts.Add(new Vector3(halfX * halfInnerTop, height, -halfZ));               //0
        verts.Add(new Vector3(halfX * halfInnerBase, 0, -halfZ));                   //1
        verts.Add(new Vector3(halfX * -halfInnerTop, height, -halfZ));              //2
        verts.Add(new Vector3(halfX * -halfInnerBase, 0, -halfZ));                  //3

        tris.AddRange(new int[]{
                index + 0, 6, index + 1,
                index + 0, 7, 6,

                index + 2, index + 3, 4,
                index + 2, 4, 5,

                index + 0, 5, 7,
                index + 0, index + 2, 5
                });

        if (capped)
        {
            tris.AddRange(new int[]
            {
                index + 1, index + 2, index + 0,
                index + 2, index + 1, index + 3
            });
        }

        return verts;
    }

    List<Vector3> WestOut(bool capped, int index, float halfX, float halfZ, float halfInnerTop, float halfInnerBase, out List<int> tris)
    {
        List<Vector3> verts = new List<Vector3>();
        tris = new List<int>();
        verts.Add(new Vector3(-halfX, height, halfZ * halfInnerTop));               //0
        verts.Add(new Vector3(-halfX, 0, halfZ * halfInnerBase));                   //1
        verts.Add(new Vector3(-halfX, height, -halfZ * halfInnerTop));              //2
        verts.Add(new Vector3(-halfX, 0, -halfZ * halfInnerBase));                  //3

        tris.AddRange(new int[]{
                index + 0, index + 1, 2,
                index + 0, 2, 3,

                index + 2, 4, index + 3,
                index + 2, 5, 4,

                index + 0, 3, 5,
                index + 0, 5, index + 2,
                });

        if (capped)
        {
            tris.AddRange(new int[]
            {
                index + 1, index + 0, index + 2,
                index + 2, index + 3, index + 1
            });
        }

        return verts;
    }

    List<Vector3> EastOut(bool capped, int index, float halfX, float halfZ, float halfInnerTop, float halfInnerBase, out List<int> tris)
    {
        List<Vector3> verts = new List<Vector3>();
        tris = new List<int>();
        verts.Add(new Vector3(halfX, height, halfZ * halfInnerTop));               //0
        verts.Add(new Vector3(halfX, 0, halfZ * halfInnerBase));                   //1
        verts.Add(new Vector3(halfX, height, -halfZ * halfInnerTop));              //2
        verts.Add(new Vector3(halfX, 0, -halfZ * halfInnerBase));                  //3

        tris.AddRange(new int[]{
                index + 0, 0, index + 1,
                index + 0, 1, 0,

                index + 2, index + 3, 6,
                index + 2, 6, 7,

                index + 0, 7, 1,
                index + 0, index + 2, 7
                });

        if (capped)
        {
            tris.AddRange(new int[]
            {
                index + 1, index + 2, index + 0,
                index + 2, index + 1, index + 3
            });
        }

        return verts;
    }

    List<Vector2> GetUV(Vector2 shape, List<Vector3> verts)
    {
        List<Vector2> uvs = new List<Vector2>();

        for (int i=0, l=verts.Count; i<l; i++)
        {
            uvs.Add(new Vector2(verts[i].x + shape.x / 2, verts[i].z / shape.y / 2));
        }

        return uvs;

    }
}

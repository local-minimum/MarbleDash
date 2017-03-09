using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPathTile : MonoBehaviour {

    MeshFilter mf;
    Mesh m;

    private void Start()
    {
        mf = GetComponent<MeshFilter>();
        m = new Mesh();
        mf.sharedMesh = m;
    }

    BoardGrid board;
    GridPos pos;
    Vector2 size;

    public void SetPosition(BoardGrid board, GridPos pos)
    {
        this.board = board;
        this.pos = pos;
        size = board.TileShape;
        transform.position = board.GetLocalPosition(pos);
    }

    BallPathTile nextTile;
    BallPathTile previousTile;

    public void SetNextTile(BallPathTile next)
    {
        nextTile = next;
        next.SetPreviousTile(this);
    }

    void SetPreviousTile(BallPathTile prev)
    {
        previousTile = prev;
    }

    public void GenerateMesh()
    {
        float halfX = size.x / 2f;
        float halfZ = size.y / 2f;
        Direction exit = (nextTile.pos - pos).AsMajorDirection();
        Direction entry = (previousTile.pos - pos).AsMajorDirection();

        if (previousTile == null)
        {
            m.Clear();
            m.vertices = new Vector3[]
            {
                new Vector3(-halfX, 0, halfZ),
                new Vector3(halfX, 0, halfZ),
                new Vector3(halfX, 0, -halfZ),
                new Vector3(-halfX, 0, -halfZ),

            };
            m.triangles = new int[]
            {
                0, 1, 3,
                1, 2, 3,
            };

            if (exit == Direction.North)
            {
                m.uv = new Vector2[]
                {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0)
                };
            } else if (exit == Direction.South)
            {
                m.uv = new Vector2[]
                {
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            } else if (exit == Direction.East)
            {
                m.uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)

                };
            } else if (exit == Direction.West)
            {
                m.uv = new Vector2[]
                {
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1)

                };
            }
        }
    }
}

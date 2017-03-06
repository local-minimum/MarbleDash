using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Occupancy { Free, BallPathSource, BallPathTarget, BallPath, Wall, WallBreakable, WallIllusory, Hole};

public struct GridPos
{
    public int x;
    public int y;

    public GridPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void RotateCW()
    {
        int tmp = x;
        x = y;
        y = -tmp;
    }

    public void RotateCCW()
    {
        int tmp = x;
        x = -y;
        y = tmp;
    }

    public void Rotate180()
    {
        x = -x;
        y = -y;
    }

    public static bool operator ==(GridPos a, GridPos b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(GridPos a, GridPos b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static GridPos operator +(GridPos a, GridPos b)
    {
        return new GridPos(a.x + b.x, a.y + b.y);
    }
    
    public static GridPos operator -(GridPos a, GridPos b)
    {
        return new GridPos(a.x - b.x, a.y - b.y);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public static int TaxiCabDistance(GridPos a, GridPos b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}

[ExecuteInEditMode]
public class BoardGrid : MonoBehaviour {

    [SerializeField]
    Transform target;

    [SerializeField]
    int size = 10;

    public int Size
    {
        get
        {
            return size;
        }
    }

    [SerializeField]
    Occupancy[,] gridOccupancy;

    Vector3 targetSize;

    Vector3 targetLocalOffset;

    void Start() {
        gridOccupancy = new Occupancy[size, size];
        targetSize = target.InverseTransformVector(target.GetComponent<Collider>().bounds.size);
        targetLocalOffset = Vector3.forward * targetSize.z;
        targetSize.z = 0;
        targetLocalOffset = targetLocalOffset - targetSize / 2f;
    }

    public Vector2 TileShape
    {
        get
        {
            return new Vector2(targetSize.x, targetSize.y) / size;
        }
    }

    public Vector3 GetLocalPosition(int x, int y)
    {
        return targetLocalOffset + new Vector3(targetSize.x * (x + 0.5f) / size, targetSize.y * (y + 0.5f) / size);
    }

    public Vector3 GetLocalPosition(GridPos pos)
    {
        return GetLocalPosition(pos.x, pos.y);
    }

    public Vector3 GetWorldPosition(GridPos pos)
    {
        return target.TransformPoint(GetLocalPosition(pos.x, pos.y));
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return target.TransformPoint(GetLocalPosition(x, y));

    }

    public GridPos RandomPosition
    {
        get
        {
            return new GridPos(Random.Range(0, size), Random.Range(0, size));
        }
    }

    public void Occupy(int x, int y, Occupancy occupancy)
    {
        gridOccupancy[x, y] = occupancy;
    }

    public void Occupy(GridPos pos, Occupancy occupancy)
    {
        gridOccupancy[pos.x, pos.y] = occupancy;
    }

    public void Free(int x, int y)
    {
        gridOccupancy[x, y] = Occupancy.Free;
    }

    public void Free(GridPos pos)
    {
        gridOccupancy[pos.x, pos.y] = Occupancy.Free;
    }

    public void FreeAll()
    {
        if (gridOccupancy == null || gridOccupancy.GetLength(0) != size)
        {
            gridOccupancy = new Occupancy[size, size];
        }

        for (int x=0; x<size; x++)
        {
            for (int y=0; y<size; y++)
            {
                gridOccupancy[x, y] = Occupancy.Free;
            }
        }
    }

    public bool IsFree(GridPos pos)
    {
        return gridOccupancy[pos.x, pos.y] == Occupancy.Free;
    }

    public Occupancy GetOccupancy(GridPos pos)
    {
        return gridOccupancy[pos.x, pos.y];
    }

    public IEnumerable<GridPos> Find(Occupancy occupancy)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (gridOccupancy[x, y] == occupancy)
                {
                    yield return new GridPos(x, y);
                }
            }
        }
    }

    public enum Neighbourhood { Cross, Eight};
    
    public IEnumerable<GridPos> Neighbours(GridPos pos, Neighbourhood neighbourhood)
    {
        int yMax = pos.y + 1;
        for (int x = pos.x - 1, xMax = pos.x + 1, xOff = -1; xOff < 2; x++, xOff++)
        {
            for (int y = pos.y - 1, yOff = -1; yOff < 2; y++, yOff++)
            {
                if (Mathf.Abs(xOff) == Mathf.Abs(yOff) && (neighbourhood == Neighbourhood.Cross || xOff == 0))
                {
                    continue;
                } else if (x < 0 || y < 0 || x >= size || y >= size)
                {
                    continue;
                }
                yield return new GridPos(x, y);
            }
        }
    } 

    private void OnDrawGizmosSelected()
    {
        float halfSize = (size + 1) / 2.0f;
        Gizmos.color = Color.red;
        for (int x=0; x<size; x++)
        {

            for (int y=0; y<size; y++)
            {
                Gizmos.DrawWireSphere(GetWorldPosition(x, y), 10f / size);
            }

        }
    }
}

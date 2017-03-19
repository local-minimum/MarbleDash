using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Occupancy { Free, BallPathSource, BallPathTarget, BallPath, Wall, WallBreakable, WallIllusory, Hole, Obstacle, Enemy, Player};
public enum Direction { None, North, South, West, East };

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

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public Direction AsMajorDirection()
    {
        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            return x > 0 ? Direction.East : Direction.West;
        } else if (y != 0)
        {
            return y > 0 ? Direction.North : Direction.South;
        } else
        {
            return Direction.None;
        }
    }

    public static int TaxiCabDistance(GridPos a, GridPos b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public int EightMagnitude
    {
        get
        {
            return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
        }
    }

    public static int ShortestDimension(GridPos a, GridPos b)
    {
        return Mathf.Min(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    public GridPos NineNormalized
    {
        get
        {
            return new GridPos(x > 0 ? 1 : (x < 0 ? -1 : 0), y > 0 ? 1 : (y < 0 ? -1 : 0));
        }
    }

    public bool IsZero()
    {
        return x == 0 && y == 0;
    }
}

[ExecuteInEditMode]
public class BoardGrid : MonoBehaviour {

    [SerializeField]
    Transform target;

    [SerializeField]
    int size = 10;

    [SerializeField]
    Transform solidTileParent;

    [SerializeField]
    Transform holesTileParent;

    [SerializeField]
    Transform solidTilePrefab;

    [SerializeField]
    Transform holesTilePrefab;

    public int Size
    {
        get
        {
            return size;
        }
    }

    int[,] gridOccupancy;

    [SerializeField]
    Vector3 targetSize;

    [SerializeField]
    Vector3 targetLocalOffset;

    static BoardGrid _instance;
    public static BoardGrid instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BoardGrid>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start() {
        gridOccupancy = new int[size, size];
        /*
        targetSize = target.InverseTransformVector(target.GetComponent<Collider>().bounds.size);
        targetLocalOffset = Vector3.forward * targetSize.z;
        targetSize.x = Mathf.Abs(targetSize.x);
        targetSize.y = Mathf.Abs(targetSize.y);
        targetSize.z = 0;
        targetLocalOffset = targetLocalOffset - targetSize / 2f;
        */
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

    public GridPos Clamp(GridPos pos)
    {
        return new GridPos(Mathf.Min(Mathf.Max(0, pos.x), size - 1), Mathf.Min(Mathf.Max(0, pos.y), size - 1));
    }

    public bool IsValidPosition(GridPos pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < size && pos.y < size;
    }

    public void Occupy(int x, int y, Occupancy occupancy)
    {
        if (occupancy == Occupancy.Free)
        {
            Free(x, y);
        }
        else
        {
            if (gridOccupancy[x, y] == (1 << (int)Occupancy.Free))
            {
                gridOccupancy[x, y] = 1 << (int)occupancy;
            }
            else
            {
                gridOccupancy[x, y] |= 1 << (int)occupancy;
            }
        }
    
    }

    public void Occupy(GridPos pos, Occupancy occupancy)
    {
        Occupy(pos.x, pos.y, occupancy);
    }

    public void Free(int x, int y)
    {
        gridOccupancy[x, y] = 1 << (int) Occupancy.Free;
    }

    public void Free(GridPos pos)
    {
        gridOccupancy[pos.x, pos.y] = 1 << (int) Occupancy.Free;
    }

    public void Free(GridPos pos, Occupancy filt)
    {
        if (filt == Occupancy.Free)
        {
            throw new System.ArgumentException("Can't free free on " + pos);
        }

        int val = gridOccupancy[pos.x, pos.y];
        int filtVal = 1 << (int)filt;
        if ((val & filtVal) != filtVal)
        {
            Debug.LogWarning(string.Format("Pos {0} doesn't have filter {1} set. Nothing to free.", pos, filt));
        } else
        {
            gridOccupancy[pos.x, pos.y] = val & (~filtVal);
        }
    }

    public void FreeAll()
    {
        if (gridOccupancy == null || gridOccupancy.GetLength(0) != size)
        {
            gridOccupancy = new int[size, size];
        }

        int val = 1 << (int)Occupancy.Free;
        for (int x=0; x<size; x++)
        {
            for (int y=0; y<size; y++)
            {
                gridOccupancy[x, y] = val;
            }
        }
    }

    public bool IsFree(GridPos pos)
    {
        //Debug.Log((int) gridOccupancy[pos.x, pos.y]);
        return gridOccupancy[pos.x, pos.y] == (1 << (int) Occupancy.Free);
    }

    public bool IsFree(int x, int y)
    {
        return gridOccupancy[x, y] == (1 << (int) Occupancy.Free);
    }

    public List<Occupancy> GetOccupancy(GridPos pos)
    {
        List<Occupancy> ret = new List<Occupancy>();
        int posVal = gridOccupancy[pos.x, pos.y];
        foreach (int val in System.Enum.GetValues(typeof(Occupancy)))
        {
            if ((posVal & (1 << val)) != 0)
            {

                ret.Add((Occupancy)val);
            }
        }
        return ret;
    }

    public bool HasOccupancy(GridPos pos, Occupancy filt)
    {
        return ((gridOccupancy[pos.x, pos.y] & (1 << (int)filt))) != 0;
    }

    public IEnumerable<GridPos> Find(Occupancy occupancy)
    {
        int filt = 1 << (int)occupancy;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if ((gridOccupancy[x, y] & filt) != 0)
                {
                    yield return new GridPos(x, y);
                }
            }
        }
    }

    public IEnumerable<GridPos> FindIsOnlyAny(params Occupancy[] occupancy)
    {

        int filt = 1 << (int)occupancy[0];
        for (int i=1; i<occupancy.Length; i++)
        {
            filt |= 1 << (int)occupancy[i];
        }
        int notFilt = ~filt;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if ((gridOccupancy[x, y] & notFilt) == 0)
                {
                    yield return new GridPos(x, y);
                }
            }
        }
    }

    public enum Neighbourhood { Cross, Eight};
    
    public IEnumerable<GridPos> Neighbours(GridPos pos, Neighbourhood neighbourhood)
    {
      
        for (int x = pos.x - 1, xOff = -1; xOff < 2; x++, xOff++)
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

    public int[,] GetOccupancyNonFree(GridPos pos)
    {
        int mask = 1 <<  (int) Occupancy.Free;    
        int[,] ret = new int[3, 3];
        for (int yOff = -1; yOff < 2; yOff++)
        {
            for (int xOff = -1; xOff < 2; xOff++)
            {
                GridPos cur = pos + new GridPos(xOff, yOff);
                if (IsValidPosition(cur))
                {
                    ret[xOff + 1, yOff + 1] = (gridOccupancy[cur.x, cur.y] & mask) == 0 ? 1 : 0;
                }
                else
                {
                    ret[xOff + 1, yOff + 1] = -1;
                }
            }
        }
        return ret;
    }

    public int[,] GetOccupancyContext(GridPos pos, params Occupancy[] filter)
    {
        int mask = 1 << (int)filter[0];
        for (int i = 0; i < filter.Length; i++)
        {
            mask |= 1 << (int)filter[i];
        }
        int[,] ret = new int[3, 3];
        for (int yOff = -1; yOff < 2; yOff++)
        {
            for (int xOff = -1; xOff < 2; xOff++)
            {
                GridPos cur = pos + new GridPos(xOff, yOff);
                if (IsValidPosition(cur))
                {
                    ret[xOff + 1, yOff + 1] = (gridOccupancy[cur.x, cur.y] & mask) != 0 ? 1 : 0;
                }
                else
                {
                    ret[xOff + 1, yOff + 1] = -1;
                }
            }
        }
        return ret;
    }

    public int[,] GetNotOccupancyContext(GridPos pos, params Occupancy[] filter)
    {
        int mask = 1 << (int)filter[0];
        for (int i = 0; i < filter.Length; i++)
        {
            mask |= 1 << (int)filter[i];
        }
        int[,] ret = new int[3, 3];
        for (int yOff = -1; yOff < 2; yOff++)
        {
            for (int xOff = -1; xOff < 2; xOff++)
            {
                GridPos cur = pos + new GridPos(xOff, yOff);
                if (IsValidPosition(cur))
                {
                    ret[xOff + 1, yOff + 1] = (gridOccupancy[cur.x, cur.y] & mask) != 0 ? 0 : 1;
                }
                else
                {
                    ret[xOff + 1, yOff + 1] = -1;
                }
            }
        }
        return ret;
    }

    public static int CountContextOccupied(int[,] context, bool includeCenter, bool countExtierior)
    {
        int count = 0;
        for (int x = 0; x < 3; x++)
        {
            for (int y =0; y< 3; y++)
            {
                if (x == y && x == 1 && !includeCenter)
                {
                    continue;
                }
                if (context[x, y] > 0 || countExtierior && context[x, y] == -1)
                {
                    count++;
                } 
            }
        }

        return count;
    }

    public static List<GridPos> ContextToOffsets(int[,] context)
    {
        List<GridPos> relPositions = new List<GridPos>();
        for (int yOff = -1; yOff < 2; yOff++)
        {
            for (int xOff = -1; xOff < 2; xOff++)
            {
                if (xOff == 0 & yOff == 0)
                {
                    continue;
                }

                if (context[xOff + 1, yOff + 1] == 1)
                {
                    relPositions.Add(new GridPos(xOff, yOff));
                }
            }
        }

        return relPositions;
    }

    [SerializeField, Range(-1, 1)]
    float heightOffset = -0.1f;

    public void ConstructFloor()
    {
        InactivatePreviousTiles();
        Vector3 localScale = TileShape;
        localScale.x /= 2f;
        localScale.y /= 2f;
        localScale.z = 1;
        int holeFilt = 1 << (int) Occupancy.Hole;
        for (int x=0; x<size; x++)
        {
            for (int y=0; y<size; y++)
            {
                Transform t;
                TileType tType;
                if ((gridOccupancy[x, y] &  holeFilt) != 0)
                {
                    t = GetNextHole();
                    tType = TileType.Hole;
                } else
                {
                    t = GetNextSolid();
                    tType = TileType.Solid;
                }

                t.GetComponent<BoardTile>().SetPosition(this, new GridPos(x, y), heightOffset, tType);
                t.localScale = localScale;
            }
        }
    }

    int nextHoleIndex = 0;

    Transform GetNextHole()
    {
        Transform t;
        if (nextHoleIndex < holesTileParent.childCount)
        {
            t = holesTileParent.GetChild(nextHoleIndex);
            t.gameObject.SetActive(true);
        } else
        {
            t = Instantiate(holesTilePrefab, holesTileParent, false);
        }
        nextHoleIndex++;
        return t;
    }

    int nextSolidIndex = 0;
    Transform GetNextSolid()
    {
        Transform t;
        if  (nextSolidIndex < solidTileParent.childCount)
        {
            t = solidTileParent.GetChild(nextSolidIndex);
            t.gameObject.SetActive(true);
        } else
        {
            t = Instantiate(solidTilePrefab, solidTileParent, false);
        }

        nextSolidIndex++;
        return t;
    }

    void InactivatePreviousTiles()
    {
        nextHoleIndex = 0;
        nextSolidIndex = 0;
        for (int i=0, l=solidTileParent.childCount; i<l; i++)
        {
            solidTileParent.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0, l = holesTileParent.childCount; i < l; i++)
        {
            holesTileParent.GetChild(i).gameObject.SetActive(false);
        }

    }

    [SerializeField]
    bool drawGizmos = false;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
        {
            return;
        }

        Gizmos.color = Color.red;
        for (int x=0; x<size; x++)
        {

            for (int y=0; y<size; y++)
            {
                Gizmos.DrawWireSphere(GetWorldPosition(x, y), 10f / size);
            }

        }
    }

    public void DebugPosition(GridPos pos, params Occupancy[] filter)
    {
        int[,] context = GetOccupancyContext(pos, filter);
        var msg = string.Format("Context around {0}:\n", pos);

        for (int y = 2; y > -1; y--)
        {
            for (int x = 0; x < 3; x++)
            {
                msg += context[x, y];

                if (x == 2)
                {
                    msg += "\n";
                }
                else
                {
                    msg += ", ";
                }
            }
        }

        Debug.Log(msg);

    }

    public void DebugPosition(GridPos pos)
    {
        var msg = string.Format("Context around {0}:\n", pos);

        for (int yOff=1; yOff>-2; yOff--)
        {
            for (int xOff=-1; xOff<2; xOff++)
            {
                GridPos tmp = new GridPos(pos.x + xOff, pos.y + yOff);

                msg += IsValidPosition(tmp) ? gridOccupancy[pos.x, pos.y].ToString() : "-" ;

                if (xOff == 1)
                {
                    msg += "\n";
                } else
                {
                    msg += ", ";
                }
            }
        }

        Debug.Log(msg);
    }
}

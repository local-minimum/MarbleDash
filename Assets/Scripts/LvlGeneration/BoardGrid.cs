using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LocalMinimum.Grid;
using LocalMinimum.Arrays;

public enum Occupancy { Free, BallPathSource, BallPathTarget, BallPath, Wall, WallBreakable, WallIllusory, Hole, Obstacle, Enemy, Player, NoGrip};

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

    BitMaskArray<Occupancy> gridOccupancy;

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
        gridOccupancy = new BitMaskArray<Occupancy>(size);
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

    public bool OnBoard(GridPos pos)
    {
        return pos.x >= 0 && pos.x < size && pos.y >= 0 && pos.y < size;
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
            return new GridPos(PlayerRunData.stats.lvlRnd.Range(0, size),
                PlayerRunData.stats.lvlRnd.Range(0, size));
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
            gridOccupancy.Set(x, y, occupancy);
        }
        else
        {
            if (gridOccupancy.HasOnly(x, y, Occupancy.Free))
            {
                gridOccupancy.Set(x, y, occupancy);
            }
            else
            {
                gridOccupancy.Occupy(x, y, occupancy);
            }
        }
    
    }

    public void Occupy(GridPos pos, Occupancy occupancy)
    {
        Occupy(pos.x, pos.y, occupancy);
    }

    public void Free(int x, int y)
    {
        gridOccupancy.Set(x, y, Occupancy.Free);
    }

    public void Free(GridPos pos)
    {
        gridOccupancy.Set(pos, Occupancy.Free);
    }

    public void Free(GridPos pos, Occupancy filt)
    {
        if (filt == Occupancy.Free)
        {
            throw new System.ArgumentException("Can't free free on " + pos);
        }

        gridOccupancy.DeOccupy(pos, filt);
    }

    public void FreeAll()
    {
        if (gridOccupancy == null || !gridOccupancy.IsSize(size))
        {
            gridOccupancy = new BitMaskArray<Occupancy>(size);
        }

        gridOccupancy.SetAll(Occupancy.Free);
    }

    public bool IsFree(GridPos pos)
    {
        return gridOccupancy.HasOnly(pos, Occupancy.Free);
    }

    public bool IsFree(int x, int y)
    {
        return gridOccupancy.HasOnly(x, y, Occupancy.Free);
    }

    public List<Occupancy> GetOccupancy(GridPos pos)
    {
        return gridOccupancy.Flags(pos).ToList();
    }

    public bool HasOccupancy(GridPos pos, Occupancy filt)
    {
        return gridOccupancy.Has(pos, filt);
    }

    public IEnumerable<GridPos> Find(Occupancy occupancy)
    {
        foreach(Coordinate c in gridOccupancy.Find(occupancy))
        {
            yield return c;
        }
    }

    public IEnumerable<GridPos> FindIsOnlyAny(params Occupancy[] occupancy)
    {
        foreach (Coordinate c in gridOccupancy.FindOnlyAny(occupancy))
        {
            yield return c;
        }
    }

    public int Count(Occupancy occupancy)
    {
        return gridOccupancy.Count(occupancy);
    }

    public int CountInvalid()
    {
        return gridOccupancy.CountUnflagged();
    }

    int GetOccupancyFilter(params Occupancy[] occupancy)
    {
        int filt = 1 << (int)occupancy[0];
        for (int i = 1; i < occupancy.Length; i++)
        {
            filt |= 1 << (int)occupancy[i];
        }
        return filt;
    }
    
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

    public bool[,] GetFilterNotAny(params Occupancy[] occupancy)
    {
        return gridOccupancy.GetFilterNotAny(occupancy);
    }

    public bool[,] GetFilterHas(Occupancy occupancy)
    {
        return gridOccupancy.GetFilter(occupancy);
    }

    public int[,] GetOccupancyContextNonFree(GridPos pos)
    {
        return gridOccupancy.GetContextHasNot(pos, 3, Occupancy.Free);
    }

    public int[,] GetOccupancyContext(GridPos pos, params Occupancy[] filter)
    {
        return gridOccupancy.GetContextHasAny(pos, 3, -1, filter);
    }

    public int[,] GetNotOccupancyContext(GridPos pos, params Occupancy[] filter)
    {
        return gridOccupancy.GetContextHasNotAny(pos, 3, -1, filter);
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

    [SerializeField, Range(-1, 1)]
    float heightOffset = -0.1f;

    public void ConstructFloor()
    {
        InactivatePreviousTiles();
        Vector3 localScaleHoles = TileShape;
        Vector3 localScaleNonHoles = TileShape;
        localScaleHoles.x /= 2f;
        localScaleHoles.y /= 2f;
        localScaleHoles.z = .1f;
        localScaleNonHoles.x /= 2f;
        localScaleNonHoles.y /= 2f;
        localScaleNonHoles.z = 1f;
        int holeFilt = 1 << (int) Occupancy.Hole;
        for (int x=0; x<size; x++)
        {
            for (int y=0; y<size; y++)
            {
                Transform t;
                TileType tType;
                if ((gridOccupancy.GetInt(x, y) &  holeFilt) != 0)
                {
                    t = GetNextHole();
                    tType = TileType.Hole;
                } else
                {
                    t = GetNextSolid();
                    tType = TileType.Solid;
                }
                GridPos tilePos = new GridPos(x, y);
                tileLookUp[tilePos] = t;
                t.GetComponent<BoardTile>().SetPosition(this, tilePos, heightOffset, tType);
                t.localScale = tType == TileType.Hole ? localScaleHoles : localScaleNonHoles;
            }
        }
    }

    public BoardTile ConstructFloorAt(GridPos pos, TileType tileType)
    {
        Vector3 localScale = TileShape;
        localScale.x /= 2f;
        localScale.y /= 2f;
        localScale.z = tileType == TileType.Solid ? 1f : 0.1f;
        Transform t;
        if (tileType == TileType.Solid)
        {
            t = GetNextSolid();
        } else
        {
            t = GetNextHole();
        }
        tileLookUp[pos] = t;
        BoardTile tile = t.GetComponent<BoardTile>();
        tile.SetPosition(this, pos, heightOffset, tileType);
        t.localScale = localScale;
        return tile;
    }

    public Transform GetTile(GridPos pos)
    {
        return tileLookUp[pos];
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

    Dictionary<GridPos, Transform> tileLookUp = new Dictionary<GridPos, Transform>();

    public void InactivatePreviousTiles()
    {
        tileLookUp.Clear();
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

    [SerializeField]
    Vector3 gizmoOffset;

    [SerializeField]
    Occupancy gizmoShowing;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
        {
            return;
        }

        bool[,] flagStatus = gridOccupancy.GetFilter(gizmoShowing);

        Gizmos.color = Color.red;
        for (int x=0; x<size; x++)
        {

            for (int y=0; y<size; y++)
            {
                Gizmos.DrawWireSphere(GetWorldPosition(x, y), 10f / size);
                Gizmos.DrawIcon(transform.TransformPoint(GetLocalPosition(x, y)) + gizmoOffset, flagStatus[x, y] ? "numberIcon_Y.png" : "numberIcon_N.png", true);

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

                msg += IsValidPosition(tmp) ? gridOccupancy.GetInt(pos).ToString() : "-" ;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreBuilder : MonoBehaviour {

    [SerializeField]
    BoardGrid board;

    [SerializeField]
    RoomMaker roomMaker;

    [SerializeField]
    GridPos displayHoleLowerLeft;

    [SerializeField]
    GridPos displayHoleUpperRight;

    [SerializeField]
    GridPos[] storeSlots;

    [SerializeField]
    List<GridPos> exits = new List<GridPos>();

    [SerializeField]
    StoreItemTrigger sitPrefab;

    private void Start()
    {
        if (!PlayerRunData.stats.InStore)
        {
            PlayerRunData.stats.EnterNewStore();
        }
        board.FreeAll();
        TilesHoleAndHoles();
        MakeStoreSlots();
        MakeWalls();
    }

    void MakeWalls()
    {
        foreach(GridPos pos in board.Find(Occupancy.Wall))
        {
            board.Occupy(pos, Occupancy.Wall);
        }
        roomMaker.ConstructWalls();
    }

    void TilesHoleAndHoles()
    {
        List<GridPos> walls = new List<GridPos>();
        board.InactivatePreviousTiles();
        foreach(GridPos pos in board.Find(Occupancy.Free))
        {
            if (InclusiveInside(displayHoleLowerLeft, displayHoleUpperRight, pos))
            {
                continue;
            }
            if (Surrounding(displayHoleLowerLeft, displayHoleUpperRight, pos))
            {
                board.Occupy(pos, Occupancy.Wall);
            }

            BoardTile tile = board.ConstructFloorAt(pos, IsHole(pos) ? TileType.Hole : TileType.Solid);
            if (tile.tileType == TileType.Hole)
            {
                StoreTrigger st = tile.GetComponentInChildren<StoreTrigger>();
                st.gameObject.SetActive(true);

                if (exits.Contains(pos))
                {
                    StoreItemTrigger trig = Instantiate(sitPrefab, tile.transform, false);
                    trig.transform.localPosition = Vector3.zero;
                    trig.itemIndex = -1;
                    BoxCollider bc = trig.GetComponent<BoxCollider>();
                    bc.size = new Vector3(bc.size.x * 2.5f, bc.size.y * 4.5f, bc.size.z);
                }
            }
        }
    }

    bool IsHole(GridPos pos)
    {
        if (exits.Contains(pos))
        {
            return true;
        } else
        {
            for (int i=1, l=storeSlots.Length; i<l; i+=2)
            {
                if (pos == storeSlots[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    void MakeStoreSlots()
    {
        int storeIndex = 0;
        for (int i = 0, last=storeSlots.Length - 1; i<last; i+=2)
        {
            MakeSlot(storeSlots[i], storeSlots[i + 1], storeIndex);
            storeIndex++;
        }
    }

    void MakeSlot(GridPos from, GridPos to, int storeIndex)
    {
        Debug.Log("Making slot from " + from + " to " + to);
        GridPos cur = from;
        int i = 0;
        StoreItemTrigger trig;

        while (cur != to)
        {
            board.Occupy(cur, Occupancy.BallPath);

            trig = Instantiate(sitPrefab, board.GetTile(cur), false);
            trig.transform.localPosition = Vector3.zero;
            trig.itemIndex = storeIndex;

            cur += (to - cur).NineNormalized;
            if (board.IsValidPosition(cur.West))
            {
                board.Occupy(cur.West, Occupancy.Wall);
            }
            if (board.IsValidPosition(cur.East))
            {
                board.Occupy(cur.East, Occupancy.Wall);
            }

            i++;
            if (i > 10)
            {
                break;
            }
        }

        trig = Instantiate(sitPrefab, board.GetTile(to), false);
        trig.transform.localPosition = Vector3.zero;
        trig.itemIndex = storeIndex;

        GridPos south = to.South;
        board.Occupy(to, Occupancy.BallPath);
        if (board.IsValidPosition(south)) {
            board.Occupy(south, Occupancy.Wall);            
        }

        if (board.IsValidPosition(south.West))
        {
            board.Occupy(south.West, Occupancy.Wall);
        }

        if (board.IsValidPosition(south.East))
        {
            board.Occupy(south.East, Occupancy.Wall);
        }
    }

    public static bool InclusiveInside(GridPos a, GridPos b, GridPos c)
    {
        return c.x >= a.x && c.x <= b.x && c.y >= a.y && c.y <= b.y;
    }

    public static bool Surrounding(GridPos a, GridPos b, GridPos c)
    {
        return c.x - a.x == -1 && c.y <= b.y + 1 && c.y >= a.y - 1 ||
            c.x - b.x == 1 && c.y <= b.y + 1 && c.y >= a.y - 1 ||
            c.y - a.y == -1 && c.x <= b.x + 1 && c.x >= a.x - 1 ||
            c.y - b.y == 1 && c.x <= b.x + 1 && c.x >= a.x - 1;
    }
}

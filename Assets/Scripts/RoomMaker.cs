using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomMaker : MonoBehaviour {

    [SerializeField]
    BallPath ballPath;

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField, Range(1, 32)]
    int primaryRooms = 15;

    int[,] roomLayout;

    int size;

    [SerializeField]
    Wall wallPrefab;

    [SerializeField]
    Transform wallParent;

    [SerializeField]
    bool clearPreviousBoard;

    public void GenerateRooms()
    {
        if (clearPreviousBoard)
        {
            boardGrid.FreeAll();
        }

        size = boardGrid.Size;
        roomLayout = new int[size, size];
        SeedRooms();
        CountPrivates();
        RemoveRoomInRoom();
        SolidifyUncertain();
        SnakeWalls();
    }

    [SerializeField]
    int[] seedShapes = new int[] { 3, 4, 5, 6, 7 };

    void SeedRooms()
    {
        for (int i=0; i<primaryRooms;i++)
        {

            int mask = 1 << i;

            GridPos pos = boardGrid.RandomPosition;
            int w = seedShapes[Random.Range(0, seedShapes.Length)];
            int h = seedShapes[Random.Range(0, seedShapes.Length)];

            GridPos low = boardGrid.Clamp(pos - new GridPos(Random.Range(0, w), Random.Range(0, h)));
            GridPos high = boardGrid.Clamp(low + new GridPos(w, h));

            if (GridPos.ShortestDimension(low, high) < seedShapes[0])
            {
                //Debug.Log(string.Format("Too small room {0} - {1}", low, high));
                continue;
            }

            for (int y = low.y; y<=high.y; y++)
            {
                for (int x = low.x; x<=high.x; x++)
                {
                    roomLayout[x, y] |= mask;
                }
            }
        }
    }

    Dictionary<int, int> privates = new Dictionary<int, int>();
    int sharedTiles;

    void CountPrivates()
    {
        sharedTiles = 0;
        privates.Clear();

        for (int i = 0; i<primaryRooms; i++)
        {
            privates[i] = 0;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int val = roomLayout[x, y];
                if (val == 0 || BitCount(val) > 1)
                {
                    if (val != 0)
                    {
                        sharedTiles++;
                    }
                    continue;
                }

                try
                {
                    privates[FirstBit(val)]++;
                } catch (KeyNotFoundException)
                {
                    Debug.LogError(string.Format("{0} has {1} set and first is {2}", val, BitCount(val), FirstBit(val)));
                }
            }
        }
    }

    void RemoveRoomInRoom()
    {
        int n = 0;
        int mask = 0;
        for (int i = 0; i < primaryRooms; i++)
        {
            if (privates[i] == 0)
            {
                mask |= 1 << i;
                n++;
            }
        }
        mask = ~mask;
        Debug.Log(string.Format("Will remove {0} rooms.", n));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                roomLayout[x, y] &= mask;   
            }
        }
    }
    
    void SolidifyUncertain()
    {
        foreach(KeyValuePair<int, int> room in privates.Where(e => e.Value > 0).OrderBy(e => e.Value))
        {         
            int mask = 1 << room.Key;
                      
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int val = roomLayout[x, y];
                    if (TwoBits(val) && (mask & val) != 0)
                    {
                        sharedTiles--;
                        roomLayout[x, y] = mask;
                    }
                }
            }
        }
    }

    public void ConstructWalls()
    {
        for (int i=0, l=wallParent.childCount; i<l; i++)
        {
            wallParent.GetChild(i).gameObject.SetActive(false);
        }

        int n = 0;
        foreach(GridPos pos in boardGrid.Find(Occupancy.Wall))
        {
            Wall wall = GetWall(n);
            wall.SetPosition(boardGrid, pos);
            n++;
        }
    }

    Wall GetWall(int n)
    {

        if (n < wallParent.childCount)
        {
            return wallParent.GetChild(n).GetComponent<Wall>();
        } else
        {
            Wall wall = Instantiate(wallPrefab, wallParent);
            return wall;
        }
    }

    void SnakeWalls()
    {
        bool[,] walls = new bool[size, size];

        for (int y = 0; y < size; y++)
        {
            int prevX = roomLayout[0, y];
            for (int x = 0; x < size; x++)
            {
                int val = roomLayout[x, y];

                if (val != prevX)
                {
                    if (val == 0 && boardGrid.IsFree(x, y))
                    {
                        boardGrid.Occupy(x, y, Occupancy.Wall);
                        walls[x, y] = true;
                    }
                    else if (boardGrid.IsFree(x - 1, y) && (prevX == 0 || x > 0 && Random.value < 0.5f))
                    {
                        boardGrid.Occupy(x - 1, y, Occupancy.Wall);
                        walls[x - 1, y] = true;
                    }
                    else if (boardGrid.IsFree(x, y))
                    {
                        boardGrid.Occupy(x, y, Occupancy.Wall);
                        walls[x, y] = true;
                    }

                }
                prevX = val;

                if (y == 0)
                {
                    continue;
                }

                int prevY = roomLayout[x, y - 1];

                if (val != prevY)
                {
                    if (val == 0 && boardGrid.IsFree(x, y))
                    {
                        boardGrid.Occupy(x, y, Occupancy.Wall);
                        walls[x, y] = true;
                    }
                    else if ((prevY == 0 || Random.value < 0.5f) && boardGrid.IsFree(x, y - 1))
                    {
                        boardGrid.Occupy(x, y - 1, Occupancy.Wall);
                        walls[x, y - 1] = true;
                    }
                    else if (boardGrid.IsFree(x, y))
                    {
                        boardGrid.Occupy(x, y, Occupancy.Wall);
                        walls[x, y] = true;
                    }

                }
            }
        }
    }

    static int BitCount(int val)
    {
        int count = 0;

        while (val != 0)
        {
            if ((val & 0x1) == 0x1) count++;
            val >>= 1;
        }
        return count;
    }

    static bool TwoBits(int val)
    {
        int count = 0;

        while (val != 0)
        {
            if ((val & 0x1) == 0x1)
            {
                count++;
                if (count == 2)
                {
                    return true;
                }
            }
            val >>= 1;
        }
        return count == 2;
    }

    static int FirstBit(int val)
    {
        int index = 0;

        while (val != 0)
        {
            if ((val & 0x1) == 0x1) return index;
            val >>= 1;
            index++;
        }
        return -1;
    }


    static int UnsetBit(int value, int bitIndex) {
        int mask = 1 << bitIndex;
        return value & ~mask;
    }

    [SerializeField]
    bool drawGizmos;

    private void OnDrawGizmosSelected()
    {        
        if (roomLayout == null || !drawGizmos)
        {
            return;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int val = roomLayout[x, y];
                int first = FirstBit(val);

                if (first == -1)
                {
                    continue;
                }

                if (first % 2 == 0)
                {
                    Gizmos.color = Color.Lerp(Color.red, Color.black, first / 32.0f);   
                } else
                {
                    Gizmos.color = Color.Lerp(Color.blue, Color.white, first / 32.0f);
                }
                Gizmos.DrawWireCube(boardGrid.GetWorldPosition(x, y), Vector3.one);
            }
        }

    }


}

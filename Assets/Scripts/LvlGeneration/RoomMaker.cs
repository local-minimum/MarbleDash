﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LocalMinimum.Grid;
using LocalMinimum.Arrays;
using LocalMinimum.Collections;

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
        Debug.Log("Rooms: Generating");
        if (clearPreviousBoard)
        {
            boardGrid.FreeAll();
        }

        size = boardGrid.Size;
        roomLayout = new int[size, size];
        Debug.Log("Rooms: Seeding rooms");
        SeedRooms();
        Debug.Log("Rooms: Checking privates");
        CountPrivates();
        Debug.Log("Rooms: Remove internal rooms");
        RemoveRoomInRoom();
        Debug.Log("Rooms: Solidify");
        SolidifyUncertain();
        Debug.Log("Rooms: Allocate walls");
        SnakeWalls();
        Debug.Log("Rooms: Clean up reduntant walls");
        CleanUpWalls();
    }

    [SerializeField]
    int[] seedShapes = new int[] { 3, 4, 5, 6, 7 };

    void SeedRooms()
    {
        System.Random rnd = PlayerRunData.stats.lvlRnd;

        for (int i=0; i<primaryRooms;i++)
        {

            int mask = 1 << i;

            GridPos pos = boardGrid.RandomPosition;
            int w = seedShapes[rnd.Range(0, seedShapes.Length)];
            int h = seedShapes[rnd.Range(0, seedShapes.Length)];

            GridPos low = boardGrid.Clamp(pos - new GridPos(rnd.Range(0, w), rnd.Range(0, h)));
            GridPos high = boardGrid.Clamp(low + new GridPos(w, h));

            if (GridPos.ChessBoardDistance(low, high) < seedShapes[0])
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

    Dictionary<GridPos, Wall> activeWalls = new Dictionary<GridPos, Wall>();

    public void ConstructWalls()
    {

        activeWalls.Clear();

        for (int i=0, l=wallParent.childCount; i<l; i++)
        {
            wallParent.GetChild(i).gameObject.SetActive(false);
        }

        int n = 0;
        foreach(GridPos pos in boardGrid.FindAny(Occupancy.Wall, Occupancy.WallBreakable))
        {
            Wall wall = GetWall(n);
            wall.SetPosition(this, boardGrid, pos);
            n++;
            activeWalls[pos] = wall;
        }
        
        Debug.Log(string.Format("Placed {0}, counted {1}, though I made {2} walls", n, boardGrid.Count(Occupancy.Wall), wallCount));
    }
    public Wall GetActiveWall(GridPos pos)
    {
        return activeWalls[pos];
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

    int wallCount;

    bool[,] walls;

    void SnakeWalls()
    {
        System.Random rnd = PlayerRunData.stats.lvlRnd;
        walls = new bool[size, size];

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
                    else if (boardGrid.IsFree(x - 1, y) && (prevX == 0 || x > 0 && rnd.NextDouble() < 0.5f))
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
                    else if ((prevY == 0 || rnd.NextDouble() < 0.5f) && boardGrid.IsFree(x, y - 1))
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

        wallCount = walls.Count();
    }

    void CleanUpWalls()
    {

        bool[,] wallsBefore = (bool[,]) walls.Clone();
        walls.GenericFilter(5, WallBlockRemover, EdgeCondition.Constant, false);

        foreach (Coordinate coord in wallsBefore.ZipTwo(walls, (a, b) => a != b).ToCoordinates())
        {
            boardGrid.Free(coord, Occupancy.Wall);
        }

    }

    bool WallBlockRemover(int x, int y, bool[,] context)
    {
        //TODO: Can be extended to minimize breaking up wall segments

        if (!context[2, 2])
        {
            //Never add walls
            return false;
        } else
        {
            bool[,] center = context.GetCenteredContext(3, 2, 2, EdgeCondition.Constant, false);
            int n = center.Count();
            if (n < 4)
            {
                //Can't be wallblock
                return true;
            } else
            {
                bool[,] fourBlock = center.GenericFilter(2, ArrayRegions.All);

                if (fourBlock.Any())
                {
                    Coordinate coord = new Coordinate();
                    while (fourBlock.Any()) {
                        
                        //Exists  a wall block

                        int toResolve = PlayerRunData.stats.lvlRnd.Range(0, fourBlock.Count()) + 1;
                        if (fourBlock.Locate(toResolve, ref coord))
                        {
                            //Select which of the four corners of block to remove
                            int piece = PlayerRunData.stats.lvlRnd.Range(0, 4);
                            coord.x += piece % 2;
                            coord.y += piece > 1 ? 1 : 0;
                            //Offset and update
                            center[coord.x, coord.y] = false;
                            coord.x += x - 1;
                            coord.y += y - 1;
                            if (!walls[coord.x, coord.y])
                            {
                                Debug.LogError((GridPos)coord + " not true");
                            }
                            else {
                                walls[coord.x, coord.y] = false;
                            }
                            fourBlock = center.GenericFilter(2, ArrayRegions.All);
                        } else
                        {
                            Debug.LogError("Can't get " + toResolve + "th in " + fourBlock.Map(e => e ? 1 : 0).ToCSV());
                            break;
                        }
                    }
                    return center[1, 1];
                } else
                {
                    //No wallblocks in center
                    return true;
                }
            }
        }
    }


    public void FillFreeSingleIslands()
    {
        Coordinate coord = new Coordinate();
        int labels;
        int[,] labeled = boardGrid.GetFilterHas(Occupancy.Wall).Inverted().Label(out labels, Neighbourhood.Cross);
        for (int i=1; i<labels+1; i++)
        {
            if (labeled.CountValue(i) == 1)
            {
                if (labeled.Locate(i, 1, ref coord)) {
                    if (boardGrid.IsFree(coord))
                    {
                        boardGrid.Occupy(coord, Occupancy.Wall);
                    }
                }
            }
        }
    }

    public void BridgeIsolatedRooms(int[,] playerConnectivity, int count)
    {
        //TODO: Validate that there's connection between all rooms not just loop

        //All rooms are isloated to start
        List<List<int>> roomGroups = new List<List<int>>();
        for (int label = 1; label < count + 1; label++) {
            roomGroups.Add(new List<int>() { label });
        }
        int nRooms = roomGroups.Count;
        while (roomGroups.Count > 1)
        {
            Debug.Log(nRooms + " room groups");
            bool[,] room = playerConnectivity.HasAnyValue(roomGroups[0].ToArray());
            bool[,] otherRooms = playerConnectivity.ZipTwo(room, (a, b) => !b && a > 0);
            
            bool[,] dilatedRoom = room.Dilate(Neighbourhood.Cross, EdgeCondition.Constant);

            int dilations = 1;
            while (!dilatedRoom.ZipTwo(otherRooms, (a, b) => a && b).Any())
            {

                dilatedRoom = room.ZipThree(dilatedRoom, walls, (r, d, w) => r || d && w).Dilate(Neighbourhood.Cross, EdgeCondition.Constant);
                dilations++;
            
                if (dilations > 10)
                {
                    throw new System.ArgumentException("Have dilated more than 10 times, should have found something");
                }
            }

            //Makes a passage between areas
            int[,] crawlMap = dilatedRoom.Distance(room.Edge().ToCoordinates().ToArray(), Neighbourhood.Cross);
            GridPos crawlPoint = dilatedRoom.ZipThree(otherRooms, crawlMap, (d, o, c) => d && o && c > 0).ToCoordinates().Shuffle(PlayerRunData.stats.lvlRnd).First();

            int distance = crawlMap[crawlPoint.x, crawlPoint.y];
            int roomSourceID = playerConnectivity[crawlPoint.x, crawlPoint.y];
            Debug.Log(dilatedRoom[crawlPoint.x, crawlPoint.y]);
            Debug.Log(otherRooms[crawlPoint.x, crawlPoint.y]);
            Debug.Log(distance);
            Debug.Log(roomSourceID);

            while (crawlMap[crawlPoint.x, crawlPoint.y] > 0)
            {
                bool foundDirection = false;
                foreach (GridPos neighbour in crawlPoint.GetNeighbours(Neighbourhood.Cross))
                {

                    if (boardGrid.IsValidPosition(neighbour) && crawlMap[neighbour.x, neighbour.y] < distance)
                    {
                        distance = crawlMap[neighbour.x, neighbour.y];
                        if (boardGrid.HasOccupancy(neighbour, Occupancy.Wall))
                        {
                            boardGrid.Free(neighbour, Occupancy.Wall);
                            boardGrid.Occupy(neighbour, Occupancy.WallBreakable);
                            Debug.Log("Making " + neighbour + " breakable");
                        } else
                        {
                            Debug.Log(neighbour + " has no wall, this may be OK");
                        }
                        crawlPoint = neighbour;
                        foundDirection = true;
                        break;
                    }
                }
                if (!foundDirection)
                {
                    throw new System.ArgumentException("No neighbour to " + crawlPoint + " is closer to target, should not be possible");
                }
            }

            //Register room connections
            bool madeConnection = false;
            for (int i=1, l=roomGroups.Count; i<l; i++)
            {
                if (roomGroups[i].Contains(roomSourceID))
                {
                    madeConnection = true;
                    roomGroups[0].AddRange(roomGroups[i]);
                    roomGroups.RemoveAt(i);
                    break;
                }
            }
            if (!madeConnection)
            {
                throw new System.ArgumentException("Connectivity Map wrong because could not connect rooms that were connected");
            }

            if (nRooms <= roomGroups.Count)
            {
                throw new System.ArgumentException("Didn't make any connection, though should have");
            }
            nRooms--;
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

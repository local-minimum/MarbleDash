using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallPath : MonoBehaviour {

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField]
    bool connectPrevious = true;

    GridPos startPos;
    GridPos endPos;
    List<GridPos> path = new List<GridPos>();

    public int minPath = 10;

    public int maxPath = 30;

    [SerializeField, Range(0, 1)]
    float minPathTolerance = 0.8f;

    public Vector3 DropTarget
    {
        get
        {
            return boardGrid.GetWorldPosition(startPos);
        }
    }

    public void GeneratePath(bool connectPrevious)
    {
        while (true)
        {
            SetPathSource(connectPrevious);
            SetPath(Random.Range(minPath, maxPath));
            if (path.Count < Mathf.FloorToInt(minPath * minPathTolerance))
            {
                for (int i = 0, n = path.Count; i < n; i++)
                {
                    boardGrid.Free(path[i]);
                }
                connectPrevious = false;

            } else
            {
                break;
            }
        }

        endPos = path[path.Count - 1];
        path.Remove(endPos);
        
    }

    [SerializeField, Range(0, 1)]
    float holeProbability = 0.5f;
    public void GeneratePathHoles()
    {
        for (int i = 0, l = path.Count; i < l; i++)
        {

            GridPos pos = path[i];
            int[,] context = boardGrid.GetOccupancyContext(pos, Occupancy.BallPath);
            if (context[2, 1] == 1 && context[1, 0] == 1 && boardGrid.IsFree(pos.x + 1, pos.y - 1) && holeProbability < Random.value)
            {
                boardGrid.Occupy(pos.x + 1, pos.y - 1, Occupancy.Hole);
            } else if (context[2, 1] == 1 && context[1, 0] == 1 && context[1, 2] == 0 && boardGrid.IsFree(pos.x, pos.y + 1) && holeProbability < Random.value)
            {
                boardGrid.Occupy(pos.x, pos.y + 1, Occupancy.Hole);
            }else if (context[0, 1] == 1 && context[1, 0] == 1 && boardGrid.IsFree(pos.x - 1, pos.y - 1) && holeProbability < Random.value)
            {
                boardGrid.Occupy(pos.x - 1, pos.y - 1, Occupancy.Hole);
            }
            else if (context[0, 1] == 1 && context[1, 0] == 1 && context[2, 1] == 0 && boardGrid.IsFree(pos.x + 1, pos.y) && holeProbability < Random.value)
            {
                boardGrid.Occupy(pos.x + 1, pos.y, Occupancy.Hole);
            }

        }
    }

    public void ConstructPath()
    {
        RemoveTiles();
        startTile.SetPosition(boardGrid, startPos);
        BallPathTile next = null;
        BallPathTile prev = startTile;

        for (int i = 0, l = path.Count; i < l; i++)
        {
            next = GetTile(i);
            next.SetPosition(boardGrid, path[i]);
            prev.SetNextTile(next);
            prev.GenerateMesh();
            prev = next;
        }

        endTile.SetPosition(boardGrid, endPos);
       
        if (prev != null)
        {
            prev.SetNextTile(endTile);
            prev.GenerateMesh();
        }
    }

    void RemoveTiles()
    {
        for (int i = 0, l = pathParent.childCount; i < l; i++)
        {
            pathParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    [SerializeField]
    BallPathTile startTile;

    [SerializeField]
    BallPathTile endTile;

    [SerializeField]
    Transform pathParent;

    [SerializeField]
    BallPathTile pathTilePrefab;

    BallPathTile GetTile(int i)
    {
        if (i < pathParent.childCount)
        {
            BallPathTile bpt = pathParent.GetChild(i).GetComponent<BallPathTile>();
            bpt.gameObject.SetActive(true);
            return bpt;
        } else
        {
            BallPathTile bpt = Instantiate(pathTilePrefab, pathParent, false);
            bpt.name = "Ball Path Segment " + i;
            return bpt;
        } 
    }

    void SetPathSource(bool connectPrevious)
    {

        bool hasSource = false;

        if (connectPrevious)
        {
            try
            {
                startPos = boardGrid.Find(Occupancy.BallPathTarget).First();
                hasSource = true;
            }
            catch (System.NullReferenceException) {}
            catch (System.InvalidOperationException) { }
            finally
            {
            }
        }

        boardGrid.FreeAll();

        if (!hasSource)
        {
            startPos = boardGrid.RandomPosition;
        }

        boardGrid.Occupy(startPos, Occupancy.BallPathSource);
    }

    [SerializeField, Range(0, 1)]
    float turnProb = 0.4f;


    [SerializeField, Range(0, 1)]
    float backstepFraction = 0.5f;

    [SerializeField]
    int persistance = 7;

    public void SetPath(int length)
    {
        List<GridPos> crossNeighbours = boardGrid.Neighbours(startPos, BoardGrid.Neighbourhood.Cross).ToList();
        GridPos direction = crossNeighbours[Random.Range(0, crossNeighbours.Count)] - startPos;
        GridPos pos = startPos + direction;

        int i = 0;
        path.Clear();
        while (length > path.Count)
        {
            path.Add(pos);

            boardGrid.Occupy(pos, Occupancy.BallPath);

            crossNeighbours = boardGrid
                .Neighbours(pos, BoardGrid.Neighbourhood.Cross)
                .Where(e => !path.Contains(e) && e != startPos)
                .ToList();

            GridPos next = pos + direction;

            if (Random.value > turnProb && crossNeighbours.Contains(pos))
            {
                pos = next;
            }
            else
            {
                crossNeighbours.Remove(pos);

                var nextNeighbours = new List<GridPos>();

                while (crossNeighbours.Count > 0)
                {
                    next = crossNeighbours[Random.Range(0, crossNeighbours.Count)];                    

                    nextNeighbours = boardGrid
                        .Neighbours(next, BoardGrid.Neighbourhood.Eight)
                        .Where(e => !boardGrid.IsFree(e))
                        .ToList();
                   
                    if (nextNeighbours.Count > 2 ||
                        nextNeighbours.Count == 2 && 
                        GridPos.TaxiCabDistance(nextNeighbours[0], nextNeighbours[1]) != 1)
                    {
                        crossNeighbours.Remove(next);
                    } else
                    {
                        break;
                    }
                }

                if (crossNeighbours.Count == 0)
                {
                    int nodes = path.Count;
                    int backstep = Mathf.CeilToInt(nodes * backstepFraction);
                    if (nodes > backstep + 1)
                    {
                        next = path[path.Count - backstep];
                        //Debug.Log("Path length before " + path.Count);

                        for (int idE=nodes - backstep; idE < nodes; idE++)
                        {
                            boardGrid.Free(path[idE]);
                        }

                        path = path
                            .Select((e, idE) => new { index = idE, elem = e })
                            .Where(e => e.index < nodes - backstep)
                            .Select(e => e.elem)
                            .ToList();

                        //Debug.Log("Path length after " + path.Count);
                        pos = path[path.Count - 1];

                    }
                    else
                    {
                        for (int idE = 0; idE < nodes; idE++)
                        {
                            boardGrid.Free(path[idE]);
                        }
                        path.Clear();
                        crossNeighbours = boardGrid.Neighbours(startPos, BoardGrid.Neighbourhood.Cross).ToList();
                        next = crossNeighbours[Random.Range(0, crossNeighbours.Count)];
                        pos = startPos;
                        //Debug.Log("Clearing path");
                    }
                    
                }

                direction = next - pos;
                pos = next;

                /*
                boardGrid.DebugPosition(next);

                Debug.Log(boardGrid
                    .Neighbours(next, BoardGrid.Neighbourhood.Eight)
                    .Where(e => !boardGrid.IsFree(e))
                    .Count());

                 */

            }

            i++;
            if (i > length * persistance)
            {

                break;
            }
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

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(boardGrid.GetWorldPosition(startPos), 0.5f);

        Gizmos.color = Color.gray;

        for (int i=0; i<path.Count; i++)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.black, i / ((float)path.Count));
            Gizmos.DrawSphere(boardGrid.GetWorldPosition(path[i]), 0.25f);
        }
    }
}

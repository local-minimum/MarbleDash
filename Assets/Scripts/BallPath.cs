using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallPath : MonoBehaviour {

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField]
    LineRenderer lineRenderer;

    [SerializeField]
    bool connectPrevious = true;

    GridPos source;
    GridPos target;
    List<GridPos> path = new List<GridPos>();

    public int minPath = 10;

    public int maxPath = 30;

    private void Start()
    {
        lineRenderer.enabled = false;
    }

    public void GeneratePath(bool connectPrevious)
    {
        while (true)
        {
            SetPathSource(connectPrevious);
            SetPath(Random.Range(minPath, maxPath));
            if (path.Count < Mathf.FloorToInt(minPath * 0.8f))
            {
                for (int i=0, n=path.Count; i<n; i++)
                {
                    boardGrid.Free(path[i]);
                }
                connectPrevious = false;

            } else
            {
                break;
            }
        }
        List<Vector3> positions = new List<Vector3>();
        positions.Add(boardGrid.GetLocalPosition(source));
        positions.AddRange(path.Select(e => boardGrid.GetLocalPosition(e)));
      
        lineRenderer.enabled = true;
        lineRenderer.numCapVertices = 1;
        lineRenderer.numPositions = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    void SetPathSource(bool connectPrevious)
    {

        bool hasSource = false;

        if (connectPrevious)
        {
            try
            {
                source = boardGrid.Find(Occupancy.BallPathTarget).First();
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
            source = boardGrid.RandomPosition;
        }

        boardGrid.Occupy(source, Occupancy.BallPathSource);
    }

    [SerializeField]
    float turnProb = 0.4f;


    [SerializeField, Range(0, 1)]
    float backstepFraction = 0.5f;

    [SerializeField]
    int persistance = 7;

    public void SetPath(int length)
    {
        List<GridPos> crossNeighbours = boardGrid.Neighbours(source, BoardGrid.Neighbourhood.Cross).ToList();
        GridPos direction = crossNeighbours[Random.Range(0, crossNeighbours.Count)] - source;
        GridPos pos = source + direction;

        int i = 0;
        path.Clear();
        while (length > path.Count)
        {
            path.Add(pos);

            boardGrid.Occupy(pos, Occupancy.BallPath);

            crossNeighbours = boardGrid
                .Neighbours(pos, BoardGrid.Neighbourhood.Cross)
                .Where(e => !path.Contains(e) && e != source)
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
                        crossNeighbours = boardGrid.Neighbours(source, BoardGrid.Neighbourhood.Cross).ToList();
                        next = crossNeighbours[Random.Range(0, crossNeighbours.Count)];
                        pos = source;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(boardGrid.GetWorldPosition(source), 0.5f);

        Gizmos.color = Color.gray;

        for (int i=0; i<path.Count; i++)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.black, i / ((float)path.Count));
            Gizmos.DrawSphere(boardGrid.GetWorldPosition(path[i]), 0.25f);
        }
    }
}

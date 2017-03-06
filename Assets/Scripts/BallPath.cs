using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallPath : MonoBehaviour {

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField]
    bool connectPrevious = true;

    GridPos source;
    GridPos target;
    List<GridPos> path = new List<GridPos>();

    public int minPath = 10;

    public int maxPath = 30;

    public void GeneratePath(bool connectPrevious)
    {
        SetPathSource(connectPrevious);
        SetPath(Random.Range(minPath, maxPath));
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
                do
                {
                    next = crossNeighbours[Random.Range(0, crossNeighbours.Count)];
                    nextNeighbours = boardGrid
                        .Neighbours(next, BoardGrid.Neighbourhood.Eight)
                        .Where(e => boardGrid.IsFree(pos))
                        .ToList();

                    if (nextNeighbours.Count > 2 || nextNeighbours.Count == 2 && GridPos.TaxiCabDistance(nextNeighbours[0], nextNeighbours[1]) != 1)
                    {
                        crossNeighbours.Remove(next);
                    } else
                    {
                        break;
                    }
                } while (true);

                if (nextNeighbours.Count == 0)
                {
                    Debug.LogError("Bad End");
                    break;
                }
                direction = next - pos;
                pos = next;
            }

            i++;
            if (i > length * 4)
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

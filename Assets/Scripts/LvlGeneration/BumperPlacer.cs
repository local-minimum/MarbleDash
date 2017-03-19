using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class BumperPlacer : MonoBehaviour {

    [SerializeField]
    BoardGrid board;

    [SerializeField]
    Bumper prefab;

    [SerializeField]
    int minBumpers;

    [SerializeField]
    int maxBumpers;

    List<GridPos> nextPositions = new List<GridPos>();
    List<Bumper> activeBumpers = new List<Bumper>();
    List<Bumper> pool = new List<Bumper>();

    public void AllocateBumpPlacements()
    {
        nextPositions.Clear();
        int bumperTargetCount = Random.Range(minBumpers, maxBumpers);
        if (bumperTargetCount > 0)
        {
            GridPos[] positions = board.Find(Occupancy.Free)
                .Where(e => BoardGrid.CountContextOccupied(board.GetOccupancyNonFree(e), false, true) < 2)
                .ToArray()
                .Shuffle();

            for (int i=0, l=Mathf.Min(positions.Length, bumperTargetCount); i<l; i++)
            {
                GridPos pos = positions[i];
                nextPositions.Add(pos);
                board.Occupy(pos, Occupancy.Obstacle);
            }
        }
    }

    public void SpawnBumpers()
    {
        DeactivateBumpers();
        for (int i=0, l=nextPositions.Count; i<l; i++)
        {
            Bumper b = GetBumper();
            b.SetPosition(board, nextPositions[i]);
        }
    }

    int nextInPool = 0;

    void DeactivateBumpers()
    {
        for (int i=0, l=activeBumpers.Count; i<l;i++)
        {
            activeBumpers[i].gameObject.SetActive(false);
        }
        activeBumpers.Clear();
        nextInPool = 0;
    }

    Bumper GetBumper()
    {
        if (nextInPool < pool.Count)
        {
            Bumper b = pool[nextInPool];
            activeBumpers.Add(b);
            nextInPool++;
            return b;
        } else
        {
            Bumper b = Instantiate(prefab, transform, false);
            activeBumpers.Add(b);
            nextInPool++;
            pool.Add(b);
            return b;
        }
    }
}

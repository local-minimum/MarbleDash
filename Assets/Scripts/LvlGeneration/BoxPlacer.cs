using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LocalMinimum.Grid;
using LocalMinimum.Collections;

public class BoxPlacer : MonoBehaviour {

    [SerializeField]
    Transform boxPlaceParent;

    [SerializeField]
    CoinBox[] prefabs;

    [SerializeField]
    int minBoxes = 5;

    [SerializeField]
    int maxBoxes = 15;

    [SerializeField]
    BoardGrid board;

    List<GridPos> positions = new List<GridPos>();

    public void Generate()
    {
        positions.Clear();
        GridPos[] freePositions = board.Find(Occupancy.Free).ToArray().Shuffle();
        positions.AddRange(freePositions.Take(PlayerRunData.stats.lvlRnd.Range(minBoxes, maxBoxes)));
        for (int i = 0, l = positions.Count; i < l; i++)
        {
            board.Occupy(positions[i], Occupancy.Obstacle);
        }
    }

    public void Place()
    {
        System.Random rnd = PlayerRunData.stats.lvlRnd;

        for (int i=0, l=boxPlaceParent.childCount; i<l;  i++)
        {
            Destroy(boxPlaceParent.GetChild(i).gameObject, 0.1f);
        }


        for (int i = 0, l = positions.Count; i < l; i++)
        {
            CoinBox box = Instantiate(prefabs[rnd.Range(0, prefabs.Length)], boxPlaceParent, false);
            box.SetPosition(positions[i]);
        }
    }



}

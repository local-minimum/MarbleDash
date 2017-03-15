using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class CollectionsHelper
{
    public static GridPos[] Shuffle(this GridPos[] data)
    {
        List<KeyValuePair<float, GridPos>> list = new List<KeyValuePair<float, GridPos>>();


        for(int i=0; i<data.Length; i++)
        {
            list.Add(new KeyValuePair<float, GridPos>(Random.value, data[i]));
        }
      
        return list.OrderBy(e => e.Key).Select(e => e.Value).ToArray();
    }
}

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
        positions.AddRange(freePositions.Take(Random.Range(minBoxes, maxBoxes)));
        for (int i = 0, l = positions.Count; i < l; i++)
        {
            board.Occupy(positions[i], Occupancy.Obstacle);
        }
    }

    public void Place()
    {
        for (int i=0, l=boxPlaceParent.childCount; i<l;  i++)
        {
            Destroy(boxPlaceParent.GetChild(i).gameObject, 0.1f);
        }


        for (int i = 0, l = positions.Count; i < l; i++)
        {
            CoinBox box = Instantiate(prefabs[Random.Range(0, prefabs.Length)], boxPlaceParent, false);
            box.SetPosition(positions[i]);
        }
    }



}

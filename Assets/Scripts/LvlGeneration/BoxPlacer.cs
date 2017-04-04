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
    
    IEnumerable<GridPos> MyOwnTake(GridPos[] item, int amount)
    {
        for (int i=0; i<amount; i++)
        {
            yield return item[i];
        }
    }

    public void Generate()
    {
        Debug.Log("Boxes: Clearing");
        positions.Clear();
        Debug.Log("Boxes: Finding free positions and shuffle");
        GridPos[] freePositions = board.Find(Occupancy.Free).ToArray().Shuffle();
        int boxes = Mathf.Min(PlayerRunData.stats.lvlRnd.Range(minBoxes, maxBoxes), freePositions.Length);
        if (boxes > 0) {
            Debug.Log(string.Format("Boxes: Taking {0} boxes from {1} available positions", boxes, freePositions.Length));
            //positions.AddRange(freePositions.Take(boxes)); //Crashes in webplayer
            positions.AddRange(MyOwnTake(freePositions, boxes)); //Doesn't crash
            for (int i = 0, l = positions.Count; i < l; i++)
            {
                Debug.Log("Boxes: Occupying position " + positions[i] + " for box " + i);
                board.Occupy(positions[i], Occupancy.Obstacle);
            }
        } else
        {
            Debug.LogWarning("Boxes: No empty places for boxes!");
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

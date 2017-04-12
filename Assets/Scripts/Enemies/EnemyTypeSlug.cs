using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Grid;

public class EnemyTypeSlug : Enemy {

    [SerializeField]
    int trailLengthBase = 5;

    [SerializeField]
    int trailLengthPerTier = 1;

    List<BoardTile> slimedTiles = new List<BoardTile>();
     
    bool didWalkThisTurn;

    [SerializeField, Range(0, 2)]
    float turnTimeMultiplier = 1.75f;

    [SerializeField]
    Color slimedColor;

    [SerializeField, Range(0, 1)]
    float slimeColorIntensity = 0.75f;

    protected override EnemyMode ExecuteWalking(PlayerController player, int turnIndex, float turnTime)
    {
        if (didWalkThisTurn)
        {
            didWalkThisTurn = false;
        }
        else {
            didWalkThisTurn = true;
            base.ExecuteWalking(player, turnIndex, turnTimeMultiplier * turnTime);
            StartCoroutine(DelaySlimeUpdate(target, turnTimeMultiplier * turnTime));
        }
        return EnemyMode.Walking;
    }

    float timeBeforeUnslime = 0.3f;
    float timeBeforeSlime = 0.9f;

    IEnumerator<WaitForSeconds> DelaySlimeUpdate(GridPos target, float turnTime)
    {
        //Debug.Log(target);
        int trailLength = trailLengthPerTier * (activeTierIndex + 1) + trailLengthBase;
        
        yield return new WaitForSeconds(timeBeforeUnslime * turnTime);

        while (slimedTiles.Count > trailLength)
        {
            BoardTile removeTile = slimedTiles[0];
            slimedTiles.RemoveAt(0);
            removeTile.RemoveSlime();
        }

        yield return new WaitForSeconds((timeBeforeSlime - timeBeforeUnslime) * turnTime);

        Material m = board.GetTile(target).GetComponentInChildren<Renderer>().material;

        //Should only modify slimy if is new on trail
        BoardTile addTile = board.GetTile(target).GetComponentInChildren<BoardTile>();
        slimedTiles.Add(addTile);
        addTile.Slime(slimedColor, slimeColorIntensity);
        
    }

    protected override bool ForceBehaviourSequence()
    {
        if (didWalkThisTurn)
        {            
            return true;
        }
        else {
            return base.ForceBehaviourSequence();
        }
    }


    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.black;
        GridPos prev = pos;
        
        for (int i=slimedTiles.Count - 1; i >= 0; i--)
        {
            GridPos cur = slimedTiles[i].pos;

            Gizmos.DrawLine(
                board.GetWorldPosition(prev), board.GetWorldPosition(cur));

            Gizmos.DrawCube(board.GetWorldPosition(cur), Vector3.one * 0.5f);

            prev = cur;
        }
    }
 
}

using System.Collections;
using System.Collections.Generic;
using LocalMinimum.Grid;
using LocalMinimum.Collections;
using LocalMinimum.Arrays;
using UnityEngine;
using System.Linq;

/// <summary>
/// The mole scurries in and out of holes
/// </summary>
/// .
/// The behaviour pattern is as follows.
/// 
/// 1. Select a hole to go to based on player position and walk below ground avoiding holes
/// 2. If hole is not good enough anymore change to other hole
/// 3. If next to hole, hunt is available
///     This is three steps
///         1. Low in hole
///         2. High in hole
///         3. Attack
///         4. Dive into hole (may not be same if near)


public class EnemyTypeMole : Enemy {

    bool underground = true;
    bool inHole = false;

    [SerializeField]
    int throwLength = 7;

    [SerializeField]
    float prefNearbyHole = 0.2f;
    
    int nearbyHoleDist = 2;

    [SerializeField, Range(2, 20)]
    int minimumHolesInLevel = 3;

    static List<GridPos> claimedBurrows = new List<GridPos>();

    protected override bool SpecialCriteriaForSelectionFullfilled(Level lvl)
    {        
        return lvl.boardHoles.Length > minimumHolesInLevel;
    }

    protected override bool ForceBehaviourSequence()
    {
        
        if (inHole) {
            if (!underground)
            {
                behaviour = EnemyMode.Hiding;
                return true;
            } else if (previousBehaviour == EnemyMode.Hiding)
            {
                behaviour = EnemyMode.Walking;
            }

        }

        return false;
    }

    protected override GridPos ExecuteHiding(PlayerController player, float turnTime)
    {
        underground = true;
        return pos;
    }

    protected override GridPos ExecuteWalking(PlayerController player, float turnTime)
    {
        inHole = board.HasOccupancy(pos, Occupancy.Hole);
        //Debug.Log(string.Format("Mole in hole {0} and previously {1}", inHole, previousBehaviour));
        if (inHole && previousBehaviour == EnemyMode.Walking)
        {
            behaviour = EnemyMode.Hiding;
            return ExecuteHiding(player, turnTime);
        }

        if (previousBehaviour == EnemyMode.Hiding || targetCheckpoints.Count == 0)
        {            
            GridPos playerPos = player.onTile;
            targetCheckpoints.Clear();
            GridPos target = new GridPos();

            Coordinate[] preferred = lvl.boardHoles.Where(e => e != pos && !claimedBurrows.Contains(e) && LineOfSight(e, playerPos)).ToArray();
            if (preferred.Length > 0)
            {
                underground = true;
                target = preferred.Shuffle().First();
                Debug.Log("Player based selection");
            }
            else {
                
                bool selectedHole = false;

                if (Random.value < prefNearbyHole)
                {

                    preferred = lvl.boardHoles
                        .Where(e => e != pos && !claimedBurrows.Contains(e))
                        .Where(e => GridPos.ChessBoardDistance(e, pos) < nearbyHoleDist)
                        .ToArray();

                    if (preferred.Length > 0)
                    {
                        underground = false;
                        target = preferred.Shuffle().First();
                        selectedHole = true;
                        Debug.Log("Proximity selection");
                    }

                }
                    
                if (!selectedHole)
                {
                    //Need redoing incase attempted nearby hole without finding one
                    preferred = lvl.boardHoles.Where(e => e != pos && !claimedBurrows.Contains(e)).ToArray();                    
                    target = preferred.Shuffle().First();
                    Debug.Log("Random hole");
                    underground = true;
                }
                    
            }
            Debug.Log("Selected burrows" + target);
            targetCheckpoints.Add(target);
            claimedBurrows.Add(target);
            SetTargetDistances(0);
            if (claimedBurrows.Contains(pos))
            {
                claimedBurrows.Remove(pos);
            }
        }

        SetContextFromDistanceMapAndPosition(targetDistanceMap);
        return SelectContextDirectionAndMove(turnTime);
    }

    protected override void SetTargetDistances(int targetIndex)
    {
        if (underground)
        {
            GridPos target = targetCheckpoints[targetIndex];
            targetDistanceMap = lvl.enemyHolesConnectivity.Distance(target, Neighbourhood.Eight);
            activeTargetIndex = targetIndex;
        }
        else {
            base.SetTargetDistances(targetIndex);
        }
    }

    bool LineOfSight(GridPos a, GridPos b)
    {
        int taxiDistance = GridPos.TaxiCabDistance(a, b);
        return taxiDistance < throwLength && GridPos.ChessBoardDistance(a, b) == taxiDistance && lvl.nonBlocking.LineInOneRegion(a, b);
    }
}

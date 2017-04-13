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
    bool hasAttacked = false;

    [SerializeField]
    int throwLength = 7;

    [SerializeField]
    float prefNearbyHole = 0.2f;
    
    int nearbyHoleDist = 2;

    [SerializeField, Range(2, 20)]
    int minimumHolesInLevel = 3;

    static List<GridPos> claimedBurrows = new List<GridPos>();

    GridPos lastBurrow = new GridPos(-1, -1);

    protected override bool SpecialCriteriaForSelectionFullfilled(Level lvl)
    {        
        return lvl.boardHoles.Length > minimumHolesInLevel;
    }

    protected override bool ForceBehaviourSequence()
    {
        inHole = board.HasOccupancy(pos, Occupancy.Hole);

        if (inHole) {
            UpdateBurrows(pos);
            if (hasAttacked)
            {
                if (!underground)
                {
                    behaviour = EnemyMode.Hiding;
                    return true;
                }
                else if (previousBehaviour == EnemyMode.Hiding)
                {
                    behaviour = EnemyMode.Walking;
                    return true;
                }
            } 
        } else
        {
            if (!underground)
            {
                if (hasAttacked)
                {
                    behaviour = EnemyMode.Hiding;
                    return true;
                } else if (previousBehaviour == EnemyMode.Attack1)
                {
                    behaviour = EnemyMode.Attack1;
                    //Need to keep attacking
                    return true;
                }
            }
        }

        return false;
    }

    protected override bool PlayerInRange(GridPos playerOffset, EnemyMode mode)
    {
        if (IsAttack(mode))
        {
            int dist = playerOffset.ChessBoardMagnitude;
            if (mode == EnemyMode.Attack1)
            {
                return (inHole || !underground) && dist <= attackRange;
            } else if (mode == EnemyMode.Attack2)
            {
                return inHole && LineOfSight(pos, playerOffset + pos);
            }
            return false;
        } else
        {
            return true;
        }
    }

    [SerializeField, Range(2, 5)]
    int allowHunting = 2;

    protected override EnemyMode ExecuteAttack1(int turnIndex, float turnTime)
    {
        int distToBurrow = GridPos.ChessBoardDistance(lastBurrow, pos);
        int distToPlayer = GridPos.ChessBoardDistance(player.onTile, pos);
        if (underground)
        {
            Debug.Log("Popping out of burrow");
            underground = false;
            targetCheckpoints.Clear();
            transform.localPosition = board.GetLocalPosition(pos) + attack2Offset;
            StartCoroutine(delayTransitionToNone(turnTime * 0.1f));

        } else if (distToPlayer <= 1)
        {
            Debug.Log("player can be bitten " + distToPlayer);
            StartCoroutine(AnimateAttack1(player, turnTime * 0.25f));
        } else
        {
            if (distToBurrow < allowHunting)
            {
                Debug.Log("hunt, dist to burrows " + distToBurrow);
                StartCoroutine(delayTransitionToNone(turnTime * 0.1f));
                ExecuteHunt(turnIndex, turnTime);
            } else
            {
                Debug.Log("give up hunting because " + distToBurrow + " >= " + allowHunting);
                hasAttacked = true;
                behaviour = EnemyMode.Hiding;
                StartCoroutine(delayTransitionToNone(turnTime * 0.1f));
                ExecuteHiding(turnIndex, turnTime);
            }
        }

        return EnemyMode.Attack1;
    }

    IEnumerator AnimateAttack1(PlayerController player, float afterBiteStart)
    {
        PlayerDestructable destruct = player.GetComponent<PlayerDestructable>();
        yield return StartCoroutine(LookTowards((player.onTile - pos).NineDirection));
        anim.SetTrigger("Bite");
        hasAttacked = true;
        yield return new WaitForSeconds(afterBiteStart);
        destruct.Hurt(Random.Range(activeTier.minAttack[0], activeTier.maxAttack[0]), 0);
        behaviour = EnemyMode.None;
    }

    [SerializeField]
    Vector3 attack2Offset;

    void UpdateBurrows(GridPos burrow)
    {
        if (lastBurrow != burrow)
        {
            if (board.IsValidPosition(burrow))
            {
                claimedBurrows.Add(burrow);
            }
            if (claimedBurrows.Contains(lastBurrow))
            {
                claimedBurrows.Remove(lastBurrow);
            }
            else
            {
                if (lastBurrow.x >= 0 && lastBurrow.y >= 0)
                {
                    Debug.LogWarning("Failed to release burrows: " + lastBurrow);
                }
            }
            lastBurrow = burrow;
        }
    }

    protected override EnemyMode ExecuteAttack2(int turnIndex, float turnTime)
    {
        transform.localPosition = board.GetLocalPosition(pos) + attack2Offset;
        underground = false;
        if (previousBehaviour == EnemyMode.Attack2)
        {
            hasAttacked = true;
            DirtBallsManager.instance.Throw(pos, player.onTile, Random.Range(activeTier.minAttack[1], activeTier.maxAttack[1]));
        }
        StartCoroutine(delayTransitionToNone(turnTime * 0.1f));
        return EnemyMode.Attack2;
    }

    public IEnumerator<WaitForSeconds> delayTransitionToNone(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        behaviour = EnemyMode.None;
    }

    protected override EnemyMode ExecuteHiding(int turnIndex, float turnTime)
    {
        if (inHole)
        {
            underground = true;
            transform.localPosition = board.GetLocalPosition(pos) + hidingOffset;            
        } else
        {
            if (targetCheckpoints.Count == 0)
            {
                targetCheckpoints.Add(lastBurrow);
            }
            behaviour = EnemyMode.Walking;
            ExecuteWalking(turnIndex, turnTime);
        }
        return EnemyMode.Hiding;
    }

    protected override EnemyMode ExecuteWalking(int turnIndex, float turnTime)
    {

        //Debug.Log(string.Format("Mole in hole {0} and previously {1}", inHole, previousBehaviour));
        if (inHole && previousBehaviour == EnemyMode.Walking)
        {
            behaviour = EnemyMode.Hiding;
            ExecuteHiding(turnIndex, turnTime);
            return EnemyMode.Walking;
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
                //Debug.Log("Player based selection");
            }
            else {
                
                bool selectedHole = false;

                if (Random.value < prefNearbyHole)
                {

                    preferred = lvl.boardHoles
                        .Where(e => e != pos && !claimedBurrows.Contains(e))
                        .Where(e => GridPos.ChessBoardDistance(e, pos) <= nearbyHoleDist)
                        .ToArray();

                    if (preferred.Length > 0)
                    {
                        underground = false;
                        target = preferred.Shuffle().First();
                        selectedHole = true;
                        //Debug.Log("Proximity selection");
                    }

                }
                    
                if (!selectedHole)
                {
                    //Redoing hole filtering is indeed neccesary incase attempted nearby hole without finding one
                    preferred = lvl.boardHoles.Where(e => e != pos && !claimedBurrows.Contains(e)).ToArray();    
                    target = preferred.Shuffle().First();
                    underground = true;
                    //Debug.Log("Random hole");
                }

            }
            //Debug.Log("Selected burrows" + target);
            targetCheckpoints.Add(target);
            SetTargetDistances(0);
            if (underground)
            {
                hasAttacked = false;
            }
            UpdateBurrows(target);
            Debug.Log("Claimed burrows " + claimedBurrows.Count);
        }

        if (underground)
        {
            SetUndergroundContextFromDistansMapAndPosition(targetDistanceMap);
        }
        else {

            SetContextFromDistanceMapAndPosition(targetDistanceMap);
        }
        SelectContextDirectionAndMove(turnTime);
        return EnemyMode.Walking;
    }

    void SetUndergroundContextFromDistansMapAndPosition(int[,] distances)
    {
        context = distances.GetCenteredContext(3, pos);
        contextPosition = pos;

        GridPos contextTarget = lastBurrow - pos + new GridPos(1, 1);

        //Set all holes and such as impassable
        //context = context.Map(e => e == 0 ? -1 : e);

        //If target is in context give that distance zero
        if (contextTarget.x >= 0 && contextTarget.x < 2 && contextTarget.y >= 0 && contextTarget.y < 2)
        {
            //context[contextTarget.x, contextTarget.y] = 0;
        }
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
        int taxiDistance = GridPos.ChessBoardDistance(a, b);
        GridPos offset = b - a;
        return taxiDistance <= throwLength && (offset.x == 0 || offset.y == 0 || Mathf.Abs(offset.x) == Mathf.Abs(offset.y)) && lvl.nonBlocking.LineInOneRegion(a, b);
    }

    [SerializeField]
    Vector3 undergroundOffset;

    [SerializeField]
    Vector3 hidingOffset;

    protected override void Move(GridPos offset, float maxTime)
    {
        jumpHeightAxis = new Vector3(Mathf.Abs(jumpHeightAxis.x), Mathf.Abs(jumpHeightAxis.y), Mathf.Abs(jumpHeightAxis.z)) * (underground ? -1f : 1);
        localPlacementOffset = underground ? undergroundOffset : Vector3.forward;
        base.Move(offset, maxTime);
    }

    void OnDestroy()
    {
        pos = new GridPos(-1, -1);
        if (claimedBurrows.Contains(lastBurrow))
        {
            claimedBurrows.Remove(lastBurrow);
        }
    }
}

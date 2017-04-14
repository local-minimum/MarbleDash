using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.TurnBased;
using LocalMinimum.Grid;

public class CoinBox : MonoBehaviour {

    Destructable destructable;

    [SerializeField, Range(0, 1)]
    float lootProbability = 1f;

    [SerializeField]
    int minCoin;

    [SerializeField]
    int maxCoin;

    [SerializeField]
    Material[] MaterialSequencePrefabs;

    Material[] mats = new Material[3];

    MeshRenderer mr;

    [SerializeField]
    Vector3 placementOffset;

    private void Awake()
    {
        destructable = GetComponent<Destructable>();
        
        mr = GetComponent<MeshRenderer>();
        for (int i = 0; i < MaterialSequencePrefabs.Length; i++)
        {
            mats[i] = Instantiate(MaterialSequencePrefabs[i]);
        }
        mr.material = mats[0];
        lvl = Level.instance;
        board = BoardGrid.instance;
        dislodgeAngle = PlayerRunData.stats.lvlRnd.Range(dislodgeAngleMin, dislodgeAngleMax);
        turnsActive = GetComponent<TurnsActive<BoxStates>>();
    }

    Level lvl;
    TurnsManager turnManger;
    
    float dislodgeAngle = 12;

    [SerializeField, Range(0, 20)]
    float dislodgeAngleMin = 12;

    [SerializeField, Range(0, 20)]
    float dislodgeAngleMax = 12;

    [SerializeField, Range(0, 1)]
    float slideProbability = 0.75f;

    bool inPlay = true;

    GridPos target;
    BoardGrid board;

    public System.Func<int, float, BoxStates> SelectAction(int turnIndex, float tickTime, out int turns)
    {
        turns = 1;

        if (inPlay)
        {
            Vector2 tilt = BoardController.instance.DelayedTilt;

            if (tilt.x > dislodgeAngle)
            {
                if (tilt.y > dislodgeAngle)
                {
                    target = pos.NorthEast;
                }
                else if (tilt.y < -dislodgeAngle)
                {
                    target = pos.SouthEast;
                }
                else
                {
                    target = pos.East;
                }
            }
            else if (tilt.x < -dislodgeAngle)
            {
                if (tilt.y > dislodgeAngle)
                {
                    target = pos.NorthWest;
                }
                else if (tilt.y < -dislodgeAngle)
                {
                    target = pos.SouthWest;
                }
                else
                {
                    target = pos.West;
                }
            }
            else
            {
                if (tilt.y > dislodgeAngle)
                {
                    target = pos.North;
                }
                else if (tilt.y < -dislodgeAngle)
                {
                    target = pos.South;
                }
                else
                {
                    return Staying;

                }
            }


            if (!board.IsValidPosition(target) || 
                board.HasOccupancyAny(target, Occupancy.Obstacle, Occupancy.Enemy, Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory) || 
                turnsActive.MostRecentAction != BoxStates.Sliding && Random.value > slideProbability)
            {
                return Staying;
            }
            else
            {
                turns = 2;
                return Sliding;
            }
        } else
        {            
            BoardGrid.instance.Free(pos, Occupancy.Obstacle);
            Debug.Log("Coin box fell down hole");
            Destroy(gameObject, 0.5f);
            return Breaking;
        }
    }

    [SerializeField]
    AnimationCurve planarStart;

    [SerializeField]
    AnimationCurve sliding;

    TurnsActive<BoxStates> turnsActive;

    public BoxStates Sliding(int turnIndex, float tickTime)
    {
        board.Free(pos, Occupancy.Obstacle);
        board.Occupy(target, Occupancy.Obstacle);
        lvl.EnqueueConnecitivityReconstruction();
        Debug.Log(turnsActive.GetMostRecentSelections(BoxStates.Sliding));

        if (turnsActive.GetMostRecentSelections(BoxStates.Sliding) < 1)
        {
            TurnsMover.instance.Move(turnsActive, pos, target, planarStart, 2, 0.05f, 0f, MoveCallback);

        }
        else {
            TurnsMover.instance.Move(turnsActive, pos, target, sliding, 2, 0f, 0f, MoveCallback);
        }
        pos = target;
        return BoxStates.Sliding;
    }

    public void MoveCallback()
    {        
        if (board.HasOccupancy(target, Occupancy.Hole))
        {
            //TODO: Drop box;
            gameObject.AddComponent<Rigidbody>();
            GetComponent<BoxCollider>().size = Vector3.one * 0.25f;
            inPlay = false;
        }
    }

    public BoxStates Staying(int turnIndex, float tickTime)
    {
        return BoxStates.Standing;
    }

    public BoxStates Breaking(int turnIndex, float tickTime)
    {
        return BoxStates.Breaking;
    }



    GridPos pos;

    public void SetPosition(GridPos pos)
    {
        transform.localPosition = BoardGrid.instance.GetLocalPosition(pos) + placementOffset;
        mr.material = mats[0];

        this.pos = pos;
        if (!BoardGrid.instance.HasOccupancy(pos, Occupancy.Obstacle)) {
            BoardGrid.instance.Occupy(pos, Occupancy.Obstacle);
        }
    }

    [SerializeField]
    Color healthyColor = Color.white;

    [SerializeField]
    Color deadColor = Color.black;

    void Crack(int amount)
    {
        if (destructable.PartialHealth < 0.4f)
        {
            mr.material = mats[2];
        } else if (destructable.PartialHealth < 0.75f)
        {
            mr.material = mats[1];
        }

        mr.material.color = Color.Lerp(deadColor, healthyColor, destructable.PartialHealth);

        //TODO: Animate to cracked state if not there
    }

    void Break(int amount)
    {
        inPlay = false;        
        PlayerRunData.stats.boxesBroken++;

        if (Random.value < lootProbability)
        {

            CoinFountains.instance.ShowerMe(transform);
            int coin = Random.Range(minCoin, maxCoin);
            if (coin > 0)
            {
                PlayerRunData.stats.Coin += coin;
            }
        } else
        {

            DustMaker.instance.ShowerMe(transform);
        }
        mr.material = mats[2];
        mr.material.color = deadColor;

        BoardGrid.instance.Free(pos, Occupancy.Obstacle);
        Level.instance.ReconstructConnectivities(Level.ConnectivityTypes.AboveGround);

        transform.localScale = new Vector3(
            transform.localScale.x * 1.1f,
            transform.localScale.y * 0.02f,
            transform.localScale.z * 1.1f);

        GetComponent<Collider>().enabled = false;
        turnsActive.ForceInterrupt();
        Destroy(gameObject, .5f);
    }
}

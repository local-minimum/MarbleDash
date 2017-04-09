using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    }

    Level lvl;

    private void OnEnable()
    {
        if (lvl == null)
        {
            lvl = Level.instance;
        }
        lvl.OnTurnTick += Lvl_OnTurnTick;
    }

    private void OnDisable()
    {
        lvl.OnTurnTick -= Lvl_OnTurnTick;
    }


    [SerializeField, Range(0, 20)]
    float dislodgeAngle = 12;

    [SerializeField, Range(0, 1)]
    float slideProbability = 0.75f;

    private void Lvl_OnTurnTick(PlayerController player, int turnIndex, float tickTime)
    {
        Vector2 tilt = BoardController.instance.DelayedTilt;
        GridPos target;
        Debug.Log(tilt);
        if (tilt.x > dislodgeAngle)
        {
            if (tilt.y > dislodgeAngle)
            {
                target = pos.SouthEast;
            } else if (tilt.y < -dislodgeAngle)
            {
                target = pos.NorthEast;
            } else
            {
                target = pos.East;
            }
        } else if (tilt.x < -dislodgeAngle)
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
        } else
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
                return;
            }
        }

        BoardGrid board = BoardGrid.instance;

        if (!board.IsValidPosition(target) || board.HasOccupancyAny(target, Occupancy.Obstacle, Occupancy.Enemy, Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory) || Random.value > slideProbability)
        {
            //Can't move there
        } else
        {
            board.Free(pos, Occupancy.Obstacle);
            board.Occupy(target, Occupancy.Obstacle);            
            SetPosition(target);
            lvl.EnqueueConnecitivityReconstruction();

            pos = target;

            if (board.HasOccupancy(target, Occupancy.Hole))
            {
                //TODO: Drop box;
                gameObject.AddComponent<Rigidbody>();
                GetComponent<BoxCollider>().size = Vector3.one * 0.25f;                
            }
        }
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
        Destroy(gameObject, .5f);
    }
}

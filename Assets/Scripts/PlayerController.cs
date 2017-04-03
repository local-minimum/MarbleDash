using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Grid;
using LocalMinimum.Arrays;

public class PlayerController : MonoBehaviour {

    static PlayerController _instance;

    public static PlayerController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerController>();
            }
            return _instance;
        }
    }

    [SerializeField]
    BoardController board;

    Rigidbody rb;

    bool grounded = false;

    public bool Grounded
    {
        get
        {
            return grounded;
        }
    }

    Destructable destructable;
    BoardGrid boardGrid;
    Level lvl;

    private void Awake()
    {
        if (_instance == this || _instance == null)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start () {
        rb = GetComponent<Rigidbody>();
        groundLayer = LayerMask.NameToLayer("ground");
        destructableLayer = LayerMask.NameToLayer("destructables");
        destructable = GetComponent<Destructable>();
        boardGrid = board.GetComponent<BoardGrid>();
    }

    [SerializeField, Range(0, 5)]
    float forceMultiplier = 0.5f;

    public void Inert()
    {
        rb.velocity = Vector3.zero;
    }

    public void Freeze()
    {
        Inert();
        rb.isKinematic = true;
    }

    public void Thaw()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.WakeUp();
    }

    public void KillReset(string message)
    {
        onTile = offTile;
        collidingGrounds.Clear();
        StoreSwapper.instance.HideAllStores();
        PlayerRunData.stats.Reset();
        lvl.Generate();
        lvl.Implement();
    }

    void OnEnable()
    {
        if (lvl == null)
        {
            lvl = Level.instance;
            lvl.OnNewLevel += Lvl_OnNewLevel;
        }

    }

    void OnDisable()
    {
        if (lvl)
        {
            lvl.OnNewLevel -= Lvl_OnNewLevel;
        }
    }

    private void Lvl_OnNewLevel()
    {
        collidingGrounds.Clear();
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.H))
        {
            switch (PlayerRunData.stats.holeMode)
            {
                case PlayModeHoles.Damage:
                    PlayerRunData.stats.holeMode = PlayModeHoles.InstaKill;
                    break;
                case PlayModeHoles.InstaKill:
                    PlayerRunData.stats.holeMode = PlayModeHoles.Damage;
                    break;
            }
        }

        if (grounded)
        {
            if (!rb.isKinematic)
            {
                if (HasTracktion())
                {
                    rb.AddForce(board.Slope * forceMultiplier);
                }
                TrackGridPos();
            } else
            {
                Debug.Log("Player kinematic");
            }
        } else
        {
            onTile = offTile;
            Debug.Log("Player not grounded");
        }
     
    }

    bool HasTracktion()
    {
        return boardGrid.OnBoard(_onTile) && !boardGrid.HasOccupancy(_onTile, Occupancy.NoGrip);
    }

    void TrackGridPos()
    {

        int closest = -1;
        BoardTile closestBoard = null;
        float sqDist = 0;
        for (int i=0, l=collidingGrounds.Count; i<l; i++)
        {
            float curSqDist = (collidingGrounds[i].position - transform.position).sqrMagnitude;
            BoardTile curBoard = collidingGrounds[i].GetComponent<BoardTile>();

            if (curBoard != null && (closest < 0 || curSqDist < sqDist))
            {
                closest = i;
                closestBoard = curBoard;
            }
        }

        if (closest == -1 || closestBoard == null)
        {
            onTile = offTile;
            Debug.Log("Player off tile because closest " + closest + " or no closestBoard " + closestBoard);
        } else
        {
            Debug.Log("Player on tile " + closestBoard + " with pos " + closestBoard.pos);
            onTile = closestBoard.pos;
        }
        
    }

    int groundLayer;
    int destructableLayer;

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == destructableLayer)
        {
            Destructable otherDest = collision.gameObject.GetComponent<Destructable>();
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy)
            {
                
                int reflectedDamage = 0;
                int hitPart;
                int attack = destructable.GetVelocityForce();
                if (!enemy.AllowsAttack(collision.contacts, attack, out reflectedDamage, out hitPart))
                {
                    AttackingMesh am = collision.gameObject.GetComponentInChildren<AttackingMesh>();
                    if (am)
                    {
                        int dmg = am.RollDamage;
                        //Should be addition because you get reflection hurt and enemy attack you hurt
                        destructable.Hurt(dmg + reflectedDamage, hitPart);
                    } else
                    {
                        if (reflectedDamage > 0)
                        {
                            destructable.Hurt(reflectedDamage, hitPart);
                        }
                    }

                } else if (otherDest.GetVelocityForce() < destructable.GetVelocityForce()) {
                    otherDest.Hurt(attack - reflectedDamage, hitPart);
                } else
                {
                    Debug.Log("Hurting " + otherDest.name + " refused");
                }
                if (reflectedDamage > 0) {
                    //Invoke self hurt
                    destructable.Hurt(reflectedDamage, 0);
                }
            }
            else
            {
                otherDest.Hurt(destructable.GetVelocityForce(), 0);
            }
        }
    }

    public int[,] enemyDistancesCross;
    public int[,] enemyDistancesEight;
     
    GridPos offTile = new GridPos(-1, -1);
    GridPos _onTile = new GridPos(-1, -1);

    public GridPos onTile
    {
        get
        {
            return _onTile;
        }

        private set
        {
            if (_onTile != value)
            {
                if (boardGrid.IsValidPosition(_onTile))
                {                    
                    boardGrid.Free(_onTile, Occupancy.Player);
                }
                _onTile = value;
                if (boardGrid.IsValidPosition(value))
                {
                    boardGrid.Occupy(value, Occupancy.Player);
                    if (lvl != null && lvl.enemyConnectivityCross != null)
                    {
                        enemyDistancesCross = lvl.enemyConnectivityCross.HasValue(lvl.enemyConnectivityCross[value.x, value.y]).Distance(value);
                        enemyDistancesEight = lvl.enemyConnectivityEight.HasValue(lvl.enemyConnectivityEight[value.x, value.y]).Distance(value, LocalMinimum.Arrays.Neighbourhood.Eight);
                    } else
                    {
                        Debug.LogWarning("Not a level so no connectivity");
                    }
                    Debug.Log("Player on tile " + value);
                } else
                {
                    Debug.Log("Player not on board " + value);
                }
            }
        }
    }

    List<Transform> collidingGrounds = new List<Transform>();

    private void OnCollisionStay(Collision collision)
    {
        if (groundLayer == collision.gameObject.layer)
        {
            grounded = true;

            if (!collidingGrounds.Contains(collision.transform))
            {
                collidingGrounds.Add(collision.transform);
            }            
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (groundLayer == collision.gameObject.layer)
        {
            collidingGrounds.Remove(collision.transform);
            grounded = collidingGrounds.Count > 0;
        }
    }

    public void EmoteStatus()
    {
        StartCoroutine(delayEmote());
    }

    IEnumerator<WaitForSeconds> delayEmote()
    {
        yield return new WaitForSeconds(1f);
        destructable.Emit();
    }

    public void HurtMe(int amount)
    {
        PlayerRunData.stats.damageTaken += amount;
        Debug.Log("Player hurt");
    }

    public void KillMe(int amount)
    {        
        PlayerRunData.stats.damageTaken += amount;
        Debug.Log("Player dead");
        KillReset("");
    }

#if UNITY_EDITOR

    [SerializeField]
    Vector3 gizmoOffset;

    public enum GizmoContent { EnemyCross, EnemyEight};

    [SerializeField]
    GizmoContent gizmoContent;

    private void OnDrawGizmosSelected()
    {

        int[,] connectivity;
        switch (gizmoContent)
        {
            case GizmoContent.EnemyCross:
                connectivity = enemyDistancesCross;
                break;
            case GizmoContent.EnemyEight:
                connectivity = enemyDistancesEight;
                break;
            default:
                connectivity = null;
                break;
        }

        if (connectivity == null)
        {
            return;
        }

        int size = boardGrid.Size;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {

                Gizmos.DrawIcon(boardGrid.transform.TransformPoint(boardGrid.GetLocalPosition(x, y)) + gizmoOffset, "numberIcon_" + (connectivity[x, y] < 21 ? connectivity[x, y].ToString() : "plus") + ".png", true);
            }
        }
    }

#endif

}

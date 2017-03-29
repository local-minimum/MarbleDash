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
        lvl = Level.instance;
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
        StoreSwapper.instance.HideAllStores();
        PlayerRunData.stats.Reset();
        lvl.Generate();
        lvl.Implement();
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
                rb.AddForce(board.Slope * forceMultiplier);
                TrackGridPos();
            }
        } else
        {
            onTile = offTile;
        }
     
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

        if (closest < 0 || closestBoard == null)
        {
            onTile = _onTile;
        } else
        {
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
                    enemyDistancesCross = lvl.enemyConnectivity4.HasValue(lvl.enemyConnectivity4[value.x, value.y]).Distance(value);
                    enemyDistancesEight = lvl.enemyConnectivity8.HasValue(lvl.enemyConnectivity4[value.x, value.y]).Distance(value, LocalMinimum.Arrays.Neighbourhood.Eight);
                    Debug.Log("Player on tile " + value);
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
        Debug.Log("Player should be dead");
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

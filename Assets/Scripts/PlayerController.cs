using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Destructable destructable;
    BoardGrid boardGrid;

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
    }

    public void KillReset(string message)
    {
         
    }

	void Update () {
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
                
                int deflectedHurt = 0;
                if (otherDest.GetVelocityForce() < destructable.GetVelocityForce() && enemy.AllowsAttack(collision.contacts, out deflectedHurt)) {
                    otherDest.Hurt(destructable.GetVelocityForce() - deflectedHurt);
                } else
                {
                    Debug.Log("Hurting " + otherDest.name + " refused");
                }
                if (deflectedHurt > 0) {
                    destructable.Hurt(deflectedHurt);
                }
            }
            else
            {
                otherDest.Hurt(destructable.GetVelocityForce());
            }
        }
    }

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
        
        Debug.Log("Player hurt");
    }

    public void KillMe(int amount)
    {
        Debug.Log("Player should be dead");
    }
}

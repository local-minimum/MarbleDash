using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum EnemyMode {Standing, Patroling, Hunting, Attacking};


public class Enemy : MonoBehaviour {

    #region Tiers

    [SerializeField]
    EnemyTier[] tiers;

    [SerializeField]
    EnemyTier activeTier;

    public void SetTier(int tier)
    {
        activeTier = tiers[tier];
        bodyProperties.SetInitial(activeTier);
    }

    public List<KeyValuePair<int, int>> GetTiersInDifficutlyRange(int min, int max)
    {
        List<KeyValuePair<int, int>> filteredTiers = new List<KeyValuePair<int, int>>();
        for (int i=0; i<tiers.Length; i++)
        {
            if (tiers[i].difficulty <= max && tiers[i].difficulty >= min)
            {
                filteredTiers.Add(new KeyValuePair<int, int>(i, tiers[i].difficulty));
            }
        }
        return filteredTiers;
    }

    #endregion

    [SerializeField]
    Destructable bodyProperties;

    Level lvl;

    int playerLayer;

    bool attackedThisTurn;

    EnemyMode behaviour = EnemyMode.Patroling;

    public EnemyMode Behaviour
    {
        get
        {
            return behaviour;
        }
    }

    bool isAlive = true;

    GridPos pos = new GridPos(-1, -1);
    BoardGrid board;

    [SerializeField]
    Vector3 localPlacementOffset = new Vector3(0, 0, 0.5f);

    [SerializeField]
    string attackAnim;

    [SerializeField]
    int attackRange = 2;

    public void SetPosition(GridPos pos, BoardGrid board)
    {        
        this.pos = pos;
        this.board = board;

        if (!board.HasOccupancy(pos, Occupancy.Enemy))
        {
            Debug.Log(string.Format("Setting {0} for {1} pos {2} '{3}'",
                Occupancy.Enemy, name, pos, string.Join(",", board.GetOccupancy(pos).Select(e => e.ToString()).ToArray())));
            board.Occupy(pos, Occupancy.Enemy);
            Debug.Log(string.Format("'{0}'", string.Join(",", board.GetOccupancy(pos).Select(e => e.ToString()).ToArray())));


        }
        transform.localPosition = board.GetLocalPosition(pos) + localPlacementOffset;     
    }

   
    Animator anim;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        lvl = Level.instance;
        lvl.OnTurnTick += Lvl_OnTurnTick;   
    }

    private void OnDisable()
    {
        lvl.OnTurnTick -= Lvl_OnTurnTick;  
    }


    private void Lvl_OnTurnTick(PlayerController player, float turntime)
    {
        attackedThisTurn = false;
        GridPos playerDirection = (player.onTile - pos);
        if (playerDirection.EightMagnitude <= attackRange)
        {
            Attack(playerDirection);
        }
        else if (behaviour == EnemyMode.Hunting)
        {
            ExecuteHunt(player, turntime);        
        } else if (behaviour == EnemyMode.Patroling)
        {
            ExecutePatrol(turntime);
        } else
        {
            ExecuteRest(turntime);
        }
    }

    protected virtual void ExecuteRest(float turnTime)
    {
        behaviour = EnemyMode.Patroling;
    }

    protected virtual void ExecuteHunt(PlayerController player, float turnTime)
    {
        GridPos playerDirection = (player.onTile - pos);
        int[,] context = DirectionFilteredContext(
        board.GetOccupancyContext(pos, Occupancy.Free, Occupancy.BallPath, Occupancy.Player),
        playerDirection.NineNormalized);

        GridPos moveDirection = SelectMoveOffset(BoardGrid.ContextToOffsets(context));
        Move(moveDirection, turnTime);
        
    }

    int[,] DirectionFilteredContext(int[,] context, GridPos direction)
    {
        direction += new GridPos(1, 1);
        for (int x=0; x<3; x++)
        {
            for (int y=0; y<3; y++)
            {
                if (context[x, y] == 1 && Mathf.Abs(direction.x - x) + Mathf.Abs(direction.y - y) > 1)
                {
                    context[x, y] = 0;
                }
            }
        }

        return context;
    }

    protected virtual void ExecutePatrol(float turnTime)
    {
        
        int[,] context = board.GetNotOccupancyContext(pos,
            Occupancy.BallPathTarget,
            Occupancy.Enemy,
            Occupancy.Obstacle,
            Occupancy.Wall,
            Occupancy.WallBreakable,
            Occupancy.WallIllusory,
            Occupancy.Hole);
        GridPos moveDirection = SelectMoveOffset(BoardGrid.ContextToOffsets(context));
        Move(moveDirection, turnTime);
    }

    protected virtual GridPos SelectMoveOffset(List<GridPos> offsets)
    {
        if (offsets.Count == 0)
        {
            behaviour = EnemyMode.Standing;
            return new GridPos(0, 0);
        }
        return offsets[Random.Range(0, offsets.Count)];
    }

    [SerializeField]
    float attackDelay = 0.4f;

    protected virtual void Attack(GridPos playerDirection)
    {
        behaviour = EnemyMode.Attacking;
        Debug.Log("Attacks");
        StartCoroutine(LookTowards(playerDirection));

        if (anim)
        {
            StartCoroutine(DelayAttackTrigger(attackDelay));
        }

    }

    IEnumerator<WaitForSeconds> DelayAttackTrigger(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetTrigger(attackAnim) ;
        yield return new WaitForSeconds(delay);
        behaviour = EnemyMode.Hunting;
    }

    protected virtual void Move(GridPos offset, float maxTime)
    {
        board.Free(pos, Occupancy.Enemy);
        pos += offset;
        if (!board.HasOccupancy(pos, Occupancy.Enemy))
        {
            board.Occupy(pos, Occupancy.Enemy);
            StartCoroutine(JumpToPos(maxTime, board.GetLocalPosition(pos) + localPlacementOffset));
        } else
        {
            pos -= offset;
            board.Occupy(pos, Occupancy.Enemy);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            behaviour = EnemyMode.Hunting;
        }
    }

    public virtual bool AllowsAttack(ContactPoint[] contactPoints, out int reflectedDamage)
    {
        reflectedDamage = 0;
        return !attackedThisTurn && behaviour != EnemyMode.Attacking;
    }

    public virtual void HurtEffect(int amount)
    {
        FloatTextManager.ShowText(transform, amount.ToString());
        attackedThisTurn = true;
        Splatterer.instance.SplatMe(transform);
        Debug.Log("Hurt " + name);
    }

    public virtual void KilledEffect(int amount)
    {
        FloatTextManager.ShowText(transform, amount.ToString());
        attackedThisTurn = true;
        isAlive = false;
        board.Free(pos, Occupancy.Enemy);
        Splatterer.instance.SplatMe(transform);
        EnemySpawner.instance.iDied(this);
        Destroy(gameObject, 0.1f);
        Debug.Log("Killed " + name);
    }

    [SerializeField, Range(0, 1)]
    float jumpFractionDuration = 0.8f;

    [SerializeField]
    AnimationCurve heightCurve;

    [SerializeField]
    AnimationCurve planarCurve;

    [SerializeField]
    Vector3 jumpHeightAxis = Vector3.up;

    protected IEnumerator<WaitForSeconds> LookTowards(GridPos direction)
    {
        Direction dir = direction.AsMajorDirection();
        Quaternion target = Quaternion.identity;
        Quaternion source = transform.localRotation;
        switch (dir)
        {
            case Direction.East:
                target = Quaternion.AngleAxis(270, -Vector3.forward);
                break;
            case Direction.West:
                target = Quaternion.AngleAxis(90, -Vector3.forward);
                break;
            case Direction.South:
                target = Quaternion.AngleAxis(0, -Vector3.forward);
                break;
            case Direction.North:
                target = Quaternion.AngleAxis(180, -Vector3.forward);
                break;
            default:
                target = source;
                break;
        }

        if (source != target)
        {
            float p = 0;
            for (int i=0; i<11; i++)
            {
                transform.localRotation = Quaternion.LerpUnclamped(source, target, p);
                p += 0.1f;
                yield return new WaitForSeconds(0.016f);
            }
            transform.localRotation = target;
        }
    }

    protected IEnumerator<WaitForSeconds> JumpToPos(float maxTime, Vector3 targetPos)
    {
        float startTime = Time.timeSinceLevelLoad;
        float duration = maxTime * jumpFractionDuration;
        float progress = 0;
        Vector3 startPos = transform.localPosition;
        while (progress < 1)
        {
            progress = (Time.timeSinceLevelLoad - startTime) / duration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, planarCurve.Evaluate(progress)) + jumpHeightAxis * heightCurve.Evaluate(progress);
            yield return new WaitForSeconds(0.016f);
        }

        transform.localPosition = targetPos; 

    }
}

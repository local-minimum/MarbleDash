using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum EnemyMode {None, Standing, Walking, Patroling, Hunting, Tracking, Homing, Haste,
    Attack1, Attack2, Attack3, Attack4, Attack5};


public class Enemy : MonoBehaviour {

    #region Tiers

    [SerializeField]
    EnemyTier[] tiers;
    
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

    public int GetDifficulty(int tier)
    {
        return tiers[tier].difficulty;
    }

    #endregion

    [SerializeField]
    EnemyDestructable bodyProperties;

    Level lvl;

    int playerLayer;

    EnemyMode behaviour = EnemyMode.Patroling;

    public EnemyMode Behaviour
    {
        get
        {
            return behaviour;
        }
    }

    #region Attack

    bool attackedThisTurn;

    public bool IsAttacking
    {
        get
        {
            return IsAttack(behaviour);
        }
    }

    bool IsAttack(EnemyMode behaviour)
    {
        return behaviour == EnemyMode.Attack1 || behaviour == EnemyMode.Attack2 || behaviour == EnemyMode.Attack3 ||
            behaviour == EnemyMode.Attack4 || behaviour == EnemyMode.Attack5;
    }

    public int GetAttackStrength()
    {        
        int idAttack = GetAttackModeAsIndex(behaviour);
        if (idAttack < 0)
        {
            return 0;
        } else 
        {
            int min = activeTier.minAttack[activeTier.minAttack.Length - 1];
            if (activeTier.minAttack.Length < idAttack)
            {
                min = activeTier.minAttack[idAttack];
            }

            int max = activeTier.maxAttack[activeTier.maxAttack.Length - 1];
            if (activeTier.maxAttack.Length < idAttack)
            {
                max = activeTier.maxAttack[idAttack];
            }

            return Random.Range(min, max);
        }
    }

    int GetAttackModeAsIndex(EnemyMode mode)
    {
        int idAttack = 0;

        if (IsAttack(mode))
        {
            for (int i = 0, l = activeTier.availableModes.Length; i < l; i++)
            {
                if (mode == activeTier.availableModes[i])
                {
                    return idAttack;
                } else if (IsAttack(activeTier.availableModes[i]))
                {
                    idAttack++;
                }
            }
        }
        return -1;
    }

    [SerializeField]
    string[] attackAnim;

    void TriggerAttackAnimation()
    {
        int attackIndex = GetAttackModeAsIndex(behaviour);
        if (attackIndex >= 0 && attackIndex < attackAnim.Length)
        {
            anim.SetTrigger(attackAnim[attackIndex]);
        }
    }

    #endregion

    bool isAlive = true;

    GridPos pos = new GridPos(-1, -1);
    BoardGrid board;

    [SerializeField]
    Vector3 localPlacementOffset = new Vector3(0, 0, 0.5f);

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

    private void Lvl_OnTurnTick(PlayerController player, int turnIndex, float turnTime)
    {
        attackedThisTurn = false;
        if (!ForceBehaviourSequence())
        {
            GridPos playerOffset = (player.onTile - pos);
            //Debug.Log(playerOffset.EightMagnitude);
            var availableModes = activeTier.availableModes
                .Select((e, i) => new KeyValuePair<int, EnemyMode>(i, e))
                .Where(o => !OnCoolDown(o.Key, turnIndex) && PlayerInRange(playerOffset, o.Value))
                .ToList();

            var attacks = availableModes.Where(o => IsAttack(o.Value)).ToList();

            if (attacks.Count > 0)
            {
                behaviour = SelectModeFromAvailable(attacks);
            } else
            {
                behaviour = SelectModeFromAvailable(availableModes.Where(o => !IsAttack(o.Value)).ToList());
            }

        }

        UpdateCoolDown(turnIndex);
        InvokeSelectedModeExecution(turnTime, player);
        //Debug.Log(behaviour);
    }

    void UpdateCoolDown(int turnId)
    {
        lastModeInvocation[behaviour] = turnId;
    }

    void InvokeSelectedModeExecution(float turnTime, PlayerController player)
    {
        switch (behaviour)
        {
            case EnemyMode.None:
                break;
            case EnemyMode.Standing:
                ExecuteStanding(turnTime);
                break;
            case EnemyMode.Patroling:
                ExecutePatroling(turnTime);
                break;
            case EnemyMode.Walking:
                ExecuteWalking(turnTime);
                break;
            case EnemyMode.Hunting:
                ExecuteHunt(player, turnTime);
                break;
            case EnemyMode.Homing:
                ExecutHoming(player, turnTime);
                break;
            case EnemyMode.Haste:
                ExecutHaste(player, turnTime);
                break;
            case EnemyMode.Attack1:
                ExecuteAttack1(player, turnTime);
                break;
            case EnemyMode.Attack2:
                ExecuteAttack2(player, turnTime);
                break;
            case EnemyMode.Attack3:
                ExecuteAttack3(player, turnTime);
                break;
            case EnemyMode.Attack4:
                ExecuteAttack4(player, turnTime);
                break;
            case EnemyMode.Attack5:
                ExecuteAttack5(player, turnTime);
                break;
        }
    }

    EnemyMode SelectModeFromAvailable(List<KeyValuePair<int, EnemyMode>> options)
    {
        int l = options.Count;
        float totWeights = 0;

        for (int i=0; i<l; i++)
        {
            totWeights += activeTier.behaviourProbWeights[options[i].Key];
        }

        float val = Random.Range(0, totWeights);

        for (int i=0; i<l; i++)
        {
            if (val <= activeTier.behaviourProbWeights[options[i].Key])
            {
                return options[i].Value;
            } else
            {
                val -= activeTier.behaviourProbWeights[options[i].Key];
            }
        }

        return options[l - 1].Value;
    }

    Dictionary<EnemyMode, int> lastModeInvocation = new Dictionary<EnemyMode, int>();

    protected virtual bool OnCoolDown(int modeIndex, int turnId)
    {
        EnemyMode mode = activeTier.availableModes[modeIndex];

        if (lastModeInvocation.ContainsKey(mode) && activeTier.behaviourCoolDowns.Length > modeIndex)
        {
            return turnId - lastModeInvocation[mode] < activeTier.behaviourCoolDowns[modeIndex];
        }

        return false;
    }

    [SerializeField]
    int huntRange = 4;

    protected virtual bool PlayerInRange(GridPos playerOffset, EnemyMode mode)
    {
        if (IsAttack(mode))
        {
            //TODO: Support more ranges or is that just overwrite
            return playerOffset.EightMagnitude <= attackRange;
        } else if (mode == EnemyMode.Hunting)
        {
            return playerOffset.EightMagnitude <= 4;
        } else
        {
            return true;
        }
    }

    protected virtual bool ForceBehaviourSequence()
    {
        //Implement transition states here if needed.
        return false;
    }

    #region EnemyModeExecutions

    protected virtual void ExecuteAttack2(PlayerController player, float turnTime)
    {

    }

    protected virtual void ExecuteAttack3(PlayerController player, float turnTime)
    {

    }

    protected virtual void ExecuteAttack4(PlayerController player, float turnTime)
    {

    }

    protected virtual void ExecuteAttack5(PlayerController player, float turnTime)
    {

    }

    protected virtual void ExecutHaste(PlayerController player, float turnTime)
    {

    }

    protected virtual void ExecutHoming(PlayerController player, float turnTime)
    {

    }

    protected virtual void ExecuteWalking(float turnTime)
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

    protected virtual void ExecuteStanding(float turnTime)
    {
       
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

    protected virtual void ExecutePatroling(float turnTime)
    {

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

    protected virtual void ExecuteAttack1(PlayerController player, float turnTime)
    {
        GridPos playerDirection = (player.onTile - pos);
       
        StartCoroutine(LookTowards(playerDirection));

        if (anim)
        {
            StartCoroutine(DelayAttackTrigger(attackDelay));
        }

    }
    #endregion

    [SerializeField]
    float attackDelay = 0.4f;

    IEnumerator<WaitForSeconds> DelayAttackTrigger(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerAttackAnimation();
        yield return new WaitForSeconds(delay);
        
        //To allow being hurt again
        behaviour = EnemyMode.None;
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

    public virtual bool AllowsAttack(ContactPoint[] contactPoints, int attack, out int reflectedDamage, out int bodyPart)
    {

        bodyPart = GetBodyPart(contactPoints);

        if (activeTier.damageReflection != null && activeTier.damageReflection.Length > bodyPart)
        {
            reflectedDamage = Mathf.Min(attack, activeTier.damageReflection[bodyPart]);
        } else
        {
            reflectedDamage = 0;
        }

        return !attackedThisTurn && !IsAttacking;
    }

    public int GetBodyPart(ContactPoint[] points)
    {
        //TODO: Body part detection from submeshes potentially
        return 0;
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

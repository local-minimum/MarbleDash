using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LocalMinimum.Grid;
using LocalMinimum.Arrays;
using LocalMinimum.Collections;

public enum EnemyMode {None, Standing, Walking, Patroling, Hunting, Tracking, Homing, Haste,
    Attack1, Attack2, Attack3, Attack4, Attack5, Hiding};


public class Enemy : MonoBehaviour {

    #region Tiers

    [SerializeField]
    EnemyTier[] tiers;
    
    protected EnemyTier activeTier;
    protected int activeTierIndex;

    public void SetTier(int tier)
    {
        activeTier = tiers[tier];
        activeTargetIndex = tier;
        bodyProperties.SetInitial(activeTier);
    }

    public List<KeyValuePair<int, int>> GetTiersInDifficutlyRange(Level lvl, int lvlIndex, int min, int max)
    {
        List<KeyValuePair<int, int>> filteredTiers = new List<KeyValuePair<int, int>>();

        if (SpecialCriteriaForSelectionFullfilled(lvl))
        {
            for (int i = 0; i < tiers.Length; i++)
            {
                if (tiers[i].firstLevel <= lvlIndex && tiers[i].difficulty <= max && tiers[i].difficulty >= min)
                {
                    filteredTiers.Add(new KeyValuePair<int, int>(i, tiers[i].difficulty));
                }
            }
        }
        return filteredTiers;
    }

    protected virtual bool SpecialCriteriaForSelectionFullfilled(Level lvl)
    {
        return true;
    }

    public int GetDifficulty(int tier)
    {
        return tiers[tier].difficulty;
    }

    public int GetMaxDifficulty()
    {
        return tiers[tiers.Length - 1].difficulty;
    }

    #endregion

    [SerializeField]
    EnemyDestructable bodyProperties;

    protected Level lvl;

    protected EnemyMode behaviour = EnemyMode.None;

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

    public bool IsAttack(EnemyMode behaviour)
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

    protected GridPos pos = new GridPos(-1, -1);
    protected BoardGrid board;

    [SerializeField]
    protected Vector3 localPlacementOffset = new Vector3(0, 0, 0.5f);

    [SerializeField]
    protected int attackRange = 2;

    protected GridPos contextPosition;
    protected int[,] context;

    protected List<GridPos> targetCheckpoints = new List<GridPos>();
    protected int activeTargetIndex;
    protected int[,] targetDistanceMap;
    int turnsWithThisAction;
    protected EnemyMode previousBehaviour = EnemyMode.None;

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

   
    protected Animator anim;

    void Awake()
    {
        lvl = Level.instance;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        Setup();
    }

    protected virtual void Setup()
    {

    }

    protected struct ModeStruct
    {
        public EnemyMode mode;
        public int index;

        public ModeStruct(EnemyMode mode, int index)
        {
            this.mode = mode;
            this.index = index;
        }
    }

    public void Lvl_OnTurnTick(PlayerController player, int turnIndex, float turnTime)
    {
        attackedThisTurn = false;
        if (!ForceBehaviourSequence())
        {
            GridPos playerOffset = (player.onTile - pos);
            //Debug.Log(playerOffset.EightMagnitude);
            var availableModes = activeTier.availableModes
                .Select((e, i) => new ModeStruct(e, i))
                .Where(o => !OnCoolDown(o.index, turnIndex) && PlayerInValidPosition(o.mode, player) && PlayerInRange(playerOffset, o.mode))
                .ToList();

            var attacks = availableModes.Where(o => IsAttack(o.mode)).ToList();

            if (attacks.Count > 0)
            {
                behaviour = SelectModeFromAvailable(attacks);
                //Debug.Log("Non forced attack behaviour" + behaviour);
            }
            else
            {
                behaviour = SelectModeFromAvailable(availableModes.Where(o => !IsAttack(o.mode)).ToList());
                //Debug.Log("Non forced non-behaviour " + behaviour);
            }

        } else
        {
            //Debug.Log("Forcing " + behaviour);
        }

        if (behaviour == previousBehaviour)
        {
            turnsWithThisAction++;
        } else
        {
            turnsWithThisAction = 1;
        }

        UpdateCoolDown(turnIndex);
        GetActionFunction(turnTime, player);

        previousBehaviour = behaviour;
        //Debug.Log(behaviour);
    }

    void UpdateCoolDown(int turnId)
    {
        lastModeInvocation[behaviour] = turnId;
    }

    public System.Func<PlayerController, int, float, EnemyMode> GetActionFunction(float turnTime, PlayerController player)
    {
        //Debug.Log(behaviour);

        switch (behaviour)
        {
            case EnemyMode.None:
                return null;
            case EnemyMode.Standing:
                return ExecuteStanding;
            case EnemyMode.Patroling:
                return ExecutePatroling;
            case EnemyMode.Walking:
                return ExecuteWalking;
            case EnemyMode.Hunting:
                return ExecuteHunt;
            case EnemyMode.Homing:
                return ExecuteHoming;
            case EnemyMode.Haste:
                return ExecutHaste;
            case EnemyMode.Attack1:
                return ExecuteAttack1;
            case EnemyMode.Attack2:
                return ExecuteAttack2;
            case EnemyMode.Attack3:
                return ExecuteAttack3;
            case EnemyMode.Attack4:
                return ExecuteAttack4;
            case EnemyMode.Attack5:
                return ExecuteAttack5;
            case EnemyMode.Hiding:
                return ExecuteHiding;
            default:
                return null;

        }
    }

    EnemyMode SelectModeFromAvailable(List<ModeStruct> options)
    {
        int l = options.Count;
        float totWeights = 0;

        for (int i=0; i<l; i++)
        {
            totWeights += activeTier.behaviourProbWeights[options[i].index];
        }

        float val = Random.Range(0, totWeights);

        for (int i=0; i<l; i++)
        {
            if (val <= activeTier.behaviourProbWeights[options[i].index])
            {
                return options[i].mode;
            } else
            {
                val -= activeTier.behaviourProbWeights[options[i].index];
            }
        }

        return options[l - 1].mode;
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

    protected virtual bool PlayerInValidPosition(EnemyMode mode, PlayerController player)
    {
        if (mode == EnemyMode.Homing || mode == EnemyMode.Hunting || mode == EnemyMode.Tracking)
        {
            return board.OnBoard(player.onTile) && lvl.enemyConnectivityEight[pos.x, pos.y] == lvl.enemyConnectivityEight[player.onTile.x, player.onTile.y];
        }

        return true;
    }

    protected virtual bool PlayerInRange(GridPos playerOffset, EnemyMode mode)
    {
        if (IsAttack(mode))
        {
            return playerOffset.ChessBoardMagnitude <= attackRange;
        } else if (mode == EnemyMode.Hunting)
        {
            return playerOffset.ChessBoardMagnitude <= huntRange && lvl.enemyConnectivityEight[pos.x, pos.y] == lvl.enemyConnectivityEight[pos.x + playerOffset.x, pos.y + playerOffset.y];
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

    protected virtual EnemyMode ExecuteHiding(PlayerController player, int turnIndex, float turnTime)
    {
        throw new System.NotImplementedException();
    }

    protected virtual EnemyMode ExecuteAttack2(PlayerController player, int turnIndex, float turnTime)
    {
        throw new System.NotImplementedException();
    }

    protected virtual EnemyMode ExecuteAttack3(PlayerController player, int turnIndex, float turnTime)
    {
        throw new System.NotImplementedException();
    }

    protected virtual EnemyMode ExecuteAttack4(PlayerController player, int turnIndex, float turnTime)
    {
        throw new System.NotImplementedException();
    }

    protected virtual EnemyMode ExecuteAttack5(PlayerController player, int turnIndex, float turnTime)
    {
        throw new System.NotImplementedException();
    }

    protected virtual EnemyMode ExecutHaste(PlayerController player, int turnIndex, float turnTime)
    {

        throw new System.NotImplementedException();
    }

    protected virtual EnemyMode ExecuteHoming(PlayerController player, int turnIndex, float turnTime)
    {
        if (player.Grounded)
        {
            targetCheckpoints.Clear();
            targetCheckpoints.Add(player.onTile);
        }
        SetContextFromDistanceMapAndPosition(player.enemyDistancesEight);

        SelectContextDirectionAndMove(turnTime);
        return EnemyMode.Homing;
    }

    protected GridPos SelectContextDirectionAndMove(float turnTime)
    {
        //Debug.Log(context.ToCSV());
        if (context.All(-1))
        {
            return pos;
        }
        bool[,] bestMoves = context.HasMinValue();

        //Debug.Log(bestMoves.Map(e => e ? 1 : 0).ToCSV());

        Coordinate[] valid = Convolution.ContextFilterToOffsets(bestMoves);

        GridPos offset = SelectMoveOffset(valid);
        GridPos next = pos + offset;
        Move(offset, turnTime);
        //Debug.Log(string.Format("From {0} with {1} to {2}", pos, offset, pos + offset));
        return next;
    }

    [SerializeField]
    protected int preferredWalkTargetDistance = 7;

    [SerializeField]
    protected int clearWalkTargetsAtLength = 5;


    protected virtual EnemyMode ExecuteWalking(PlayerController player, int turnIndex, float turnTime)
    {
        if (targetCheckpoints != null && activeTargetIndex < targetCheckpoints.Count && pos != targetCheckpoints[activeTargetIndex])
        {
            //TODO: Maybe more stuck testing like not having approached target for several rounds

            GridPos target = targetCheckpoints[activeTargetIndex];

            if (lvl.enemyConnectivityEight[pos.x, pos.y] != lvl.enemyConnectivityEight[target.x, target.y])
            {
                activeTargetIndex++;
            }

            if (targetDistanceMap == null)
            {
                SetTargetDistances(activeTargetIndex);
            }

        } else if (targetCheckpoints != null && activeTargetIndex < targetCheckpoints.Count - 1)
        {
            activeTargetIndex++;            
            SetTargetDistances(activeTargetIndex);
        } else
        {
            //New target
            Coordinate target = GetPossibleTargetsInMyRegion(preferredWalkTargetDistance, pos);
            if (activeTargetIndex >= clearWalkTargetsAtLength - 1)
            {
                targetCheckpoints.Clear();
            }
            targetCheckpoints.Add(pos);
            targetCheckpoints.Add(target);
            activeTargetIndex = targetCheckpoints.Count - 1;
            SetTargetDistances(activeTargetIndex);
        }

        SetContextFromDistanceMapAndPosition(targetDistanceMap);

        SelectContextDirectionAndMove(turnTime);
        return EnemyMode.Walking;
    }

    protected void SetContextFromDistanceMapAndPosition(int[,] distances)
    {
        context = distances.GetCenteredContext(3, pos);
        contextPosition = pos;

        //Set those context that are occupied by others as inaccessible
        //but keep value for self position
        int centerVal = context[1, 1];
        context = context.ZipTwo(
            board.GetOccupancyContext(pos, Occupancy.Enemy, Occupancy.Player),
            (a, b) => b == 1 ? -1 : a);
        context[1, 1] = centerVal;
    }

    protected virtual EnemyMode ExecuteStanding(PlayerController player, int turnIndex, float turnTime)
    {
        return EnemyMode.Standing;
    }

    protected virtual EnemyMode ExecuteHunt(PlayerController player, int turnIndex, float turnTime)
    {
        //Default implementation doesn't separate the two with regards to execution
        ExecuteHoming(player, turnIndex, turnTime);
        return EnemyMode.Hunting;
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

    protected virtual EnemyMode ExecutePatroling(PlayerController player, int turnIndex, float turnTime)
    {
        throw new System.NotImplementedException();
    }

    protected virtual GridPos SelectMoveOffset(Coordinate[] offsets)
    {
        if (offsets.Length == 0)
        {
            behaviour = EnemyMode.Standing;
            return new GridPos(0, 0);
        }
        //offsets[Random.Range(0, offsets.Length)];
        return offsets[Random.Range(0, offsets.Length)];
    }

    protected virtual EnemyMode ExecuteAttack1(PlayerController player, int turnIndex, float turnTime)
    {
       
        StartCoroutine(LookTowards((player.onTile - pos).NineDirection));

        if (anim)
        {
            StartCoroutine(DelayAttackTrigger(attackDelay));
        }

        return EnemyMode.Attack1;
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
            StartCoroutine(JumpToPos(maxTime, board.GetLocalPosition(pos), offset.NineDirection));
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
        PlayerRunData.stats.damageDealt += amount;
        Debug.Log("Hurt " + name);
    }

    public virtual void KilledEffect(int amount)
    {
        FloatTextManager.ShowText(transform, amount.ToString());
        attackedThisTurn = true;
        PlayerRunData.stats.damageDealt += amount;
        PlayerRunData.stats.enemiesDestroyed++;
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
    protected Vector3 jumpHeightAxis = Vector3.forward;

    Vector3 rotAxis = Vector3.forward;

    protected IEnumerator LookTowards(Direction dir)
    {
        Quaternion target = Quaternion.identity;
        Quaternion source = transform.localRotation;
        switch (dir)
        {
            case Direction.East:
                target = Quaternion.AngleAxis(270, rotAxis);
                break;
            case Direction.West:
                target = Quaternion.AngleAxis(90, rotAxis);
                break;
            case Direction.South:
                target = Quaternion.AngleAxis(0, rotAxis);
                break;
            case Direction.North:
                target = Quaternion.AngleAxis(180, rotAxis);
                break;
            case Direction.NorthEast:
                target = Quaternion.AngleAxis(225, rotAxis);
                break;
            case Direction.SouthEast:
                target = Quaternion.AngleAxis(315, rotAxis);
                break;
            case Direction.SouthWest:
                target = Quaternion.AngleAxis(45, rotAxis);
                break;
            case Direction.NorthWest:
                target = Quaternion.AngleAxis(135, rotAxis);
                break;
            default:
                target = source;
                break;
        }
        Vector3 euler = target.eulerAngles;
        target = Quaternion.Euler(0, 180, euler.z);
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


    protected IEnumerator JumpToPos(float maxTime, Vector3 targetPos, Direction lookDirection)
    {
        float startTime = Time.timeSinceLevelLoad;
        float duration = maxTime * jumpFractionDuration;
        float progress = 0;
        Vector3 startPos = board.GetLocalPosition(pos) + localPlacementOffset;

        yield return StartCoroutine(LookTowards(lookDirection));

        targetPos += localPlacementOffset;
        while (progress < 1)
        {
            progress = (Time.timeSinceLevelLoad - startTime) / duration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, planarCurve.Evaluate(progress)) + jumpHeightAxis * heightCurve.Evaluate(progress);
            yield return new WaitForSeconds(0.016f);
        }

        transform.localPosition = targetPos; 

    }

    protected virtual Coordinate GetPossibleTargetsInMyRegion(int preferredDistance, Coordinate pos)
    {
        int targetRegion = lvl.enemyConnectivityEight[pos.x, pos.y];
        int[,] distanceToMe = lvl.enemyConnectivityEight.HasValue(targetRegion).Distance(pos, Neighbourhood.Eight);
        preferredDistance = Mathf.Min(distanceToMe.Max(), preferredDistance);
        return distanceToMe.Where(e => e >= preferredDistance).ToArray().Shuffle().First();

    }

    protected virtual void SetTargetDistances(int targetIndex)
    {
        GridPos target = targetCheckpoints[targetIndex];
        int targetRegion = lvl.enemyConnectivityEight[target.x, target.y];
        targetDistanceMap = lvl.enemyConnectivityEight.HasValue(targetRegion).Distance(target, Neighbourhood.Eight);
        activeTargetIndex = targetIndex;
    }

    [SerializeField]
    Vector3 gizmoContextOffset = new Vector3(.3f, 2f, .3f);

    [SerializeField]
    Vector3 gizmoModeOffset = new Vector3(-.3f, 2f, -.3f);

    [SerializeField]
    Vector3 gizmosTargetOffset = new Vector3(0f, 2f, 0f);

    [SerializeField]
    float gizmosTargetSize = 0.2f;

    [SerializeField]
    Color gizmosTargetColor = Color.magenta;

    [SerializeField]
    Color gizmosEnemyToTargetColor = Color.cyan;

    [SerializeField]
    bool gizmosEntireDistanceMap = false;

    protected virtual void OnDrawGizmosSelected()
    {

        //BEHAVIOUR MOOD

        Gizmos.DrawIcon(
            board.transform.TransformPoint(board.GetLocalPosition(pos)) + gizmoModeOffset,
            "numberIcon_" + ((int) behaviour) + ".png", true);

        //CHECKPOINTS

        if (targetCheckpoints != null && targetCheckpoints.Count > 0)
        {
            Gizmos.color = gizmosTargetColor;
            Vector3 prev = board.transform.TransformPoint(board.GetLocalPosition(targetCheckpoints[0])) + gizmosTargetOffset;
            Vector3 cur;
            for (int i = 1, l = targetCheckpoints.Count; i < l; i++)
            {
                cur = board.transform.TransformPoint(board.GetLocalPosition(targetCheckpoints[i])) + gizmosTargetOffset;
                Gizmos.DrawLine(prev, cur);
                Gizmos.DrawCube(cur, Vector3.one * gizmosTargetSize);
                prev = cur;
            }

            if (activeTargetIndex >= 0 && activeTargetIndex < targetCheckpoints.Count)
            {
                prev = transform.position + gizmosTargetOffset;
                cur = board.transform.TransformPoint(board.GetLocalPosition(targetCheckpoints[activeTargetIndex])) + gizmosTargetOffset;
                Gizmos.color = gizmosEnemyToTargetColor;
                Gizmos.DrawLine(prev, cur);
            }
        }


        if (gizmosEntireDistanceMap)
        {

            if (targetDistanceMap == null)
            {
                return;
            }

            int w = targetDistanceMap.GetLength(0);
            int h = targetDistanceMap.GetLength(1);


            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {

                    Gizmos.DrawIcon(
                        board.transform.TransformPoint(board.GetLocalPosition(x, y)) + gizmoContextOffset,
                        "numberIcon_" + (targetDistanceMap[x, y] < 21 ? targetDistanceMap[x, y].ToString() : "plus") + ".png", true);
                }
            }

        }
        else {
            // CONTEXT

            if (context == null)
            {
                return;
            }

            int w = context.GetLength(0);
            int h = context.GetLength(1);
            int minX = contextPosition.x - (w - 1) / 2;
            int minY = contextPosition.y - (h - 1) / 2;
            int maxX = minX + w;
            int maxY = minY + h;

            for (int offX = minX, x = 0; offX < maxX; offX++, x++)
            {
                for (int offY = minY, y = 0; offY < maxY; offY++, y++)
                {

                    Gizmos.DrawIcon(
                        board.transform.TransformPoint(board.GetLocalPosition(offX, offY)) + gizmoContextOffset,
                        "numberIcon_" + (context[x, y] < 21 ? context[x, y].ToString() : "plus") + ".png", true);
                }
            }

        }
    }

}

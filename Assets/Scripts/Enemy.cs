using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyMode {Standing, Patroling, Hunting};


public class Enemy : MonoBehaviour {

    [SerializeField]
    Destructable bodyProperties;

    Level lvl;

    int playerLayer;

    EnemyMode behaviour = EnemyMode.Patroling;

    GridPos pos = new GridPos(-1, -1);
    BoardGrid board;

    public  void SetPosition(GridPos pos, BoardGrid board)
    {        
        this.pos = pos;
        this.board = board;
        board.Occupy(pos, Occupancy.Obstacle);
        transform.localPosition = board.GetLocalPosition(pos);     
    }

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");

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


    private void Lvl_OnTurnTick(PlayerController player)
    {
        if (behaviour == EnemyMode.Hunting)
        {
            
        } else if (behaviour == EnemyMode.Patroling)
        {
            int[,] context = board.GetOccupancyContext(pos, Occupancy.Free, Occupancy.BallPath);
            GridPos target = SelectMoveOffset(BoardGrid.ContextToOffsets(context));
            MoveTo(target);
        }
    }

    protected virtual GridPos SelectMoveOffset(List<GridPos> offsets)
    {
        return offsets[Random.Range(0, offsets.Count)];
    }

    protected virtual void MoveTo(GridPos target)
    {
        board.Free(pos, Occupancy.Obstacle);
        pos = target;
        board.Occupy(pos, Occupancy.Obstacle);
        transform.localPosition = board.GetLocalPosition(pos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            behaviour = EnemyMode.Hunting;
        }
    }
}

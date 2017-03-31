using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Arrays;

public delegate void TurnTick(PlayerController player, int turnIndex, float tickTime);
public delegate void NewLevel();

public class Level : MonoBehaviour {

    static Level _instance;

    public static Level instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Level>();
            }
            return _instance;
        }
    }

    public event TurnTick OnTurnTick;
    public event NewLevel OnNewLevel;

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField]
    BoardController boardController;

    [SerializeField]
    BallPath ballPath;

    [SerializeField]
    RoomMaker roomMaker;

    [SerializeField]
    BumperPlacer bumperPlacer;

    [SerializeField]
    PlayerController ball;

    [SerializeField]
    BoxPlacer boxPlacer;

    [SerializeField]
    EnemySpawner enemySpawner;

    [HideInInspector]
    public int playerConnectivityLabels;

    public int[,] playerConnectivity;

    [HideInInspector]
    public int enemyConnectivityLabels4;

    public int[,] enemyConnectivity4;

    [HideInInspector]
    public int enemyConnectivityLabels8;

    public int[,] enemyConnectivity8;

    bool previousLevel = false;

    [SerializeField, Range(0, 10)]
    float dropHeight = 0.5f;

    [SerializeField, Range(0, 2)]
    float turnTime;

    static bool makeTurns = false;

    public static bool LevelRunning
    {
        get
        {
            return makeTurns;
        }
    }

    [SerializeField, Range(0, 10)]
    float firstTickDelay = 2f;

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start () {
        StartCoroutine(TurnTicker());
        Generate();
        Implement();
	}

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    IEnumerator<WaitForSeconds> TurnTicker()
    {
        bool ticking = makeTurns;
        int turnIndex = 0;
        while (true)
        {
            if (makeTurns == false)
            {
                ticking = false;
                yield return new WaitForSeconds(Mathf.Min(0.1f, turnTime));
            } else if (ticking == false) {
                ticking = true;
                if (firstTickDelay > 0)
                {
                    yield return new WaitForSeconds(firstTickDelay);
                }
                turnIndex = 0;
            }
            else
            {
                if (OnTurnTick != null)
                {
                    OnTurnTick(ball, turnIndex, turnTime);
                }
                yield return new WaitForSeconds(turnTime);
                turnIndex++;
            }
        }
    }

    public void StopTheMotion() 
    {
        makeTurns = false;
        PlayerController.instance.Freeze();
    }

    public void StartTheMotion()
    {
        makeTurns = true;
        PlayerController.instance.Thaw();
    }

    [SerializeField, Range(1, 20)]
    int enterStoreEach = 7;

    public void MakeNextLevel()
    {
        StopTheMotion();

        if (PlayerRunData.stats.currentLevel % enterStoreEach == 0)
        {
            StoreSwapper.instance.EnterStore();
        }
        else {
            PlayerRunData.stats.NextLevel();
            Generate();
            StoreSwapper.instance.HideAllStores();
            Implement();
        }
    }

    public void Generate()
    {
        ballPath.GeneratePath(previousLevel);
        roomMaker.GenerateRooms();
        ballPath.GeneratePathHoles();
        boxPlacer.Generate();
        bumperPlacer.AllocateBumpPlacements();
        enemySpawner.AllocatePlacesAndDecideEnemies();

        bool[,] filter = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.WallBreakable, Occupancy.Hole);
        playerConnectivity = filter.Label(out playerConnectivityLabels);

        filter = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory, Occupancy.Hole, Occupancy.Obstacle);
        enemyConnectivity4 = filter.Label(out enemyConnectivityLabels4);

        filter = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory, Occupancy.Hole, Occupancy.Obstacle);
        enemyConnectivity8 = filter.Label(out enemyConnectivityLabels8, Neighbourhood.Eight);
        previousLevel = true;
    }

    public void Implement()
    {
        makeTurns = false;
        Splatterer.instance.CleanupSplatter();
        boardGrid.ConstructFloor();
        ballPath.ConstructPath();
        roomMaker.ConstructWalls();
        bumperPlacer.SpawnBumpers();
        boxPlacer.Place();
        enemySpawner.SpawnEnemies();
        DropBall();
        boardController.Balance();

        if (OnNewLevel != null)
        {
            OnNewLevel();
        }

        
        makeTurns = true;
    }

    public void DropBall()
    {
        ball.transform.position = ballPath.DropTarget + Vector3.up * dropHeight;
        ball.Inert();
        ball.EmoteStatus();
    }

#if UNITY_EDITOR

    [SerializeField]
    Vector3 gizmoOffset;

    public enum GizmoContent { Player, EnemyCross, EnemyEight};

    [SerializeField]
    GizmoContent gizmoContent;

    private void OnDrawGizmosSelected()
    {
        if (playerConnectivity == null)
        {
            return;
        }
        int[,] connectivity;
        switch (gizmoContent)
        {
            case GizmoContent.EnemyCross:
                connectivity = enemyConnectivity4;
                break;
            case GizmoContent.EnemyEight:
                connectivity = enemyConnectivity8;
                break;
            default:
                connectivity = playerConnectivity;
                break;
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

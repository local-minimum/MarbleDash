﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Arrays;
using LocalMinimum.Grid;
using System.Linq;

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

    static int _playerLevel;
    public static int playerLevel
    {
        get
        {
            return _playerLevel;
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
    public int enemyConnectivityLabelsCross;

    public int[,] enemyConnectivityCross;

    [HideInInspector]
    public int enemyConnectivityLabelsEight;

    public int[,] enemyConnectivityEight;

    public bool[,] enemyHolesConnectivity;

    public bool[,] nonBlocking;

    [HideInInspector]
    public Coordinate[] boardHoles;

    [HideInInspector]
    public int enemyHolesLabels;

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
        _playerLevel = LayerMask.NameToLayer("player");
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

    bool ticking;

    IEnumerator<WaitForSeconds> TurnTicker()
    {
        ticking = makeTurns;
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
        ticking = false;
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
        Debug.Log("Level: Generate path");
        ballPath.GeneratePath(previousLevel);
        Debug.Log("Level: Generate rooms");
        roomMaker.GenerateRooms();
        Debug.Log("Level: Generate holes");
        ballPath.GeneratePathHoles();
        Debug.Log("Level: Generate boxes");
        boxPlacer.Generate();
        Debug.Log("Level: Generate bumpers");
        bumperPlacer.AllocateBumpPlacements();

        Debug.Log("Level: Calculate connectivity");
        ReconstructConnectivities(ConnectivityTypes.All);

        //This is required to be last
        Debug.Log("Level: Generate path");
        enemySpawner.AllocatePlacesAndDecideEnemies();

        previousLevel = true;
    }

    public enum ConnectivityTypes { All, AboveGround, SolidGround};

    public void ReconstructConnectivities(ConnectivityTypes connectivityTypes)
    {

        if (connectivityTypes == ConnectivityTypes.All || connectivityTypes == ConnectivityTypes.AboveGround)
        {
            bool[,] filter = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.WallBreakable, Occupancy.Hole);
            playerConnectivity = filter.Label(out playerConnectivityLabels);

            filter = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory, Occupancy.Hole, Occupancy.Obstacle);
            enemyConnectivityCross = filter.Label(out enemyConnectivityLabelsCross);

            filter = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.WallBreakable, Occupancy.WallIllusory, Occupancy.Hole, Occupancy.Obstacle);
            enemyConnectivityEight = filter.Label(out enemyConnectivityLabelsEight, Neighbourhood.Eight);

            nonBlocking = boardGrid.GetFilterNotAny(Occupancy.Wall, Occupancy.Obstacle, Occupancy.WallBreakable);

        }

        if (connectivityTypes == ConnectivityTypes.All || connectivityTypes == ConnectivityTypes.SolidGround)
        {
            boardHoles = boardGrid.Find(Occupancy.Hole).Select(e => (Coordinate)e).ToArray();
            enemyHolesConnectivity = boardGrid.GetFilterNotAny(Occupancy.Hole);

        }
    }

    public void Implement()
    {
        makeTurns = false;

        Debug.Log("Level: Viscera cleanup");
        Splatterer.instance.CleanupSplatter();
        Debug.Log("Level: Construct floor");
        boardGrid.ConstructFloor();
        Debug.Log("Level: Construct path");
        ballPath.ConstructPath();
        Debug.Log("Level: Construct walls");
        roomMaker.ConstructWalls();
        Debug.Log("Level: Construct bumpers");
        bumperPlacer.SpawnBumpers();
        Debug.Log("Level: Construct boxes");
        boxPlacer.Place();
        Debug.Log("Level: Construct enemies");
        enemySpawner.SpawnEnemies();
        Debug.Log("Level: drop ball");
        DropBall();
        Debug.Log("Level: balance playing field");
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

    [SerializeField]
    Vector3 gizmoOffset;

    public enum GizmoContent { Player, EnemyCross, EnemyEight, Underground};

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
                connectivity = enemyConnectivityCross;
                break;
            case GizmoContent.EnemyEight:
                connectivity = enemyConnectivityEight;
                break;
            case GizmoContent.Underground:
                connectivity = enemyHolesConnectivity.Map(e => e ? 1 : 0);
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

}

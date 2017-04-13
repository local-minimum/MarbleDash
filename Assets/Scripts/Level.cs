using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Arrays;
using LocalMinimum.Grid;
using LocalMinimum.TurnBased;
using LocalMinimum;
using System.Linq;

public delegate void NewLevel();

public class Level : Singleton<Level> {

    static int _playerLevel;
    public static int playerLevel
    {
        get
        {
            return _playerLevel;
        }
    }

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

    public static bool LevelRunning
    {
        get
        {
            return TurnsManager.instance.MakingTurns;
        }
    }

    void Start () {
        _playerLevel = LayerMask.NameToLayer("player");
        Generate();
        Implement();
	}

    public void StopTheMotion() 
    {
        TurnsManager.instance.MakingTurns = false;
        PlayerController.instance.Freeze();
    }

    public void StartTheMotion()
    {
        TurnsManager.instance.MakingTurns = true;
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

        Debug.Log("Level: Fill 1 sized rooms with wall");
        //roomMaker.FillFreeSingleIslands();

        Debug.Log("Level: Generate holes");
        ballPath.GeneratePathHoles();
        Debug.Log("Level: Generate boxes");
        boxPlacer.Generate();
        Debug.Log("Level: Generate bumpers");
        bumperPlacer.AllocateBumpPlacements();

        Debug.Log("Level: Calculate connectivity");
        ReconstructConnectivities(ConnectivityTypes.All);

        try {
            //Connect rooms need happen here
            roomMaker.BridgeIsolatedRooms(playerConnectivity, playerConnectivityLabels);
        } catch (System.ArgumentException)
        {
            Debug.LogWarning("May have inaccessible room");
        }
        //This is required to be last
        Debug.Log("Level: Generate path");
        enemySpawner.AllocatePlacesAndDecideEnemies();

        previousLevel = true;
    }

    public enum ConnectivityTypes { All, AboveGround, SolidGround};

    bool requireRonstructAfterTick;

    public void EnqueueConnecitivityReconstruction()
    {
        requireRonstructAfterTick = true;
    }

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
        TurnsManager.instance.MakingTurns = false;

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

        
        TurnsManager.instance.MakingTurns = true;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TurnTick(PlayerController player);

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

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField]
    BallPath ballPath;

    [SerializeField]
    RoomMaker roomMaker;

    [SerializeField]
    PlayerController ball;

    [SerializeField]
    BoxPlacer boxPlacer;

    bool previousLevel = false;

    [SerializeField, Range(0, 3)]
    float dropHeight = 0.5f;

    [SerializeField, Range(0, 2)]
    float turnTime;

    bool makeTurns = false;

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

    IEnumerator<WaitForSeconds> TurnTicker()
    {
        bool ticking = makeTurns;
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
            }
            else
            {
                if (OnTurnTick != null)
                {
                    OnTurnTick(ball);
                }
                yield return new WaitForSeconds(turnTime);
            }
        }
    }

    public void Generate()
    {
        ballPath.GeneratePath(previousLevel);
        roomMaker.GenerateRooms();
        ballPath.GeneratePathHoles();
        boxPlacer.Generate();
        previousLevel = true;
    }

    public void Implement()
    {
        makeTurns = false;
        boardGrid.ConstructFloor();
        ballPath.ConstructPath();
        roomMaker.ConstructWalls();
        boxPlacer.Place();
        ball.transform.position = ballPath.DropTarget + Vector3.up * dropHeight;
        makeTurns = true;
    }
}

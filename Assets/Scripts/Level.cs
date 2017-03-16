﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TurnTick(PlayerController player, float tickTime);

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
    BoardController boardController;

    [SerializeField]
    BallPath ballPath;

    [SerializeField]
    RoomMaker roomMaker;

    [SerializeField]
    PlayerController ball;

    [SerializeField]
    BoxPlacer boxPlacer;

    [SerializeField]
    EnemySpawner enemySpawner;

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
                    OnTurnTick(ball, turnTime);
                }
                yield return new WaitForSeconds(turnTime);
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

    public void Generate()
    {
        ballPath.GeneratePath(previousLevel);
        roomMaker.GenerateRooms();
        ballPath.GeneratePathHoles();
        boxPlacer.Generate();
        enemySpawner.AllocatePlaces();
        previousLevel = true;
    }

    public void Implement()
    {
        makeTurns = false;
        boardGrid.ConstructFloor();
        ballPath.ConstructPath();
        roomMaker.ConstructWalls();
        boxPlacer.Place();
        enemySpawner.SpawnEnemies();
        ball.transform.position = ballPath.DropTarget + Vector3.up * dropHeight;
        ball.Inert();
        boardController.Balance();
        makeTurns = true;
    }
}
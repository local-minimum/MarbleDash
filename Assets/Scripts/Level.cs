using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    [SerializeField]
    BoardGrid boardGrid;

    [SerializeField]
    BallPath ballPath;

    [SerializeField]
    RoomMaker roomMaker;

    [SerializeField]
    PlayerController ball;

    bool previousLevel = false;

    [SerializeField, Range(0, 3)]
    float dropHeight = 0.5f;

	void Start () {
        Generate();
        Implement();
	}

    void Generate()
    {
        ballPath.GeneratePath(previousLevel);
        roomMaker.GenerateRooms();
        ballPath.GeneratePathHoles();
        previousLevel = true;
    }

    void Implement()
    {
        boardGrid.ConstructFloor();
        ballPath.ConstructPath();
        roomMaker.ConstructWalls();
        ball.transform.position = ballPath.DropTarget + Vector3.up * dropHeight;
    }
}

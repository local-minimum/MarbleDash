using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    GridPos pos;
    BoardGrid board;

    public void SetPosition(BoardGrid board, GridPos pos)
    {
        gameObject.SetActive(true);
        this.board = board;
        this.pos = pos;
        transform.localPosition = board.GetLocalPosition(pos);
    }
}

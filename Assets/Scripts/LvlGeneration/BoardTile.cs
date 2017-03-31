using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Grid;

public enum TileType { Solid, Hole };

public class BoardTile : MonoBehaviour {

    GridPos _pos;
    TileType _tileType;

    public TileType tileType
    {
        get
        {
            return _tileType;
        }
    }

    public void SetPosition(BoardGrid board, GridPos pos, float heightOffset, TileType tileType)
    {
        _tileType = tileType;
        _pos = pos;
        name = string.Format("{2} Tile ({0}, {1})", pos.x, pos.y, tileType);
        transform.localPosition = board.GetLocalPosition(pos) + Vector3.forward * heightOffset;
    }

    public GridPos pos
    {
        get
        {
            return _pos;
        }
    }

    int slimeCount;

    Color baseColor;
    Renderer rend;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        baseColor = rend.material.color;
    }

    void OnEnable()
    {
        rend.material.color = baseColor;
        slimeCount = 0;
    }

    public void Slime(Color slimeColor, float amount)
    {
        slimeCount++;
        rend.material.color = Color.Lerp(baseColor, slimeColor, amount);
    }

    public void RemoveSlime()
    {
        slimeCount--;
        if (slimeCount <= 0)
        {
            rend.material.color = baseColor;
        }
    }
}

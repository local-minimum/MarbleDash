using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.Grid
{

    public interface IGridder
    {
        GridPos Coordinate(Vector3 position, Space space);
        Vector3 Position(GridPos coordinate, Space space);
        Vector3 GetWorldPosition(Vector3 position);

        bool IsValidPosition(GridPos pos);

        int Width { get; }
        int Height { get; }

        Vector3 Normal { get; }

    }
}
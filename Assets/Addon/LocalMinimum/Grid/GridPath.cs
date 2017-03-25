using System.Collections.Generic;

namespace LocalMinimum.Grid
{
    public static class GridPath
    {

        public static List<GridPos> FindPath(GridPos source, GridPos targer, int[,] array, int filter)
        {
            List<GridPos> path = new List<GridPos>();
            Dictionary<GridPos, KeyValuePair<int, List<GridPos>>> paths = new Dictionary<GridPos, KeyValuePair<int, List<GridPos>>>();

            path.Add(source);


            return path;
        }

        public static bool IsInside(GridPos pos, int[,] array)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < array.GetLength(0) && pos.y < array.GetLength(1);
        }
        
        public static bool IsInRegion(GridPos pos, int[,] array, int filter)
        {
            return IsInside(pos, array) && array[pos.x, pos.y] == filter;
        }
    }
}

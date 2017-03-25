using System.Collections.Generic;
using System.Linq;

namespace LocalMinimum.Grid
{
    public static class GridPath
    {

        public static List<GridPos> FindPath(GridPos source, GridPos targert, int[,] array, int filter)
        {
            bool searching = source == targert;
            List<GridPos> path = new List<GridPos>();
            Dictionary<GridPos, KeyValuePair<int, List<GridPos>>> explored = new Dictionary<GridPos, KeyValuePair<int, List<GridPos>>>();
            Dictionary<GridPos, KeyValuePair<int, List<GridPos>>> seen = new Dictionary<GridPos, KeyValuePair<int, List<GridPos>>>();
            GridPos cur = source;

            while (searching) {
                List<GridPos> curPath = new List<GridPos>();
                curPath.Add(cur);
                explored.Add(source, new KeyValuePair<int, List<GridPos>>(0, curPath));

                //TODO: For each neighbour, if valid add
                
            }

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

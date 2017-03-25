using System.Collections.Generic;
using System.Linq;

namespace LocalMinimum.Grid
{
    public static class GridPath
    {

        public static List<GridPos> FindPath(GridPos source, GridPos target, int[,] array, int filter)
        {
            bool searching = source == target;
            Dictionary<GridPos, KeyValuePair<int, List<GridPos>>> explored = new Dictionary<GridPos, KeyValuePair<int, List<GridPos>>>();
            Dictionary<GridPos, KeyValuePair<int, List<GridPos>>> seen = new Dictionary<GridPos, KeyValuePair<int, List<GridPos>>>();
            GridPos cur = source;
            List<GridPos> curPath = new List<GridPos>();
            curPath.Add(cur);
            int curLength = 1;

            while (searching) {

                explored.Add(cur, new KeyValuePair<int, List<GridPos>>(curLength, curPath));

                int nextPathLength = explored[cur].Key + 1;
                foreach(GridPos neigh in cur.GetNeighbours(Neighbourhood.Eight))
                {
                    if (!IsInRegion(neigh, array, filter) || explored.ContainsKey(neigh))
                    {
                        continue;
                    }
                    
                    if (neigh == target)
                    {
                        List<GridPos> neighPath = new List<GridPos>();
                        neighPath.AddRange(curPath);
                        neighPath.Add(neigh);
                        return neighPath;
                    }

                    if (!seen.ContainsKey(neigh) || seen[neigh].Key > nextPathLength)
                    {
                        List<GridPos> neighPath = new List<GridPos>();
                        neighPath.AddRange(curPath);
                        neighPath.Add(neigh);
                        seen[neigh] = new KeyValuePair<int, List<GridPos>>(nextPathLength, neighPath);
                    }
                    
                }
                if (seen.Count == 0)
                {
                    break;
                }
                var next = seen.OrderBy(e => e.Value.Key).First();
                cur = next.Key;
                curPath.Clear();
                curPath.AddRange(next.Value.Value);
                curLength = next.Value.Key;
            }

            return new List<GridPos>();
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

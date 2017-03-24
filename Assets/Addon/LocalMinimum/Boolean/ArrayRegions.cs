using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.Boolean
{
    public static class ArrayRegions
    {
        public static int[,] Label(this bool[,] input)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            int[,] labels = new int[w, h];
            int labelCount = 0;
            for (int x=0; x<w; x++)
            {
                for (int y=0; y<h; y++)
                {
                    if (!input[x, y] || labels[x, y] > 0)
                    {
                        continue;
                    }

                    labelCount++;

                    List<KeyValuePair<int, int>> queue = new List<KeyValuePair<int, int>>();

                    labels[x, y] = labelCount;
                    queue.Add(new KeyValuePair<int, int>(x, y));

                    while (queue.Count > 0)
                    {

                        KeyValuePair<int, int> cur = queue[0];
                        queue.RemoveAt(0);

                    }
                }
            }

            return labels;
        }
    }
}
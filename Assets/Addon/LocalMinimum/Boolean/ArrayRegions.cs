using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.Boolean
{
    public static class ArrayRegions
    {
        public static int[,] Label(this bool[,] input, out int labelCount)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            int[,] labels = new int[w, h];
            labelCount = 0;
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

                    queue.Add(new KeyValuePair<int, int>(x, y));

                    while (queue.Count > 0)
                    {

                        KeyValuePair<int, int> cur = queue[0];
                        queue.RemoveAt(0);
                        labels[cur.Key, cur.Value] = labelCount;

                        if (cur.Key > 0 && labels[cur.Key - 1, cur.Value] == 0)
                        {
                            queue.Add(new KeyValuePair<int, int>(cur.Key - 1, cur.Value));
                        }
                        if (cur.Value > 0 && labels[cur.Key, cur.Value - 1] == 0)
                        {
                            queue.Add(new KeyValuePair<int, int>(cur.Key, cur.Value - 1));
                        }
                        if (cur.Key < w - 1 && labels[cur.Key + 1, cur.Value] == 0)
                        {
                            queue.Add(new KeyValuePair<int, int>(cur.Key + 1, cur.Value));
                        }
                        if (cur.Value < h - 1 && labels[cur.Key, cur.Value + 1] == 0)
                        {
                            queue.Add(new KeyValuePair<int, int>(cur.Value, cur.Key + 1));
                        }

                    }
                }
            }

            return labels;
        }


    }
}
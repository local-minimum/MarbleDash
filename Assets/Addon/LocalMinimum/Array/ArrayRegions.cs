using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.Boolean
{
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static class ArrayRegions
    {
        public static int[,] Label(this bool[,] input, out int labelCount)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int lastX = w - 1;
            int lastY = h - 1;

            int[,] labels = new int[w, h];
            labelCount = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!input[x, y] || labels[x, y] > 0)
                    {
                        continue;
                    }

                    labelCount++;

                    List<Coordinate> queue = new List<Coordinate>();
                    queue.Clear();
                    queue.Add(new Coordinate(x, y));
                    input[x, y] = false;
                    int index = 0;
                    int length = 1;
                    while (length > index)
                    {

                        Coordinate cur = queue[index];
                        int curX = cur.x;
                        int curY = cur.y;

                        labels[curX, curY] = labelCount;

                        if (curX > 0 && input[curX - 1, curY])
                        {
                            queue.Add(new Coordinate(curX - 1, curY));
                            input[curX - 1, curY] = false;
                            length++;
                        }
                        if (curY > 0 && input[curX, curY - 1])
                        {
                            queue.Add(new Coordinate(curX, curY - 1));
                            input[curX, curY - 1] = false;
                            length++;
                        }
                        if (curX != lastX && input[curX + 1, curY])
                        {
                            queue.Add(new Coordinate(curX + 1, curY));
                            input[curX + 1, curY] = false;
                            length++;
                        }
                        if (curY != lastY && input[curX, curY + 1])
                        {
                            queue.Add(new Coordinate(curX, curY + 1));
                            input[curX, curY + 1] = false;
                            length++;
                        }

                        index++;
                    }
                }
            }

            return labels;
        }

        public static int[,] DistanceToEgde(this bool[,] filter, bool treatBorderAsEdge = false)
        {
            return filter.Distance(filter.Edge(treatBorderAsEdge));
        }

        public static int[,] Distance(this bool[,] filter, bool[,] seed)
        {
            List<Coordinate> sources = seed.ToCoordinates();
            int w = filter.GetLength(0);
            int h = filter.GetLength(1);
            int lastX = w - 1;
            int lastY = h - 1;

            int[,] distances = Fill(w, h, -1);

            int index = 0;
            int length = sources.Count;
            //Debug.Log(length);
            int initialLength = length;

            while (index < length)
            {
                Coordinate coord = sources[index];
                int x = coord.x;
                int y = coord.y;
                int lowest = -1;

                if (x != 0 && filter[x - 1, y])
                {
                    if (distances[x - 1, y] < 0)
                    {
                        if (!seed[x - 1, y])
                        {
                            sources.Add(new Coordinate(x - 1, y));
                            seed[x - 1, y] = true;
                            length++;
                        }
                    }
                    else if (index >= initialLength)
                    {
                        if (lowest < 0)
                        {
                            lowest = distances[x - 1, y];
                        }
                        else
                        {
                            lowest = Mathf.Min(lowest, distances[x - 1, y]);
                        }
                    }
                }

                if (x != lastX && filter[x + 1, y])
                {
                    if (distances[x + 1, y] < 0)
                    {
                        if (!seed[x + 1, y])
                        {
                            sources.Add(new Coordinate(x + 1, y));
                            length++;
                            seed[x + 1, y] = true;
                        }
                    }
                    else if (index >= initialLength)
                    {
                        if (lowest < 0)
                        {
                            lowest = distances[x + 1, y];
                        }
                        else
                        {
                            lowest = Mathf.Min(lowest, distances[x + 1, y]);
                        }
                    }
                }

                if (y != 0 && filter[x, y - 1])
                {
                    if (distances[x, y - 1] < 0)
                    {
                        if (!seed[x, y - 1])
                        {
                            sources.Add(new Coordinate(x, y - 1));
                            length++;
                            seed[x, y - 1] = true;
                        }
                    }
                    else if (index >= initialLength)
                    {
                        if (lowest < 0)
                        {
                            lowest = distances[x, y - 1];
                        }
                        else
                        {
                            lowest = Mathf.Min(lowest, distances[x, y - 1]);
                        }
                    }
                    
                }

                if (y != lastY && filter[x, y + 1]) {
                    if (distances[x, y + 1] < 0)
                    {
                        if (!seed[x, y + 1])
                        {
                            sources.Add(new Coordinate(x, y + 1));
                            length++;
                            seed[x, y + 1] = true;
                        }
                    }
                    else if (index >= initialLength)
                    {
                        if (lowest < 0)
                        {
                            lowest = distances[x, y + 1];
                        }
                        else
                        {
                            lowest = Mathf.Min(lowest, distances[x, y + 1]);
                        }
                    }
                }

                //Debug.Log(string.Format("{0}, {1} ({2}) {3} {4}", x, y, index, index >= initialLength, lowest));
                distances[x, y] = lowest + 1;
                index++;
            }

            return distances;
        }

        public static bool[,] Edge(this bool[,] input, bool treatBorderAsEdge = false)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int lastX = w - 1;
            int lastY = h - 1;
            bool[,] output = new bool[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!input[x, y])
                    {
                        continue;
                    }

                    if (x == 0 || y == 0 || x == lastX || y == lastY)
                    {
                        if (treatBorderAsEdge)
                        {
                            output[x, y] = true;
                        }
                        else
                        {
                            bool center = input[x, y];
                            if (x > 0 && input[x - 1, y] != center ||
                                x  < lastX && input[x + 1, y] != center ||
                                y > 0 && input[x, y - 1] != center ||
                                y < lastY && input[x, y + 1] != center)
                            {
                                output[x, y] = true;
                            }
                        }
                        
                        continue;
                    } else
                    {
                        bool center = input[x, y];
                        if (input[x - 1, y] != center || 
                            input[x + 1, y] != center || 
                            input[x, y - 1] != center || 
                            input[x, y + 1] != center)
                        {
                            output[x, y] = true;
                        }

                    }
                }
            }

            return output;
        }

        public static bool[,] Equals(this int[,] input, int value)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            bool[,] output = new bool[w, h];

            for (int x=0; x<w; x++)
            {
                for (int y=0; y<h; y++)
                {
                    if (input[x, y] == value)
                    {
                        output[x, y] = true;
                    }
                }
            }
            return output;
        }

        public static List<Coordinate> ToCoordinates(this bool[,] input)
        {
            List<Coordinate> coordinates = new List<Coordinate>();
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y])
                    {
                        coordinates.Add(new Coordinate(x, y));
                    }
                }
            }

            return coordinates;
        }

        public static int[,] Fill(int w, int h, int value)
        {
            return new int[w, h].Fill(value);
        }

        public static int[,] Fill(this int[,] input, int value)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x=0; x<w; x++)
            {
                for (int y=0; y<h; y++)
                {
                    input[x, y] = value;
                }
            }
            return input;
        }

        public static int[,] Fill(this int[,] input, bool[,] filter, int value)
        {
            int w = filter.GetLength(0);
            int h = filter.GetLength(1);
            
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (filter[x, y])
                    {
                        input[x, y] = value;
                    }
                }
            }
            return input;
        }

        public static bool[,] Invert(this bool[,] input)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            bool[,] inverted = new bool[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {

                    if (!input[x, y])
                    {
                        inverted[x, y] = true;
                    }
                }

            }

            return inverted;
        }

        public static int Max(this int[,] input)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int val = input[0, 0];

            for (int x =0; x<w; x++)
            {
                for (int y=0; y<h; y++)
                {
                    if (input[x, y] > val)
                    {
                        val = input[x, y];
                    }
                }
            }
            return val;
        }

        public static int Min(this int[,] input)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int val = input[0, 0];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y] < val)
                    {
                        val = input[x, y];
                    }
                }
            }
            return val;
        }
    }
}
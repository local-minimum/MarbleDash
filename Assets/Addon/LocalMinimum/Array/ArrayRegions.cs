﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.Arrays
{

    public enum Neighbourhood { Cross, Eight };

    /// <summary>
    /// Minimal x, y coordinate struct with no functionality.
    /// 
    /// For more features, GridPos has equal structure and implicit casts exists.
    /// </summary>
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Grid.GridPos(Coordinate coord)
        {
            return new Grid.GridPos(coord.x, coord.y);
        }
    }

    public static class ArrayRegions
    {
        /// <summary>
        /// Enumerate pixel region in the boolean filter.
        /// 
        /// First identified region starts at 1.
        /// 
        /// It doesn't consider diagonal as evidence for connected region.
        /// </summary>
        /// <param name="input">The bolean filter for all regions</param>
        /// <param name="labelCount">The number of regions identified</param>
        /// <returns>Int array with positive values as region identifiers</returns>
        public static int[,] Label(this bool[,] input, out int labelCount, Neighbourhood neighbourhood = Neighbourhood.Cross)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int lastX = w - 1;
            int lastY = h - 1;
            bool doEight = neighbourhood == Neighbourhood.Eight;
            int[,] labels = new int[w, h];
            bool[,] used = new bool[w, h];

            labelCount = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (used[x, y] || !input[x, y] || labels[x, y] > 0)
                    {
                        continue;
                    }

                    labelCount++;

                    List<Coordinate> queue = new List<Coordinate>();
                    queue.Clear();
                    queue.Add(new Coordinate(x, y));
                    used[x, y] = true;
                    int index = 0;
                    int length = 1;
                    while (length > index)
                    {

                        Coordinate cur = queue[index];
                        int curX = cur.x;
                        int curY = cur.y;

                        labels[curX, curY] = labelCount;

                        if (curX > 0 && input[curX - 1, curY] && !used[curX - 1, curY])
                        {
                            queue.Add(new Coordinate(curX - 1, curY));
                            used[curX - 1, curY] = true;
                            length++;
                        }

                        if (curY > 0 && input[curX, curY - 1] && !used[curX, curY - 1])
                        {
                            queue.Add(new Coordinate(curX, curY - 1));
                            used[curX, curY - 1] = true;
                            length++;
                        }

                        if (curX != lastX && input[curX + 1, curY] && !used[curX + 1, curY])
                        {
                            queue.Add(new Coordinate(curX + 1, curY));
                            used[curX + 1, curY] = true;
                            length++;
                        }

                        if (curY != lastY && input[curX, curY + 1] && !used[curX, curY + 1])
                        {
                            queue.Add(new Coordinate(curX, curY + 1));
                            used[curX, curY + 1] = true;
                            length++;
                        }

                        if (doEight)
                        {

                            if (curX > 0 && curY > 0 && input[curX - 1, curY - 1] && !used[curX - 1, curY - 1])
                            {
                                queue.Add(new Coordinate(curX - 1, curY - 1));
                                used[curX - 1, curY - 1] = true;
                                length++;
                            }

                            if (curX > 0 && curY != lastY && input[curX - 1, curY + 1] && !used[curX - 1, curY + 1])
                            {
                                queue.Add(new Coordinate(curX - 1, curY + 1));
                                used[curX - 1, curY + 1] = true;
                                length++;
                            }

                            if (curX != lastX && curY != lastY && input[curX + 1, curY + 1] && !used[curX + 1, curY + 1])
                            {
                                queue.Add(new Coordinate(curX + 1, curY + 1));
                                used[curX + 1, curY + 1] = true;
                                length++;
                            }

                            if (curX != lastX && curY > 0 && input[curX + 1, curY - 1] && !used[curX + 1, curY - 1])
                            {
                                queue.Add(new Coordinate(curX + 1, curY - 1));
                                used[curX + 1, curY - 1] = true;
                                length++;
                            }

                        }

                        index++;
                    }
                }
            }

            return labels;
        }

        /// <summary>
        /// Gives each pixel the taxicab distance to nearest edge of region.
        /// </summary>
        /// <param name="filter">Regions</param>
        /// <param name="treatBorderAsEdge">If border or array should be treated as an edge</param>
        /// <returns>Int array with -1 meaning outside region and zero and positive the distance to edge</returns>
        public static int[,] DistanceToEgde(this bool[,] filter, bool treatBorderAsEdge = false)
        {
            return filter.Distance(filter.Edge(treatBorderAsEdge));
        }

        /// <summary>
        /// Gives the distance to the seed position(s) for all positions within the same
        /// region as a seed.
        /// </summary>
        /// <param name="filter">The regions</param>
        /// <param name="seed">The seeding positions marked as true</param>
        /// <returns>Int array with -1 meaning outside region and zero being seed and positive distance to seed</returns>
        public static int[,] Distance(this bool[,] filter, Coordinate seed, Neighbourhood neighbourhood = Neighbourhood.Cross)
        {
            bool[,] arrSeed = new bool[filter.GetLength(0), filter.GetLength(1)];
            arrSeed[seed.x, seed.y] = true;
            return filter.Distance(arrSeed, neighbourhood);
        }

        /// <summary>
        /// Maps distance to seeds in a specified region
        /// </summary>
        /// <param name="filter">Region specification</param>
        /// <param name="seeds">Coordinate for seeding distance measures</param>
        /// <param name="neighbourhood">Neighbourhood type</param>
        /// <returns>2D int array of distance to seed</returns>
        public static int[,] Distance(this bool[,] filter, Coordinate[] seeds, Neighbourhood neighbourhood = Neighbourhood.Cross)
        {
            bool[,] seed = new bool[filter.GetLength(0), filter.GetLength(1)];
            for (int i=0, l=seeds.Length; i< l; i++)
            {
                seed[seeds[i].x, seeds[i].y] = true;
            }
            return filter.Distance(seed, neighbourhood);
        }

        /// <summary>
        /// Maps distance to seed coordinates in a specified array size
        /// </summary>
        /// <param name="width">Length of outer dimension</param>
        /// <param name="height">Length of inner dimension</param>
        /// <param name="seeds">Coordinate for distance seeds</param>
        /// <param name="neighbourhood">Neighbourhood type</param>
        /// <returns>2D int array of distance to seeds</returns>
        public static int[,] Distance(int width, int height, Coordinate[] seeds, Neighbourhood neighbourhood = Neighbourhood.Cross)
        {
            bool[,] filter = Fill(width, height, true);
            return filter.Distance(seeds, neighbourhood);
        }

        /// <summary>
        /// Gives the distance to the seed position(s) for all positions within the same
        /// region as a seed.
        /// </summary>
        /// <param name="filter">The regions</param>
        /// <param name="seed">The seeding positions marked as true</param>
        /// <returns>Int array with -1 meaning outside region and zero being seed and positive distance to seed</returns>
        public static int[,] Distance(this bool[,] filter, bool[,] seed, Neighbourhood neighbourhood = Neighbourhood.Cross)
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
            int seedsLength = length;

            bool testDiagonals = neighbourhood == Neighbourhood.Eight;

            while (index < length)
            {
                Coordinate coord = sources[index];
                int x = coord.x;
                int y = coord.y;
                int lowest = -1;

                if (y != 0 && (filter[x, y - 1] || seed[x, y - 1]))
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
                    else if (index >= seedsLength)
                    {
                        int cur = distances[x, y - 1];                        
                        if (lowest < 0)
                        {
                            lowest = cur;
                        }
                        else if (cur >= 0)
                        {
                            lowest = Mathf.Min(lowest, cur);
                        }
                    }

                }

                if (y != lastY && (filter[x, y + 1] || seed[x, y + 1]))
                {
                    if (distances[x, y + 1] < 0)
                    {
                        if (!seed[x, y + 1])
                        {
                            sources.Add(new Coordinate(x, y + 1));
                            length++;
                            seed[x, y + 1] = true;
                        }
                    }
                    else if (index >= seedsLength)
                    {
                        int cur = distances[x, y + 1];
                        if (lowest < 0)
                        {
                            lowest = cur;
                        }
                        else if (cur >= 0)
                        {
                            lowest = Mathf.Min(lowest, cur);
                        }
                    }
                }


                if (x != 0 && (filter[x - 1, y] || seed[x - 1, y]))
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
                    else if (index >= seedsLength)
                    {
                        int cur = distances[x - 1, y];
                        if (lowest < 0)
                        {
                            lowest = cur;
                        }
                        else if (cur >= 0)
                        {
                            lowest = Mathf.Min(lowest, cur);
                        }
                    }
                }

                if (x != lastX && (filter[x + 1, y] || seed[x + 1, y]))
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
                    else if (index >= seedsLength)
                    {
                        int cur = distances[x + 1, y];
                        if (lowest < 0)
                        {
                            lowest = cur;
                        }
                        else
                        {
                            lowest = Mathf.Min(lowest, cur);
                        }
                    }
                }

                if (testDiagonals)
                {

                    if (x != 0 && y != lastY && (filter[x - 1, y + 1] || seed[x - 1, y + 1]))
                    {
                        if (distances[x - 1, y + 1] < 0)
                        {
                            if (!seed[x - 1, y + 1])
                            {
                                sources.Add(new Coordinate(x - 1, y + 1));
                                length++;
                                seed[x - 1, y + 1] = true;
                            }
                        }
                        else if (index >= seedsLength)
                        {
                            int cur = distances[x - 1, y + 1];
                            if (lowest < 0)
                            {
                                lowest = cur;
                            }
                            else if (cur >= 0)
                            {
                                lowest = Mathf.Min(lowest, cur);
                            }
                        }
                    }


                    if (x != lastX && y != lastY && (filter[x + 1, y + 1] || seed[x + 1, y + 1]))
                    {
                        if (distances[x + 1, y + 1] < 0)
                        {
                            if (!seed[x + 1, y + 1])
                            {
                                sources.Add(new Coordinate(x + 1, y + 1));
                                length++;
                                seed[x + 1, y + 1] = true;
                            }
                        }
                        else if (index >= seedsLength)
                        {
                            int cur = distances[x + 1, y + 1];
                            if (lowest < 0)
                            {
                                lowest = cur;
                            }
                            else if (cur >= 0)
                            {
                                lowest = Mathf.Min(lowest, cur);
                            }
                        }
                    }

                    if (x != lastX && y != 0 && (filter[x + 1, y - 1] || seed[x + 1, y - 1]))
                    {
                        if (distances[x + 1, y - 1] < 0)
                        {
                            if (!seed[x + 1, y - 1])
                            {
                                sources.Add(new Coordinate(x + 1, y - 1));
                                length++;
                                seed[x + 1, y - 1] = true;
                            }
                        }
                        else if (index >= seedsLength)
                        {
                            int cur = distances[x + 1, y - 1];
                            if (lowest < 0)
                            {
                                lowest = cur;
                            }
                            else if (cur >= 0)
                            {
                                lowest = Mathf.Min(lowest, cur);
                            }
                        }
                    }

                    if (x != 0 && y != 0 && (filter[x - 1, y - 1] || seed[x - 1, y - 1]))
                    {
                        if (distances[x - 1, y - 1] < 0)
                        {
                            if (!seed[x - 1, y - 1])
                            {
                                sources.Add(new Coordinate(x - 1, y - 1));
                                length++;
                                seed[x - 1, y - 1] = true;
                            }
                        }
                        else if (index >= seedsLength)
                        {
                            int cur = distances[x - 1, y - 1];
                            if (lowest < 0)
                            {
                                lowest = cur;
                            }
                            else if (cur >= 0)
                            {
                                lowest = Mathf.Min(lowest, cur);
                            }
                        }
                    }

                }
                //Debug.Log(string.Format("{0}, {1} ({2}) {3} {4}", x, y, index, index >= initialLength, lowest));
                distances[x, y] = lowest + 1;
                index++;
            }

            return distances;
        }

        /// <summary>
        /// Get the edge of regions
        /// </summary>
        /// <param name="input">The regions</param>
        /// <param name="treatBorderAsEdge">If border or array should be treated as an edge</param>
        /// <returns>An edge filter</returns>
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

        /// <summary>
        /// Produces a boolean filter for where int array equals a value.
        /// 
        /// Useful in combination with Label
        /// </summary>
        /// <param name="input">the input data</param>
        /// <param name="value">the sought value</param>
        /// <returns>boolean filter array</returns>
        public static bool[,] HasValue(this int[,] input, int value)
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

        public static bool[,] HasAnyValue(this int[,] input, params int[] values)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int l = values.Length;
            bool[,] output = new bool[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int i = 0; i < l; i++)
                    {
                        if (values[i] == input[x, y])
                        {
                            output[x, y] = true;
                            break;
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Converts a bolean filter array to list of coordinates of true values.
        /// </summary>
        /// <param name="input">Filter array</param>
        /// <returns>Coordinate list</returns>
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

        /// <summary>
        /// Creates a filled array with certain value.
        /// </summary>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="value">Fill value</param>
        /// <returns>int array with only fill value</returns>
        public static T[,] Fill<T>(int w, int h, T value)
        {
            return new T[w, h].FillInPlace(value);
        }

        /// <summary>
        /// Creates a filled array with certain value.
        /// </summary>
        /// <param name="input">the template array</param>
        /// <param name="value">Fill value</param>
        /// <returns>int array with only fill value</returns>
        public static T[,] FillInPlace<T>(this T[,] input, T value)
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

        /// <summary>
        /// Fills a region of input with a value
        /// </summary>
        /// <param name="input">The original data</param>
        /// <param name="filter">Where to fill</param>
        /// <param name="value">fill value</param>
        /// <returns>updated data</returns>
        public static T[,] FillInPlace<T>(this T[,] input, bool[,] filter, T value)
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

        /// <summary>
        /// Inverts true and false states of array
        /// </summary>
        /// <param name="input">the data</param>
        /// <returns>the inverted data</returns>
        public static bool[,] Inverted(this bool[,] input)
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



        /// <summary>
        /// Maximum value in array
        /// </summary>
        /// <param name="input">2d int array</param>
        /// <returns>maximum value</returns>
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

        /// <summary>
        /// Minumum value in array
        /// </summary>
        /// <param name="input">2d int array</param>
        /// <returns>minimum value</returns>
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

        /// <summary>
        /// Minumum value in array
        /// </summary>
        /// <param name="input">2d int array</param>
        /// <param name="filter">filter</param>
        /// <returns>minimum value</returns>
        public static int Min(this int[,] input, bool[,] filter)
        {
            List<Coordinate> positions = filter.ToCoordinates();
            int l = positions.Count;
            if (l > 0)
            {
                Coordinate cur = positions[0];
                int val = input[cur.x, cur.y];
                for (int i = 1; i < l; i++)
                {
                    cur = positions[i];
                    int curVal = input[cur.x, cur.y];
                    if (val > curVal)
                    {
                        val = curVal;
                    }
                }
                return val;
            } else
            {
                throw new System.ArgumentException("Filter had no true value");
            }
            
        }

        public static bool[,] HasMinValue(this int[,] input, int omitValue = -1)
        {
            return input.HasValue(input.Min(input.Map(e => e != omitValue)));
        }

        /// <summary>
        /// Number of true values in array
        /// </summary>
        /// <param name="input">Bolean 2d array</param>
        /// <returns>true value count</returns>
        public static int Count(this bool[,] input)
        {
            int count = 0;
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y])
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static int CountValue(this int[,] input, int value)
        {
            int count = 0;
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y] == value)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static bool Locate(this int[,] input, int value, int nth, ref Coordinate c)
        {
            int count = 0;
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y] == value)
                    {
                        count++;
                        if (count == nth)
                        {
                            c.x = x;
                            c.y = y;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool Locate(this bool[,] input, int nth, ref Coordinate c)
        {
            int count = 0;
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y])
                    {
                        count++;
                        if (count == nth)
                        {
                            c.x = x;
                            c.y = y;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Elementwise application of a function
        /// </summary>
        /// <param name="input">Data</param>
        /// <param name="func">The function</param>
        /// <returns>Result</returns>
        public static T[,] MapInPlace<T>(this T[,] input, System.Func<T, T> func)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    input[x, y] = func(input[x, y]);
                    
                }
            }
            return input;

        }

        /// <summary>
        /// Elementwise application of a function
        /// </summary>
        /// <param name="input">Data</param>
        /// <param name="func">The function</param>
        /// <returns>Result</returns>
        public static TTarget[,] Map<TSource, TTarget>(this TSource[,] input, System.Func<TSource, TTarget> func)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            TTarget[,] output = new TTarget[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    output[x, y] = func(input[x, y]);

                }
            }
            return output;

        }

        /// <summary>
        /// Apply function to corresponding elements and get result
        /// </summary>
        /// <param name="input">First array</param>
        /// <param name="other">Second array</param>
        /// <param name="func">function that takes two elements, one from each</param>
        /// <returns>Outpu of function</returns>
        public static TTarget[,] ZipTwo<TFirst, TSecond, TTarget>(this TFirst[,] input, TSecond[,] other, System.Func<TFirst, TSecond, TTarget> func)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            TTarget[,] output = new TTarget[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    output[x, y] = func(input[x, y], other[x, y]);

                }
            }
            return output;
        }


        public static TTarget[,] ZipThree<TFirst, TSecond, TThird, TTarget>(this TFirst[,] input, TSecond[,] other, TThird[,] secondOther, System.Func<TFirst, TSecond, TThird, TTarget> func)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            TTarget[,] output = new TTarget[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    output[x, y] = func(input[x, y], other[x, y], secondOther[x, y]);

                }
            }
            return output;
        }

        public static List<Coordinate> Where<T>(this T[,] input, System.Func<T, bool> func)
        {
            List<Coordinate> coords = new List<Coordinate>();

            int w = input.GetLength(0);
            int h = input.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (func(input[x, y]))
                    {
                        coords.Add(new Coordinate(x, y));
                    }
                    
                }
            }

            return coords;
        }

        public static bool All(this int[,] input, int value)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y] != value)
                    {
                        return false;
                    } 
                }
            }

            return true;
        }

        public static bool All(this bool[,] input)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!input[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;

        }

        public static bool Any(this int[,] input, int value)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y] == value)
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        public static bool Any(this bool[,] input)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (input[x, y])
                    {
                        return true;
                    }
                }
            }

            return false;

        }
    }
}
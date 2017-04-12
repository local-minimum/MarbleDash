using System.Collections.Generic;
using System;

namespace LocalMinimum.Arrays
{
    public enum EdgeCondition {Constant, Valid};

    public static class Convolution
    {

        public static int[,] GetCenteredContext(this int[,] input, int size, Coordinate coordinate, EdgeCondition edgeCondition=EdgeCondition.Constant, int fillValue=-1)
        {
            if (size % 2 == 0)
            {
                throw new System.ArgumentException("Size must be odd");
            }

            int offset = (size - 1) / 2;

            int[,] context = new int[size, size];

            int xMin = coordinate.x - offset;
            int xMax = xMin + size;
            int yMin = coordinate.y - offset;
            int yMax = yMin + size;

            int width = input.GetLength(0);
            int height = input.GetLength(1);

            for (int x=xMin, x2=0; x<xMax; x++, x2++)
            {
                for (int y = yMin, y2 = 0; y < yMax; y++, y2++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height)
                    {
                        switch (edgeCondition)
                        {
                            case EdgeCondition.Constant:
                                context[x2, y2] = fillValue;
                                break;
                            default:
                                throw new System.NotImplementedException("Condition " + edgeCondition + " not supported yet");
                        }
                    }
                    else
                    {
                        context[x2, y2] = input[x, y];
                    }
                }
            }
            return context;
        }

        public static T[,] GetCenteredContext<T>(this T[,] input, int size, Coordinate coordinate, EdgeCondition edgeCondition, T fillValue)
        {
            if (size % 2 == 0)
            {
                throw new System.ArgumentException("Size must be odd");
            }

            int offset = (size - 1) / 2;

            T[,] context = new T[size, size];

            int xMin = coordinate.x - offset;
            int xMax = xMin + size;
            int yMin = coordinate.y - offset;
            int yMax = yMin + size;

            int width = input.GetLength(0);
            int height = input.GetLength(1);

            for (int x = xMin, x2 = 0; x < xMax; x++, x2++)
            {
                for (int y = yMin, y2 = 0; y < yMax; y++, y2++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height)
                    {
                        switch (edgeCondition)
                        {
                            case EdgeCondition.Constant:
                                context[x2, y2] = fillValue;
                                break;
                            default:
                                throw new System.NotImplementedException("Condition " + edgeCondition + " not supported yet");
                        }
                    }
                    else
                    {
                        context[x2, y2] = input[x, y];
                    }
                }
            }
            return context;
        }

        public static T[,] GetCenteredContext<T>(this T[,] input, int size, int sourceX, int sourceY, EdgeCondition edgeCondition, T fillValue)
        {
            if (size % 2 == 0)
            {
                throw new System.ArgumentException("Size must be odd");
            }

            int offset = (size - 1) / 2;

            T[,] context = new T[size, size];

            int xMin = sourceX - offset;
            int xMax = xMin + size;
            int yMin = sourceY - offset;
            int yMax = yMin + size;

            int width = input.GetLength(0);
            int height = input.GetLength(1);

            for (int x = xMin, x2 = 0; x < xMax; x++, x2++)
            {
                for (int y = yMin, y2 = 0; y < yMax; y++, y2++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height)
                    {
                        switch (edgeCondition)
                        {
                            case EdgeCondition.Constant:
                                context[x2, y2] = fillValue;
                                break;
                            default:
                                throw new NotImplementedException("Condition " + edgeCondition + " not supported yet");
                        }
                    }
                    else
                    {
                        context[x2, y2] = input[x, y];
                    }
                }
            }
            return context;
        }

        public static T[,] GetNonCenteredContext<T>(this T[,] input, int size, int sourceX, int sourceY, EdgeCondition edgeCondition = EdgeCondition.Valid)
        {

            T[,] context = new T[size, size];

            int xMax = sourceX + size;
            int yMax = sourceY + size;

            for (int x = sourceX, x2 = 0; x < xMax; x++, x2++)
            {
                for (int y = sourceY, y2 = 0; y < yMax; y++, y2++)
                {
                    context[x2, y2] = input[x, y];                    
                }
            }
            return context;
        }

        public static bool[,] Dilate(this bool[,] input, Neighbourhood neighbourhood, EdgeCondition edgeCondition)
        {
            switch (neighbourhood) {
                case Neighbourhood.Cross:
                    return input.GenericFilter(3, CrossDilate, edgeCondition, false);
                case Neighbourhood.Eight:
                    return input.GenericFilter(3, EightDilate, edgeCondition, false);
                default:
                    throw new NotImplementedException("Neighbourhood " + neighbourhood + " not implemented as dilation");
            }
        }

        static bool CrossDilate(bool[,] data)
        {
            return data[1, 1] || data[0, 1] || data[1, 0] || data[2, 0] || data[0, 2];
        }

        static bool EightDilate(bool[,] data)
        {
            return data.Any();
        }

        public static T[,] GenericFilter<T>(this bool[,] input, int size, Func<bool[,], T> function, EdgeCondition edgeCondition = EdgeCondition.Valid, bool fillValue = false)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            T[,] result;
            bool centeredContext = true;

            switch (edgeCondition)
            {
                case EdgeCondition.Constant:
                    result = new T[w, h];
                    break;
                case EdgeCondition.Valid:
                    w -= (size - 1);
                    h -= (size - 1);
                    result = new T[w, h];
                    centeredContext = false;
                    break;
                default:
                    throw new NotImplementedException("Condition " + edgeCondition + " not supported yet");

            }
            for (int x=0; x< w; x++)
            {
                for (int y=0; y< h; y++)
                {
                    result[x, y] = function(
                        centeredContext ? 
                            input.GetCenteredContext(size, x, y, edgeCondition, fillValue) : 
                            input.GetNonCenteredContext(size, x, y, edgeCondition));
                }
            }

            return result;
        }


        public static T[,] GenericFilter<T>(this bool[,] input, int size, Func<int, int, bool[,], T> function, EdgeCondition edgeCondition, bool fillValue)
        {
            int w = input.GetLength(0);
            int h = input.GetLength(1);

            T[,] result;
            bool centeredContext = true;

            switch (edgeCondition)
            {
                case EdgeCondition.Constant:
                    result = new T[w, h];
                    break;
                case EdgeCondition.Valid:
                    w -= (size - 1);
                    h -= (size - 1);
                    result = new T[w, h];
                    centeredContext = false;
                    break;
                default:
                    throw new NotImplementedException("Condition " + edgeCondition + " not supported yet");

            }
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    result[x, y] = function(x, y,
                        centeredContext ?
                            input.GetCenteredContext(size, x, y, edgeCondition, fillValue) :
                            input.GetNonCenteredContext(size, x, y, edgeCondition));
                }
            }

            return result;
        }

        public static Coordinate[] ContextFilterToOffsets(bool[,] context)
        {
            int count = context.Count();
            Coordinate[] offsets = new Coordinate[count];
            int w = context.GetLength(0);
            int h = context.GetLength(1);
            
            if (w % 2 == 0 || h % 2 == 0)
            {
                throw new System.ArgumentException("Array dimensions must be odd");
            }

            int offX = (w - 1) / 2;
            int offY = (h - 1) / 2;
            int index = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (context[x, y])
                    {
                        offsets[index] = new Coordinate(x - offX, y - offY);
                        index++;
                    }
                }
            }

            return offsets;
        }

        public static string ToCSV(this int[,] input, string delim = ",", string newRow = "\n")
        {
            string output = "";
            int w = input.GetLength(0);
            int h = input.GetLength(1);
            int lastY = h - 1;
            int lastX = w - 1;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    output += input[x, y];

                    if (y != lastY)
                    {
                        output += delim;
                    }
                }

                if (x != lastX)
                {
                    output += newRow;
                }
            }
            return output;

        }
    }
}
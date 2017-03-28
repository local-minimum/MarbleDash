using System.Collections.Generic;

namespace LocalMinimum.Arrays
{
    public enum EdgeCondition {Constant};

    public static class Convolution
    {

        public static int[,] GetContext(this int[,] input, int size, Coordinate coordinate, EdgeCondition edgeCondition=EdgeCondition.Constant, int fillValue=-1)
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

    }
}
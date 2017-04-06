using System.Collections;
using System.Collections.Generic;
using System;

namespace LocalMinimum.Arrays
{

    public class BitMaskArray<T>
    {
        int[,] data;
        int width;
        int height;

        Array values;

        #region Constructors

        public BitMaskArray(int width, int height)
        {

            values = Enum.GetValues(typeof(T));
            if (values.Length > 31)
            {
                throw new ArgumentException("The enum type may only contain 31 values");
            }
            this.width = width;
            this.height = height;
            data = new int[width, height];

        }

        public BitMaskArray(int size)
        {
            values = Enum.GetValues(typeof(T));
            if (values.Length > 31)
            {
                throw new ArgumentException("The enum type may only contain 31 values");
            }
            width = size;
            height = size;
            data = new int[size, size];

        }

        #endregion

        public static int IndexToMask(int index)
        {
            return 1 << index;
        }


        public void ClearAllFlags()
        {
            int val = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    data[x, y] = val;
                }
            }
        }

        int EnumToInt(object flag)
        {
            return (int)flag;
        }

        public void Occupy(int x, int y, Enum flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            data[x, y] |= mask;
        }

        public void Occupy(Coordinate coord, Enum flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            data[coord.x, coord.y] |= mask;
        }

        public void DeOccupy(Coordinate coord, Enum flag)
        {
            int mask = ~IndexToMask(EnumToInt(flag));
            data[coord.x, coord.y] &= mask;
        }

        public void DeOccupy(int x, int y, Enum flag)
        {
            int mask = ~IndexToMask(EnumToInt(flag));
            data[x, y] &= mask;
        }

        public void Set(int x, int y, Enum flag)
        {
            data[x, y] = IndexToMask(EnumToInt(flag));
        }

        public void Set(Coordinate coord, Enum flag)
        {
            data[coord.x, coord.y] = IndexToMask(EnumToInt(flag));
        }

        public void SetAll(Enum flag)
        {
            int val = IndexToMask(EnumToInt(flag));
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    data[x, y] = val;
                }
            }
        }

        public bool Has(Coordinate coord, Enum flag)
        {
            return (data[coord.x, coord.y] & IndexToMask(EnumToInt(flag))) != 0;
        }

        public bool Has(int x, int y, Enum flag)
        {
            return (data[x, y] & IndexToMask(EnumToInt(flag))) != 0;
        }

        public bool HasOnly(Coordinate coord, Enum flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            return (data[coord.x, coord.y] & mask) == mask;
        }

        public bool HasOnly(int x, int y, Enum flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            return (data[x, y] & mask) == mask;
        }

        public IEnumerable<Coordinate> Find(Enum flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) != 0)
                    {
                        yield return new Coordinate(x, y);
                    }
                }
            }
        }

        public IEnumerable<Coordinate> FindOnly(Enum flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) == mask)
                    {
                        yield return new Coordinate(x, y);
                    }
                }
            }
        }

        public IEnumerable<Coordinate> FindNot(T flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) == 0)
                    {
                        yield return new Coordinate(x, y);
                    }
                }
            }
        }

        public IEnumerable<Coordinate> FindAny(params T[] flags)
        {
            int mask = GetOptionalMask(flags);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) != 0)
                    {
                        yield return new Coordinate(x, y);
                    }
                }
            }
        }

        public IEnumerable<Coordinate> FindOnlyAny(params T[] flags)
        {
            int antiMask = ~GetOptionalMask(flags);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & antiMask) == 0 && data[x, y] != 0)
                    {
                        yield return new Coordinate(x, y);
                    }
                }
            }
        }

        public int Count(T flag)
        {
            int mask = IndexToMask(EnumToInt(flag));
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) != 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public bool[,] GetFilterNotAny(params T[] occupancy)
        {
            bool[,] filter = new bool[width, height];
            int mask = GetOptionalMask(occupancy);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) == 0)
                    {
                        filter[x, y] = true;
                    }
                }
            }

            return filter;
        }

        public bool[,] GetFilterAny(params T[] occupancy)
        {
            bool[,] filter = new bool[width, height];
            int mask = GetOptionalMask(occupancy);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) != 0)
                    {
                        filter[x, y] = true;
                    }
                }
            }

            return filter;
        }

        public bool[,] GetFilter(T occupancy)
        {
            bool[,] filter = new bool[width, height];
            int mask = IndexToMask(EnumToInt(occupancy));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((data[x, y] & mask) != 0)
                    {
                        filter[x, y] = true;
                    }
                }
            }

            return filter;
        }

        public IEnumerable<T> Flags(Coordinate pos)
        {
            int posVal = data[pos.x, pos.y];
            foreach (var val in values)
            {
                if ((posVal & IndexToMask((int)val)) != 0)
                {
                    yield return (T)val;
                }
            }

        }

        public IEnumerable<T> Flags(int x, int y)
        {
            int posVal = data[x, y];
            foreach (var val in values)
            {
                if ((posVal & IndexToMask((int)val)) != 0)
                {
                    yield return (T)val;
                }
            }

        }

        int GetOptionalMask(params T[] filter)
        {
            if (filter.Length == 0)
            {
                throw new ArgumentException("At least one filter required");
            }

            int mask = IndexToMask(EnumToInt(filter[0]));
            for (int i = 1; i < filter.Length; i++)
            {
                mask |= IndexToMask(EnumToInt(filter[1]));
            }

            return mask;
        }

        public bool IsValidCoordinate(Coordinate pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
        }

        public bool IsValidX(int x)
        {
            return x >= 0 && x < width;
        }


        public bool IsValidY(int y)
        {
            return y >= 0 && y < width;
        }

        public bool IsSize(int size)
        {
            return size == width && size == height;
        }

        public int GetInt(int x, int y)
        {
            return data[x, y];
        }

        public int GetInt(Coordinate coord)
        {
            return data[coord.x, coord.y];
        }

        public int[,] GetContextHas(Coordinate coord, int size, T flag, int fillValue=-1)
        {
            return GetContextHas(coord, size, size, flag, fillValue);
        }

        public int[,] GetContextHas(Coordinate coord, int width, int height, T flag, int fillValue=-1)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                throw new ArgumentException("Dimensions must be odd");
            }

            int mask = IndexToMask(EnumToInt(flag));
            int[,] ret = new int[width, height];

            int xFrom = coord.x - (width - 1) / 2;
            int yFrom = coord.y - (height - 1) / 2;

            
            for (int ySource=yFrom, yTarget=0; yTarget < height; ySource++, yTarget++)
            {
                bool validY = IsValidY(ySource);
                for (int xSource = xFrom, xTarget = 0; xTarget < width; xSource++, xTarget++)
                {
                    if (validY && IsValidX(xSource))
                    {
                        ret[xTarget, yTarget] = (data[xSource, ySource] & mask) != 0 ? 1 : 0;
                    }
                    else
                    {
                        ret[xTarget, yTarget] = fillValue;
                    }
                }
            }
            return ret;

        }

        public int[,] GetContextHasNot(Coordinate coord, int size, T flag, int fillValue = -1)
        {
            return GetContextHasNot(coord, size, size, flag, fillValue);
        }

        public int[,] GetContextHasNot(Coordinate coord, int width, int height, T flag, int fillValue = -1)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                throw new ArgumentException("Dimensions must be odd");
            }

            int mask = IndexToMask(EnumToInt(flag));
            int[,] ret = new int[width, height];

            int xFrom = coord.x - (width - 1) / 2;
            int yFrom = coord.y - (height - 1) / 2;


            for (int ySource = yFrom, yTarget = 0; yTarget < height; ySource++, yTarget++)
            {
                bool validY = IsValidY(ySource);
                for (int xSource = xFrom, xTarget = 0; xTarget < width; xSource++, xTarget++)
                {
                    if (validY && IsValidX(xSource))
                    {
                        ret[xTarget, yTarget] = (data[xSource, ySource] & mask) == 0 ? 1 : 0;
                    }
                    else
                    {
                        ret[xTarget, yTarget] = fillValue;
                    }
                }
            }
            return ret;
        }

        public int[,] GetContextHasAny(Coordinate coord, int size, int fillValue = -1, params T[] flags)
        {
            return GetContextHasAny(coord, size, size, fillValue, flags);
        }

        public int[,] GetContextHasAny(Coordinate coord, int width, int height, int fillValue = -1, params T[] flags)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                throw new ArgumentException("Dimensions must be odd");
            }

            int mask = GetOptionalMask(flags);
            int[,] ret = new int[width, height];

            int xFrom = coord.x - (width - 1) / 2;
            int yFrom = coord.y - (height - 1) / 2;


            for (int ySource = yFrom, yTarget = 0; yTarget < height; ySource++, yTarget++)
            {
                bool validY = IsValidY(ySource);
                for (int xSource = xFrom, xTarget = 0; xTarget < width; xSource++, xTarget++)
                {
                    if (validY && IsValidX(xSource))
                    {
                        ret[xTarget, yTarget] = (data[xSource, ySource] & mask) != 0 ? 1 : 0;
                    }
                    else
                    {
                        ret[xTarget, yTarget] = fillValue;
                    }
                }
            }
            return ret;
        }

        public int[,] GetContextHasNotAny(Coordinate coord, int size, int fillValue = -1, params T[] flags)
        {
            return GetContextHasNotAny(coord, size, size, fillValue, flags);
        }

        public int[,] GetContextHasNotAny(Coordinate coord, int width, int height, int fillValue = -1, params T[] flags)
        {
            if (width % 2 == 0 || height % 2 == 0)
            {
                throw new ArgumentException("Dimensions must be odd");
            }

            int mask = GetOptionalMask(flags);
            int[,] ret = new int[width, height];

            int xFrom = coord.x - (width - 1) / 2;
            int yFrom = coord.y - (height - 1) / 2;


            for (int ySource = yFrom, yTarget = 0; yTarget < height; ySource++, yTarget++)
            {
                bool validY = IsValidY(ySource);
                for (int xSource = xFrom, xTarget = 0; xTarget < width; xSource++, xTarget++)
                {
                    if (validY && IsValidX(xSource))
                    {
                        ret[xTarget, yTarget] = (data[xSource, ySource] & mask) == 0 ? 1 : 0;
                    }
                    else
                    {
                        ret[xTarget, yTarget] = fillValue;
                    }
                }
            }
            return ret;
        }
    }
}

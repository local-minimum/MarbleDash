﻿using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Arrays;

namespace LocalMinimum.Grid
{

    public enum Direction { None, North, South, West, East, NorthWest, SouthWest, SouthEast, NorthEast };

    [System.Serializable]
    public struct GridPos
    {
        public int x;
        public int y;

        public GridPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        static public implicit operator Arrays.Coordinate(GridPos pos)
        {
            return new Arrays.Coordinate(pos.x, pos.y);
        }

        static public implicit operator int[](GridPos pos)
        {
            return new int[] { pos.x, pos.y };
        }

        public void RotateCW()
        {
            int tmp = x;
            x = y;
            y = -tmp;
        }

        public void RotateCCW()
        {
            int tmp = x;
            x = -y;
            y = tmp;
        }

        public void Rotate180()
        {
            x = -x;
            y = -y;
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public static GridPos operator +(GridPos a, GridPos b)
        {
            return new GridPos(a.x + b.x, a.y + b.y);
        }

        public static GridPos operator -(GridPos a, GridPos b)
        {
            return new GridPos(a.x - b.x, a.y - b.y);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Direction AsMajorDirection()
        {
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                return x > 0 ? Direction.East : Direction.West;
            }
            else if (y != 0)
            {
                return y > 0 ? Direction.North : Direction.South;
            }
            else
            {
                return Direction.None;
            }
        }

        public GridPos West
        {
            get
            {
                return new GridPos(x - 1, y);
            }
        }

        public GridPos East
        {
            get
            {
                return new GridPos(x + 1, y);
            }
        }

        public GridPos North
        {
            get
            {
                return new GridPos(x, y - 1);
            }
        }

        public GridPos South
        {
            get
            {
                return new GridPos(x, y + 1);
            }
        }

        public GridPos NorthWest
        {
            get
            {
                return new GridPos(x - 1, y - 1);
            }
        }

        public GridPos SouthWest
        {
            get
            {
                return new GridPos(x - 1, y + 1);
            }
        }

        public GridPos SouthEast {
            get
            {
                return new GridPos(x + 1, y + 1);
            }
        }

        public GridPos NorthEast
        {
            get
            {
                return new GridPos(x + 1, y - 1);
            }
        }

        public static int TaxiCabDistance(GridPos a, GridPos b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public static int ChessBoardDistance(GridPos a, GridPos b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public int ChessBoardMagnitude
        {
            get
            {
                return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
            }
        }

        public int TaxicabMagnitude
        {
            get
            {
                return Mathf.Abs(x) + Mathf.Abs(y);
            }
        }

        public static int ShortestDimensionDistance(GridPos a, GridPos b)
        {
            return Mathf.Min(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public GridPos NineNormalized
        {
            get
            {
                return new GridPos(x > 0 ? 1 : (x < 0 ? -1 : 0), y > 0 ? 1 : (y < 0 ? -1 : 0));
            }
        }

        public Direction NineDirection
        {
            get
            {
                GridPos normed = this.NineNormalized;
                if (normed.x < 0)
                {
                    if (normed.y < 0)
                    {
                        return Direction.NorthWest;
                    } else if (normed.y > 0)
                    {
                        return Direction.SouthWest;
                    } else
                    {
                        return Direction.West;
                    }
                } else if (normed.x > 0)
                {
                    if (normed.y < 0)
                    {
                        return Direction.NorthEast;
                    } else if (normed.y < 0)
                    {
                        return Direction.SouthEast;
                    } else
                    {
                        return Direction.East;
                    }
                } else if (normed.y > 0)
                {
                    return Direction.North;
                } else if (normed.y < 0)
                {
                    return Direction.South;
                }


                return Direction.None;
            }
        }

        public bool IsZero()
        {
            return x == 0 && y == 0;
        }

        public IEnumerable<GridPos> GetNeighbours(Neighbourhood neighbourhood)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (neighbourhood == Neighbourhood.Cross && Mathf.Abs(x) == Mathf.Abs(y))
                    {
                        continue;
                    }
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    yield return new GridPos(this.x + x, this.y + y);
                }
            }
        }
    }

}
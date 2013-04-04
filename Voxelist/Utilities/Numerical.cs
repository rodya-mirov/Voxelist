using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Voxelist.Utilities
{
    public static class Numerical
    {
        public static void Swap<E>(ref E a, ref E b)
        {
            E temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// Finds the "sign" of x; that is, 0 if x is 0,
        /// -1 if x is negative, or 1 if x is positive.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int sign(float x)
        {
            if (x == 0)
                return 0;
            else if (x < 0)
                return -1;
            else
                return 1;
        }

        /// <summary>
        /// Returns the "correct" x%y (that is, the
        /// positive remainder of x/y).  Assumes
        /// y>0, but is made to deal with x<=0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int IntMod(int x, int y)
        {
            int r = x % y;

            return (r < 0) ? r + y : r;
        }

        /// <summary>
        /// Returns the "correct" x%y (that is, the
        /// positive remainder of x/y).  Assumes
        /// y>0, but is made to deal with x<=0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float FloatMod(float x, float y)
        {
            float r = x % y;

            return (r < 0) ? r + y : r;
        }

        /// <summary>
        /// Returns the "correct" rounding DOWN
        /// of x/y, assuming y>0.  That is, the largest
        /// integer k such that k*y <= x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int IntDivide(int x, int y)
        {
            int r = (int)(x / y);

            return (r * y > x) ? r - 1 : r;
        }

        /// <summary>
        /// Returns the "correct" rounding DOWN
        /// of x/y, assuming y>0.  That is, the largest
        /// integer k such that k*y <= x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int FloatDivide(float x, float y)
        {
            int r = (int)(x / y);

            return (r * y > x) ? r - 1 : r;
        }

        /// <summary>
        /// Makes a minimal bounding box which contains both the given box and its
        /// translate by the given vector.
        /// </summary>
        /// <param name="BoundingBox"></param>
        /// <param name="intendedChange"></param>
        /// <returns></returns>
        public static BoundingBox StretchBox(BoundingBox BoundingBox, Vector3 intendedChange)
        {
            Vector3 min = BoundingBox.Min;
            Vector3 max = BoundingBox.Max;

            if (intendedChange.X < 0)
                min.X += intendedChange.X;
            else
                max.X += intendedChange.X;

            if (intendedChange.Y < 0)
                min.Y += intendedChange.Y;
            else
                max.Y += intendedChange.Y;

            if (intendedChange.Z < 0)
                min.Z += intendedChange.Z;
            else
                max.Z += intendedChange.Z;

            return new BoundingBox(min, max);
        }

        public static void FindChunkBounds(BoundingBox box, ChunkCoordinate startingCoordinates, out ChunkCoordinate min, out ChunkCoordinate max)
        {
            min = startingCoordinates;
            max = startingCoordinates;

            Vector3 boxmin = box.Min;
            Vector3 boxmax = box.Max;

            while (boxmin.X <= 0)
            {
                min.X--;
                boxmin.X += GameConstants.CHUNK_X_WIDTH;
            }

            while (boxmin.Z <= 0)
            {
                min.Z--;
                boxmin.Z += GameConstants.CHUNK_Z_LENGTH;
            }

            while (boxmax.X >= GameConstants.CHUNK_X_WIDTH)
            {
                max.X++;
                boxmax.X -= GameConstants.CHUNK_X_WIDTH;
            }

            while (boxmax.Z >= GameConstants.CHUNK_Z_LENGTH)
            {
                max.Z++;
                boxmax.Z -= GameConstants.CHUNK_Z_LENGTH;
            }
        }
    }
}

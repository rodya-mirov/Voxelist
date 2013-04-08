using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Voxelist.Utilities
{
    public static class Numerical
    {
        /// <summary>
        /// Finds the "sign" of x; that is, 0 if x is 0,
        /// -1 if x is negative, or 1 if x is positive.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int sign(float x)
        {
            /*
            if (x == 0)
                return 0;
            else if (x < 0)
                return -1;
            else
                return 1;
             */

            return (x == 0 ? 0 : (x > 0 ? 1 : -1));
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


        public static void RepairBlockCoordinates(ref int chunkX, ref int chunkZ, ref int cubeX, ref int cubeZ)
        {
            while (cubeX < 0)
            {
                chunkX--;
                cubeX += GameConstants.CHUNK_X_WIDTH;
            }

            while (cubeX >= GameConstants.CHUNK_X_WIDTH)
            {
                chunkX++;
                cubeX -= GameConstants.CHUNK_X_WIDTH;
            }

            while (cubeZ < 0)
            {
                chunkZ--;
                cubeZ += GameConstants.CHUNK_Z_LENGTH;
            }

            while (cubeZ >= GameConstants.CHUNK_Z_LENGTH)
            {
                chunkZ++;
                cubeZ -= GameConstants.CHUNK_Z_LENGTH;
            }
        }
    }
}

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
        public static int intDivide(int x, int y)
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
        public static int intDivide(float x, float y)
        {
            int r = (int)(x / y);

            return (r * y > x) ? r - 1 : r;
        }
    }
}

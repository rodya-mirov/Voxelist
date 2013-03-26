using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxelist.Utilities
{
    public static class RandomHelper
    {
        public static int WORLD_SEED = 11609681;

        public static int combineToSingleSeed(int x, int y, bool useWorldSeed)
        {
            int output = ((x & 65535) + (y << 16));
            if (useWorldSeed)
                return output ^ WORLD_SEED;
            else
                return output;
        }

        public static int randomInt(int seed, int minValue, int maxValue)
        {
            Random ran = new Random(seed);
            return ran.Next(minValue, maxValue);
        }

        public static double randomDouble(int seed)
        {
            Random ran = new Random(seed);
            return ran.NextDouble();
        }
    }
}

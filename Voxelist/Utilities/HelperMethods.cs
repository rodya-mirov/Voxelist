using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Voxelist.Utilities
{
    public static class HelperMethods
    {
        /// <summary>
        /// Just swaps the two values.  Used primarily to visually clean up code.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Swap<E>(ref E a, ref E b)
        {
            E temp = a;
            a = b;
            b = temp;
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

        /// <summary>
        /// Given a "chunk bounding box" (that is, a boundingbox with chunk coordinates
        /// indicating where the floats are taken from), finds Chunk Coordinate bounds
        /// on the box which are appropriately sharp.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="startingCoordinates"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void FindChunkBounds(BoundingBox box, ChunkCoordinate startingCoordinates,
            out ChunkCoordinate min, out ChunkCoordinate max)
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

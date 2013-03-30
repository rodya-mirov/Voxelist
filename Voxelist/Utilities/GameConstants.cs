using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Voxelist.Utilities
{
    public static class GameConstants
    {
        #region Chunk size constants
        public static int CHUNK_X_WIDTH = 16;
        public static int CHUNK_Z_LENGTH = 16;
        public static int CHUNK_Y_HEIGHT = 64;

        public static Vector3 CHUNK_SIZE { get { return new Vector3(CHUNK_X_WIDTH, CHUNK_Y_HEIGHT, CHUNK_Z_LENGTH); } }
        #endregion

        public static float PHYSICS_COLLISION_EPSILON = 0.00001f; //on a collision, move back this amount to avoid staying *slightly* inside something
    }
}

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
        public const int CHUNK_X_WIDTH = 16;
        public const int CHUNK_Z_LENGTH = 16;
        public const int CHUNK_Y_HEIGHT = 64;

        public const float CHUNK_RADIUS = 32.0f; //a slight overestimate to the full radius of a chunk

        public const int CHUNK_X_LOG = 4; //the amount of bits to shift to mult/div by the chunk width
        public const int CHUNK_Z_LOG = 4; //the amount of bits to shift to mult/div by the chunk length
        public const int CHUNK_Y_LOG = 6; //the amount of bits to shift to mult/div by the chunk depth

        private static Vector3 chunkSize = new Vector3(CHUNK_X_WIDTH, CHUNK_Y_HEIGHT, CHUNK_Z_LENGTH);
        public static Vector3 CHUNK_SIZE { get { return chunkSize; } }
        #endregion

        public const float PHYSICS_COLLISION_EPSILON = 0.00001f; //on a collision, move back this amount to avoid staying *slightly* inside something
    }
}

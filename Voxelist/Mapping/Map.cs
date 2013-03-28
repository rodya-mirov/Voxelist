using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.BlockHandling;
using Voxelist.Utilities;
using Voxelist.Rendering;
using Voxelist.Entities;

namespace Voxelist.Mapping
{
    public abstract class Map
    {
        public BlockHandler BlockHandler { get; protected set; }

        public Map(BlockHandler handler)
        {
            this.BlockHandler = handler;

            chunkCache = new ChunkCache(this, CacheRadius);
        }

        public void LoadContent(Game game)
        {
            chunkCache.StartCaching(0, 0, StartingChunkRadiusToLoad);
        }

        public void Dispose()
        {
            chunkCache.Dispose();
        }

        /// <summary>
        /// This is where you get to do your procedural generation!
        /// By whatever means you deem appropriate, construct a chunk
        /// of cubes at the designated "chunk coordinates;" if you
        /// prefer "cube" coordinates, multiply by Chunk.CHUNK_WIDTH_CUBES
        /// and Chunk.CHUNK_LENGTH_CUBES for the corner.
        /// 
        /// NOTE: Coordinates must be in the form (x,y,z), where x is
        /// the "side-side" dimension, z is the "forward-backward" dimension,
        /// and y is the "up-down" dimension.  This coordinate system
        /// doesn't always feel natural!
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public abstract Block[,,] MakeChunkBlocks(int chunkX, int chunkZ);

        #region Physics
        /// <summary>
        /// This enumerates all boundingboxes of blocks on the map which actually do intersect
        /// the given boundingbox.  Only returns boxes of blocks which are not passable of course.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <param name="box"></param>
        /// <returns>A tuple; the relevant boundingbox of the block, as well as the friction coefficient of the box.</returns>
        public IEnumerable<Collider> IntersectingBlocks(int chunkX, int chunkZ, BoundingBox box)
        {
            Point3 cubeMin = Point3.RoundDown(box.Min);
            Point3 cubeMax = Point3.RoundUp(box.Max);

            //adjust the y bounds, accounting for "out of the world" issues
            if (cubeMin.Y < 0)
                cubeMin.Y = 0;
            if (cubeMax.Y >= GameConstants.CHUNK_Y_HEIGHT)
                cubeMax.Y = GameConstants.CHUNK_Y_HEIGHT - 1;

            //now hunt through all possible collisions
            Vector3 translation = new Vector3(0, 0, 0);
            for (int x = cubeMin.X; x <= cubeMax.X; x++)
            {
                translation.X = x;
                for (int y = cubeMin.Y; y <= cubeMax.Y; y++)
                {
                    translation.Y = y;
                    for (int z = cubeMin.Z; z <= cubeMax.Z; z++)
                    {
                        translation.Z = z;

                        Block block = GetHighPriorityBlock(chunkX, chunkZ, x, y, z);
                        if (BlockHandler.IsPassable(block))
                            continue;

                        yield return new Collider(
                            block,
                            translation,
                            BlockHandler);
                    }
                }
            }
        }
        #endregion

        #region Caching
        private ChunkCache chunkCache;

        protected virtual int StartingChunkRadiusToLoad { get { return 1; } }

        /// <summary>
        /// Make sure this is higher than the ViewDistance!
        /// </summary>
        protected abstract int CacheRadius { get; }

        /// <summary>
        /// Gets the Chunk located at the specified CHUNK coordinates.
        /// This generates new chunks as rarely as possible.  The urgent
        /// flag indicates whether or not "no chunk" is an acceptable answer
        /// (urgent means you really really need the chunk, otherwise
        /// return null as convenient).
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkY"></param>
        /// <returns></returns>
        public Chunk GetChunk(int chunkX, int chunkZ, bool urgent)
        {
            if (chunkCache.IsReady(chunkX, chunkZ))
                return chunkCache.GetChunk(chunkX, chunkZ);
            else if (urgent)
            {
                chunkCache.AddChunk(chunkX, chunkZ);
                return chunkCache.GetChunk(chunkX, chunkZ);
            }
            else
                return null;
        }

        /// <summary>
        /// Gets the block at a specified coordinate.  This is a "high priority" get, which
        /// means if the chunk isn't in cache, it will construct it immediately anyway.  This
        /// usually destroys performance, but what can be done?
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <param name="cubeX"></param>
        /// <param name="cubeY"></param>
        /// <param name="cubeZ"></param>
        /// <returns></returns>
        protected Block GetHighPriorityBlock(int chunkX, int chunkZ, int cubeX, int cubeY, int cubeZ)
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

            return GetChunk(chunkX, chunkZ, true)[cubeX, cubeY, cubeZ];
        }
        #endregion

        #region Drawing

        /// <summary>
        /// How many chunks off in the distance you can see.
        /// If this changes, should call 
        /// chunkCache.SetDimensions(2*ChunkViewDistance+3)
        /// </summary>
        protected abstract int ChunkViewDistance { get; }

        public void Draw()
        {
            Vector3 translation = Vector3.Zero;

            for (int textureIndex = 0; textureIndex < BlockHandler.TotalNumberOfTextures; textureIndex++)
            {
                for (int chunkOffsetX = -ChunkViewDistance; chunkOffsetX <= ChunkViewDistance; chunkOffsetX++)
                {
                    translation.X = (chunkOffsetX << GameConstants.CHUNK_X_LOG);

                    for (int chunkOffsetZ = -ChunkViewDistance; chunkOffsetZ <= ChunkViewDistance; chunkOffsetZ++)
                    {
                        translation.Z = (chunkOffsetZ << GameConstants.CHUNK_Z_LOG);

                        Chunk toDraw = GetChunk(chunkOffsetX + Camera.ChunkX, chunkOffsetZ + Camera.ChunkZ, false);

                        if (toDraw != null)
                            toDraw.Draw(translation, textureIndex);
                    }
                }
            }
        }
        #endregion
    }
}

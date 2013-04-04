﻿using System;
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
        private int startCenterChunkX, startCenterChunkZ;

        public Map(BlockHandler handler, int startCenterChunkX, int startCenterChunkZ)
        {
            this.BlockHandler = handler;

            chunkCache = new ChunkCache(this, CacheRadius);

            lastCenterX = CenterChunkX;
            lastCenterZ = CenterChunkZ;

            this.startCenterChunkX = startCenterChunkX;
            this.startCenterChunkZ = startCenterChunkZ;
        }

        public void LoadContent(Game game)
        {
            chunkCache.StartCaching(startCenterChunkX, startCenterChunkZ, StartingChunkRadiusToLoad);

            setupEntityGeneration();
        }

        private int lastCenterX, lastCenterZ;

        public virtual void Update(GameTime gametime)
        {
            int dx = CenterChunkX - lastCenterX;
            int dz = CenterChunkZ - lastCenterZ;

            if (dx != 0 || dz != 0)
                CenterChanged(dx, dz);

            lastCenterX = CenterChunkX;
            lastCenterZ = CenterChunkZ;
        }

        private void CenterChanged(int dx, int dz)
        {
            entitiesLoaded.AdjustRange(dx, dz);
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
        /// 
        /// FUN FACT: you need to include a "buffer block" on each lateral
        /// (that is, x or z) side.  It is important that this is the same
        /// block as would actually be generated by the appropriate other call!
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <param name="chunkBlocksToFill">The array to fill with block data</param>
        /// <returns></returns>
        public abstract void MakeChunkData(int chunkX, int chunkZ, Block[, ,] chunkBlocksToFill, List<EntitySchema> entityListToFill);

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
            for (int x = cubeMin.X; x <= cubeMax.X; x++)
            {
                for (int y = cubeMin.Y; y <= cubeMax.Y; y++)
                {
                    for (int z = cubeMin.Z; z <= cubeMax.Z; z++)
                    {
                        Block block = GetHighPriorityBlock(chunkX, chunkZ, x, y, z);
                        if (BlockHandler.IsPassable(block))
                            continue;

                        yield return new Collider(block, chunkX, chunkZ, x, y, z, BlockHandler);
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
        private int CacheRadius { get { return ChunkViewDistance; } }

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
                chunkCache.ForceAddChunk(chunkX, chunkZ);
                return chunkCache.GetChunk(chunkX, chunkZ);
            }
            else
                return null;
        }

        /// <summary>
        /// This saves the specified chunk.  The only way it can fail is
        /// if the specified chunk is not actually loaded; so it returns
        /// false precisely when it successfully saves the chunk.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool SaveChunk(int chunkX, int chunkZ)
        {
            if (chunkCache.IsReady(chunkX, chunkZ))
            {
                chunkCache.SaveChunk(chunkX, chunkZ);
                return true;
            }
            else
            {
                return false;
            }
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
            int centerChunkX = CenterChunkX;
            int centerChunkZ = CenterChunkZ;

            Vector3 translation = Vector3.Zero;

            for (int textureIndex = 0; textureIndex < BlockHandler.TotalNumberOfTextures; textureIndex++)
            {
                for (int chunkOffsetX = -ChunkViewDistance; chunkOffsetX <= ChunkViewDistance; chunkOffsetX++)
                {
                    translation.X = chunkOffsetX * GameConstants.CHUNK_X_WIDTH;

                    for (int chunkOffsetZ = -ChunkViewDistance; chunkOffsetZ <= ChunkViewDistance; chunkOffsetZ++)
                    {
                        translation.Z = chunkOffsetZ * GameConstants.CHUNK_Z_LENGTH;

                        int chunkX = chunkOffsetX + centerChunkX;
                        int chunkZ = chunkOffsetZ + centerChunkZ;

                        Chunk toDraw = GetChunk(chunkX, chunkZ, false);

                        if (toDraw != null)
                        {
                            toDraw.Draw(translation, textureIndex);

                            if (toDraw.chunkX != chunkX || toDraw.chunkZ != chunkZ)
                                throw new NotImplementedException();
                        }
                    }
                }
            }
        }
        #endregion

        public int CenterChunkX { get { return Camera.ChunkX; } }
        public int CenterChunkZ { get { return Camera.ChunkZ; } }

        #region Generated Entity Handling
        private struct ChunkCoordinate
        {
            int X, Z;

            public ChunkCoordinate(int x, int z)
            {
                this.X = x;
                this.Z = z;
            }
        }

        private TorusBoolArray entitiesLoaded;

        private void setupEntityGeneration()
        {
            entitiesLoaded = new TorusBoolArray(EntitySpawnRadius, CenterChunkX, CenterChunkZ, false);
        }

        public IEnumerable<Entity> GenerateAllAvailableEntities(EntityBuilder builder, WorldManager manager)
        {
            for (int dx = -EntitySpawnRadius; dx <= EntitySpawnRadius; dx++)
            {
                for (int dz = -EntitySpawnRadius; dz <= EntitySpawnRadius; dz++)
                {
                    int chunkX = CenterChunkX + dx;
                    int chunkZ = CenterChunkZ + dz;

                    if (entitiesLoaded[chunkX, chunkZ])
                        continue;

                    Chunk chunk = GetChunk(chunkX, chunkZ, false);
                    if (chunk == null)
                        continue;

                    entitiesLoaded[chunkX, chunkZ] = true;

                    if (chunk.chunkX != chunkX || chunk.chunkZ != chunkZ)
                        throw new InvalidOperationException();

                    foreach (Entity entity in chunk.MakeGeneratedEntities(builder, manager))
                        yield return entity;
                }
            }
        }

        public virtual int EntitySpawnRadius { get { return ChunkViewDistance; } }
        public int EntityDespawnRadius { get { return EntitySpawnRadius + 1; } }
        #endregion

        /// <summary>
        /// This takes as input a position and a Ray and a maximum distance, then finds
        /// the first Block impacted by the Ray on this map, if any.  Return types are
        /// through a large number of "out" parameters.  Note it will never return the
        /// cell which contains the start of the Ray.
        /// 
        /// Note: if successful is FALSE, the returned data will be garbage (since it's
        /// not easily nullable).  So be aware of that.
        /// </summary>
        /// <param name="chunkX">The chunk(X) position to start the Ray from.  This will be
        /// set to the chunk(X) position we end on.</param>
        /// <param name="chunkZ">The chunk(Z) position to start the Ray from.  This will be
        /// set to the chunk(Z) position we end on.</param>
        /// <param name="lookRay">The Ray to look along.</param>
        /// <param name="maxDistance">The maximum distance along the ray that collisions
        /// will be considered.  Uses the MAX distance.</param>
        /// <param name="requireVisible">Whether or not to skip invisible blocks.</param>
        /// <param name="requireImpassable">Whether or not to skip passable blocks.</param>
        /// <param name="foundBlock">The block that was actually found.</param>
        /// <param name="blockPosition">The in-chunk (integer) position of the block that was found.</param>
        /// <param name="faceTouched">The face that was first touched by the Ray.</param>
        /// <param name="successful">Whether ot not anything was found</param>
        public void BlockLookedAt(ref int chunkX, ref int chunkZ, Ray lookRay, int maxDistance,
            bool requireVisible, bool requireImpassable,
            out Block foundBlock, out Point3 blockPosition,
            out Face faceTouched, out bool successful)
        {
            if (!requireVisible && !requireImpassable)
                throw new ArgumentException("I have to have something to clip!  Must have either \"requireVisible\" or \"requireImpassable\" to be true!");

            if (lookRay.Position.Y < 0 || lookRay.Position.Y >= GameConstants.CHUNK_Y_HEIGHT)
                throw new ArgumentException("Can't cast rays from offscreen!");

            successful = false;

            blockPosition = Point3.RoundDown(lookRay.Position);
            BoundingBox blockBounds = new BoundingBox(
                new Vector3(blockPosition.X, blockPosition.Y, blockPosition.Z),
                new Vector3(blockPosition.X + 1, blockPosition.Y + 1, blockPosition.Z + 1));

            if (!blockBounds.Intersects(lookRay).HasValue)
                throw new NotImplementedException();

            int xChange = Numerical.sign(lookRay.Direction.X);
            int yChange = Numerical.sign(lookRay.Direction.Y);
            int zChange = Numerical.sign(lookRay.Direction.Z);

            faceTouched = Face.FRONT;
            foundBlock = new Block();

            while (blockPosition.Y >= 0 && blockPosition.Y < GameConstants.CHUNK_Y_HEIGHT)
            {
                bool canMoveX = (xChange != 0);
                bool canMoveY = (yChange != 0 && (blockPosition.Y + yChange >= 0 && blockPosition.Y + yChange < GameConstants.CHUNK_Y_HEIGHT));
                bool canMoveZ = (zChange != 0);

                bool hitX = false;
                bool hitY = false;
                bool hitZ = false;

                BoundingBox oldBox = blockBounds;

                int hits = 0;

                #region Move and count hits...
                if (canMoveX)
                {
                    BoundingBox box = new BoundingBox(oldBox.Min + new Vector3(xChange, 0, 0), oldBox.Max + new Vector3(xChange, 0, 0));

                    float? result = box.Intersects(lookRay);
                    hitX = result.HasValue;

                    if (hitX)
                    {
                        hits++;

                        blockPosition.X += xChange;
                        faceTouched = (xChange < 0 ? Face.RIGHT : Face.LEFT);
                        blockBounds = box;

                        if (Math.Abs(blockPosition.X - lookRay.Position.X) > maxDistance) return;
                    }
                }

                if (canMoveY)
                {
                    BoundingBox box = new BoundingBox(oldBox.Min + new Vector3(0, yChange, 0), oldBox.Max + new Vector3(0, yChange, 0));

                    float? result = box.Intersects(lookRay);
                    hitY = result.HasValue;

                    if (hitY)
                    {
                        hits++;

                        blockPosition.Y += yChange;
                        faceTouched = (yChange < 0 ? Face.TOP : Face.BOTTOM);
                        blockBounds = box;

                        if (Math.Abs(blockPosition.Y - lookRay.Position.Y) > maxDistance) return;
                    }
                }

                if (canMoveZ)
                {
                    BoundingBox box = new BoundingBox(oldBox.Min + new Vector3(0, 0, zChange), oldBox.Max + new Vector3(0, 0, zChange));

                    float? result = box.Intersects(lookRay);
                    hitZ = result.HasValue;

                    if (hitZ)
                    {
                        hits++;

                        blockPosition.Z += zChange;
                        faceTouched = (zChange < 0 ? Face.BACK : Face.FRONT);
                        blockBounds = box;

                        if (Math.Abs(blockPosition.Z - lookRay.Position.Z) > maxDistance) return;
                    }
                }
                #endregion

                //We either hit a corner (yielding multiple matches) or ran off
                //the edge of the world.  In either case, I'm OK with "no result."
                if (hits != 1)
                {
                    return;
                }

                foundBlock = GetHighPriorityBlock(chunkX, chunkZ, blockPosition.X, blockPosition.Y, blockPosition.Z);

                bool compatible = true;

                if (requireVisible)
                {
                    if (BlockHandler.IsVisible(foundBlock))
                    {
                        BoundingBox visualBox = BlockHandler.VisualBoundingBox(foundBlock);
                        visualBox = new BoundingBox(
                            visualBox.Min + new Vector3(blockBounds.Min.X, blockBounds.Min.Y, blockBounds.Min.Z),
                            visualBox.Max + new Vector3(blockBounds.Min.X, blockBounds.Min.Y, blockBounds.Min.Z));
                        compatible = compatible && visualBox.Intersects(lookRay).HasValue;
                    }
                    else
                    {
                        compatible = false;
                    }
                }

                if (true)
                {
                }

                if (requireImpassable)
                {
                    if (BlockHandler.IsPassable(foundBlock))
                    {
                        compatible = false;
                    }
                    else
                    {
                        BoundingBox blockingBox = BlockHandler.PhysicalBlockingBox(foundBlock);
                        blockingBox = new BoundingBox(
                            blockingBox.Min + new Vector3(blockBounds.Min.X, blockBounds.Min.Y, blockBounds.Min.Z),
                            blockingBox.Max + new Vector3(blockBounds.Min.X, blockBounds.Min.Y, blockBounds.Min.Z));
                        compatible = compatible && blockingBox.Intersects(lookRay).HasValue;
                    }
                }

                if (compatible)
                {
                    successful = true;

                    #region Fix indexing...
                    while (blockPosition.X < 0)
                    {
                        blockPosition.X += GameConstants.CHUNK_X_WIDTH;
                        chunkX--;
                    }

                    while (blockPosition.X >= GameConstants.CHUNK_X_WIDTH)
                    {
                        blockPosition.X -= GameConstants.CHUNK_X_WIDTH;
                        chunkX++;
                    }

                    while (blockPosition.Z < 0)
                    {
                        blockPosition.Z += GameConstants.CHUNK_Z_LENGTH;
                        chunkZ--;
                    }

                    while (blockPosition.Z >= GameConstants.CHUNK_Z_LENGTH)
                    {
                        blockPosition.Z -= GameConstants.CHUNK_Z_LENGTH;
                        chunkZ++;
                    }
                    #endregion

                    return;
                }
            }
        }
    }
}

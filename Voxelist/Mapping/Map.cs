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
        #region Properties and Fields
        /// <summary>
        /// The BlockHandler which affects and afflicts all methods
        /// involved in the creation and upkeep of this Map.
        /// </summary>
        private BlockHandler BlockHandler { get; set; }

        /// <summary>
        /// The current Center Chunk coordinate (X) of this Map.
        /// Typically tied to the Chunk position of the Camera,
        /// though they should not be assumed to be always equal.
        /// </summary>
        public int CenterChunkX { get; private set; }
        
        /// <summary>
        /// The current Center Chunk coordinate (Z) of this Map.
        /// Typically tied to the Chunk position of the Camera,
        /// though they should not be assumed to be always equal.
        /// </summary>
        public int CenterChunkZ { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new map.  Requires a BlockHandler to get things done, as well as
        /// a starting center position to begin caching from.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="startCenterChunkX"></param>
        /// <param name="startCenterChunkZ"></param>
        public Map(BlockHandler handler, int startCenterChunkX, int startCenterChunkZ)
        {
            this.BlockHandler = handler;

            this.CenterChunkX = startCenterChunkX;
            this.CenterChunkZ = startCenterChunkZ;

            this.Cache = new ChunkCache(this, handler, CacheRadius);
        }
        #endregion

        #region Procedural Generation
        /// <summary>
        /// This is where you get to do your procedural generation!
        /// By whatever means you deem appropriate, construct a chunk
        /// of cubes at the designated "chunk coordinates."
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
        #endregion

        #region Upkeep Methods
        /// <summary>
        /// Loads Content for the Map.  Default behavior starts the Caching and is
        /// pretty necessary!  So, overrides should call the base method.
        /// </summary>
        /// <param name="game"></param>
        public virtual void LoadContent(Game game)
        {
            Cache.StartCaching(CenterChunkX, CenterChunkZ, StartingChunkRadiusToLoad);
        }

        /// <summary>
        /// Disposes the Map.  Not empty, so overrides should call the base method.
        /// </summary>
        public virtual void Dispose()
        {
            Cache.Dispose();
        }

        /// <summary>
        /// Updates the map.  Default behavior is used for Entity
        /// management, so overrides should call the base method.
        /// </summary>
        /// <param name="gametime"></param>
        public virtual void Update(GameTime gametime)
        {
            int dx = Camera.ChunkX - CenterChunkX;
            int dz = Camera.ChunkZ - CenterChunkZ;

            if (dx != 0 || dz != 0)
            {
                CenterChanged(dx, dz);
                CenterChunkX += dx;
                CenterChunkZ += dz;
            }
        }

        private void CenterChanged(int dx, int dz)
        {
            Cache.ChangeCenterCoordinates(dx, dz);
        }
        #endregion

        #region Caching
        /// <summary>
        /// The Cache object which does all the real content management.
        /// </summary>
        private ChunkCache Cache;

        /// <summary>
        /// How many squares away from, and not including, the center square
        /// to keep loaded at a time.  The default is ViewRadius+1, and the
        /// only other obvious choice would be ViewRadius (if cache loading
        /// becomes a major performance problem, but this will result in
        /// obvious load times).
        /// 
        /// Note this should be constant throughout gameplay; the engine does not
        /// take into account changes to this, and may behave strangely if you do.
        /// </summary>
        protected virtual int CacheRadius { get { return ViewRadius + 1; } }

        /// <summary>
        /// How much of the Map to generate before play can start.  This is
        /// set to 1, which means load the current center square and one more
        /// in every direction.
        /// </summary>
        private int StartingChunkRadiusToLoad { get { return 1; } }

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
            if (Cache.IsReady(chunkX, chunkZ))
                return Cache.GetChunk(chunkX, chunkZ);

            else if (urgent)
            {
                Cache.ForceAddChunk(chunkX, chunkZ);
                return Cache.GetChunk(chunkX, chunkZ);
            }

            else
                return null;
        }

        private Block GetHighPriorityBlock(int chunkX, int chunkZ, int cubeX, int cubeY, int cubeZ)
        {
            Numerical.RepairBlockCoordinates(ref chunkX, ref chunkZ, ref cubeX, ref cubeZ);

            return GetChunk(chunkX, chunkZ, true)[cubeX, cubeY, cubeZ];
        }
        #endregion

        #region Rendering
        /// <summary>
        /// The distance from the Center (not including the Center) at which
        /// Chunks are visible and drawn.
        /// 
        /// Note this should be constant throughout gameplay; the engine does not
        /// take into account changes to this, and may behave strangely if you do.
        /// </summary>
        public abstract int ViewRadius { get; }

        /// <summary>
        /// Draws the Map using the data from the Camera.
        /// </summary>
        public void Draw()
        {
            int centerChunkX = CenterChunkX;
            int centerChunkZ = CenterChunkZ;

            Vector3 translation = Vector3.Zero;

            for (int textureIndex = 0; textureIndex < BlockHandler.TotalNumberOfTextures; textureIndex++)
            {
                for (int chunkOffsetX = -ViewRadius; chunkOffsetX <= ViewRadius; chunkOffsetX++)
                {
                    translation.X = chunkOffsetX * GameConstants.CHUNK_X_WIDTH;

                    for (int chunkOffsetZ = -ViewRadius; chunkOffsetZ <= ViewRadius; chunkOffsetZ++)
                    {
                        translation.Z = chunkOffsetZ * GameConstants.CHUNK_Z_LENGTH;

                        int chunkX = chunkOffsetX + centerChunkX;
                        int chunkZ = chunkOffsetZ + centerChunkZ;

                        Chunk drawableChunk = GetChunk(chunkX, chunkZ, false);

                        if (drawableChunk != null)
                            drawableChunk.Draw(translation, textureIndex);
                    }
                }
            }
        }
        #endregion

        #region Generated Entity Management
        /// <summary>
        /// The range at which generated Entities are spawned (that is, come
        /// into existence, instead of being inert schemas on the Chunks). This
        /// should be at most the ViewDistance, certainly, but could be less for
        /// performance reasons (depending on the number of Entities you have,
        /// and your ViewDistance).
        /// 
        /// Note this should be constant throughout gameplay; the engine does not
        /// take into account changes to this, and may behave strangely if you do.
        /// </summary>
        public abstract int EntitySpawnRadius { get; }

        /// <summary>
        /// Generates all available (that is, within spawn range but not yet spawned)
        /// entities.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Entity> GenerateAllAvailableEntities(EntityBuilder builder, WorldManager manager)
        {
            for (int x = CenterChunkX - EntitySpawnRadius; x <= CenterChunkX + EntitySpawnRadius; x++)
            {
                for (int z = CenterChunkZ - EntitySpawnRadius; z <= CenterChunkZ + EntitySpawnRadius; z++)
                {
                    foreach (Entity e in Cache.GenerateAvailableEntities(x, z, builder, manager))
                        yield return e;
                }
            }
        }

        public void AddEntityToCache(Entity e, ChunkCoordinate chunkMin, ChunkCoordinate chunkMax)
        {
            for (int x = chunkMin.X; x <= chunkMax.X; x++)
            {
                for (int z = chunkMin.Z; z <= chunkMax.Z; z++)
                {
                    Cache.AddEntity(e, x, z);
                }
            }
        }

        public void RemoveEntityFromCache(Entity e, ChunkCoordinate chunkMin, ChunkCoordinate chunkMax)
        {
            for (int x = chunkMin.X; x <= chunkMax.X; x++)
            {
                for (int z = chunkMin.Z; z <= chunkMax.Z; z++)
                {
                    Cache.RemoveEntity(e, x, z);
                }
            }
        }

        public IEnumerable<Entity> CachedEntities(ChunkCoordinate chunkMin, ChunkCoordinate chunkMax)
        {
            for (int x = chunkMin.X; x <= chunkMax.X; x++)
            {
                for (int z = chunkMin.Z; z <= chunkMax.Z; z++)
                {
                    foreach (Entity e in Cache.CachedEntities(x, z))
                        yield return e;
                }
            }
        }
        #endregion

        #region Physics-like Methods
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
        #endregion
    }
}

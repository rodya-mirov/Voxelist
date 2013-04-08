﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Voxelist.Utilities;
using Voxelist.Entities;
using Voxelist.BlockHandling;
using Voxelist.Rendering;

namespace Voxelist.Mapping
{
    /// <summary>
    /// Multithreading rules- never run the OverwriteChunkData in a
    /// locked portion, and lock literally everything else (everything
    /// else runs pretty much instantly).
    /// </summary>
    public class ChunkCache
    {
        private Map Map { get; set; }
        private BlockHandler BlockHandler { get; set; }

        private int EntitySpawnRadius { get { return Map.EntitySpawnRadius; } }

        public ChunkCache(Map map, BlockHandler handler, int cacheRadius)
        {
            this.Map = map;
            this.BlockHandler = handler;

            this.Radius = cacheRadius;

            this.savedChunkData = new Dictionary<ChunkCoordinate, CacheData>();

            LoadStartingData();
        }

        public void Dispose()
        {
            loaderShouldKeepRunning = false;
        }

        private int IntendedCenterX { get; set; }
        private int IntendedCenterZ { get; set; }

        public void ChangeCenterCoordinates(int dx, int dz)
        {
            int newCenterX = IntendedCenterX + dx;
            int newCenterZ = IntendedCenterZ + dz;

            DespawnFarawayChunks(newCenterX, newCenterZ);

            IntendedCenterX = newCenterX;
            IntendedCenterZ = newCenterZ;
        }

        private void DespawnFarawayChunks(int newCenterX, int newCenterZ)
        {
            for (int x = IntendedCenterX - EntitySpawnRadius; x <= IntendedCenterX + EntitySpawnRadius; x++)
            {
                for (int z = IntendedCenterZ - EntitySpawnRadius; z <= IntendedCenterZ + EntitySpawnRadius; z++)
                {
                    if (IsInGridRange(x, z) && IsReady(x, z))
                    {
                        int dist = Math.Abs(x - newCenterX) + Math.Abs(z - newCenterZ);
                        if (dist > EntitySpawnRadius)
                            this[x, z].Despawn();
                    }
                }
            }
        }

        #region Grid Data Storage
        private CacheData[,] cache;

        private int Radius { get; set; }
        private int Width { get { return 2 * Radius + 1; } }
        private int Height { get { return 2 * Radius + 1; } }

        private int XMin { get; set; }
        private int XMax { get { return XMin + Width - 1; } }

        private int ZMin { get; set; }
        private int ZMax { get { return ZMin + Height - 1; } }

        private void LoadStartingData()
        {
            cache = new CacheData[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Height; z++)
                {
                    cache[x, z] = new CacheData();
                }
            }
        }

        private bool IsInGridRange(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                return (XMin <= chunkX && chunkX <= XMax && ZMin <= chunkZ && chunkZ <= ZMax);
            }
        }

        /// <summary>
        /// Returns the CacheData at a specific point from the grid array.  Does not
        /// interfere with the savedChunk data (loading or saving).  Throws an
        /// IndexOutOfRangeException if the specified chunk coordinates are out
        /// of the range of the grid.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        private CacheData this[int chunkX, int chunkZ]
        {
            get
            {
                lock (CacheLock)
                {
                    ChunkCoordinate coord = new ChunkCoordinate(chunkX, chunkZ);
                    if (savedChunkData.ContainsKey(coord))
                        return savedChunkData[coord];

                    if (!IsInGridRange(chunkX, chunkZ))
                        throw new IndexOutOfRangeException();

                    int xIndex = Numerical.IntMod(chunkX, Width);
                    int zIndex = Numerical.IntMod(chunkZ, Height);

                    return cache[xIndex, zIndex];
                }
            }

            set
            {
                lock (CacheLock)
                {
                    if (!IsInGridRange(chunkX, chunkZ))
                        throw new IndexOutOfRangeException();

                    int xIndex = Numerical.IntMod(chunkX, Width);
                    int zIndex = Numerical.IntMod(chunkZ, Height);

                    cache[xIndex, zIndex] = value;
                }
            }
        }

        #region Grid Movement
        /// <summary>
        /// This makes sure the desired chunk is within the cache bounds,
        /// but does not create any new cached chunks right now.  It may
        /// actually (probably will) lose some existing data though!
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        public void Request(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                int oldXMin = XMin;
                int oldZMin = ZMin;

                while (chunkZ < ZMin)
                    addTopRow();

                while (chunkZ > ZMax)
                    addBottomRow();

                while (chunkX < XMin)
                    addLeftColumn();

                while (chunkX > XMax)
                    addRightColumn();
            }
        }

        /// <summary>
        /// Reduces ZMin by 1, and alters the cache appropriately,
        /// which in this case means invalidating all the data for
        /// ZMax.  Should only be called within Request, because it
        /// doesn't allocate the lock!
        /// </summary>
        private void addTopRow()
        {
            //the old maximum becomes the new minimum
            for (int chunkX = XMin; chunkX <= XMax; chunkX++)
                this[chunkX, ZMax].Invalidate(chunkX, ZMin - 1, this);

            ZMin--;
        }

        /// <summary>
        /// Increases ZMin by 1, and alters the cache appropriately,
        /// which in this case means invalidating all the data for
        /// ZMin.  Should only be called within Request, because it
        /// doesn't allocate the lock! 
        /// </summary>
        private void addBottomRow()
        {
            //the old minimum becomes the new maximum
            for (int chunkX = XMin; chunkX <= XMax; chunkX++)
                this[chunkX, ZMin].Invalidate(chunkX, ZMax + 1, this);

            ZMin++;
        }

        /// <summary>
        /// Decreases XMin by 1, and alters the cache appropriately,
        /// which in this case means invalidating all the data for
        /// XMax.  Should only be called within Request, because it
        /// doesn't  allocate the lock!
        /// </summary>
        private void addLeftColumn()
        {
            for (int chunkZ = ZMin; chunkZ <= ZMax; chunkZ++)
                this[XMax, chunkZ].Invalidate(XMin - 1, chunkZ, this);

            XMin--;
        }

        /// <summary>
        /// Increases XMix by 1, and alters the cache appropriately,
        /// which in this case means invalidating all the data for
        /// XMin.  Should only be called within Request, because it
        /// doesn't allocate the lock!
        /// </summary>
        private void addRightColumn()
        {
            for (int chunkZ = ZMin; chunkZ <= ZMax; chunkZ++)
                this[XMin, chunkZ].Invalidate(XMax + 1, chunkZ, this);

            XMin++;
        }
        #endregion
        #endregion

        #region Saved Chunks
        private Dictionary<ChunkCoordinate, CacheData> savedChunkData;

        /// <summary>
        /// Whether or not there is cached data at the specified chunk coordinate.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool HasSavedChunk(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                return savedChunkData.ContainsKey(new ChunkCoordinate(chunkX, chunkZ));
            }
        }

        /// <summary>
        /// Attempts to save the specified chunk for later.  If the chunk was fully
        /// loaded, it saves it and returns true.  If the chunk was NOT fully loaded,
        /// it just returns false (does not attempt to "save it later.")
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool SaveChunk(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                if (IsReady(chunkX, chunkZ))
                {
                    CacheData dat = this[chunkX, chunkZ];
                    savedChunkData[new ChunkCoordinate(chunkX, chunkZ)] = dat;

                    CacheData newData = new CacheData();
                    newData.CleanAndValidate();
                    this[chunkX, chunkZ] = newData;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Determines whether the specified chunk coordinate is currently
        /// read to load and use.  If the answer is no, this method
        /// returns false (no automatic requests).
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool IsReady(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                if (HasSavedChunk(chunkX, chunkZ))
                {
                    return true;
                }
                if (IsInGridRange(chunkX, chunkZ))
                {
                    return !this[chunkX, chunkZ].NeedsToBeLoaded;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the chunk at the specified coordinates.  If the chunk at that
        /// position is not ready, it will throw an error.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public Chunk GetChunk(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                if (HasSavedChunk(chunkX, chunkZ))
                    return savedChunkData[new ChunkCoordinate(chunkX, chunkZ)].Chunk;
                else
                {
                    CacheData dat = this[chunkX, chunkZ];
                    if (dat.NeedsToBeLoaded)
                        throw new ArgumentOutOfRangeException("Chunk is not yet ready!");
                    else
                        return dat.Chunk;
                }
            }
        }

        public void StartCaching(int centerChunkX, int centerChunkZ, int initialRadius)
        {
            XMin = centerChunkX - Radius;
            ZMin = centerChunkZ - Radius;

            IntendedCenterX = centerChunkX;
            IntendedCenterZ = centerChunkZ;

            for (int chunkX = centerChunkX - initialRadius; chunkX <= centerChunkX + initialRadius; chunkX++)
            {
                for (int chunkZ = centerChunkZ - initialRadius; chunkZ <= centerChunkZ + initialRadius; chunkZ++)
                {
                    ForceAddChunk(chunkX, chunkZ);
                }
            }

            StartLoaderThread();
        }

        /// <summary>
        /// Forces the Cache to add a new chunk immediately.  This is usually
        /// a bad idea in terms of slowdown, but sometimes slowdown is better
        /// than the other bugs it could create.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        public void ForceAddChunk(int chunkX, int chunkZ)
        {
            lock (CacheLock)
            {
                Request(chunkX, chunkZ);

                MakeAndCacheChunk(chunkX, chunkZ);
            }
        }
        #endregion

        #region Multithreading
        private readonly object CacheLock = new object();

        private Thread cacheLoaderThread;
        private bool loaderShouldKeepRunning = true;

        private void StartLoaderThread()
        {
            loaderShouldKeepRunning = true;
            cacheLoaderThread = new Thread(LoaderThreadMethod);

            cacheLoaderThread.Start();
        }

        /// <summary>
        /// This is the method which the cacheLoaderThread runs
        /// over and over and over, until loaderShouldKeepRunning
        /// is false;
        /// </summary>
        private void LoaderThreadMethod()
        {
            while (loaderShouldKeepRunning)
            {
                bool foundChunkToLoad;
                int loadChunkX, loadChunkZ;

                FindNeededChunk(out foundChunkToLoad, out loadChunkX, out loadChunkZ);

                if (foundChunkToLoad)
                    MakeAndCacheChunk(loadChunkX, loadChunkZ);

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// If there is a currently un-cached chunk which we need,
        /// finds it, and returns true and the coordinates.  Otherwise
        /// returns false.  In terms of precedence, it finds the closest
        /// needed un-cached chunk to the center.
        /// </summary>
        /// <param name="foundChunkToLoad"></param>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        private void FindNeededChunk(out bool foundChunkToLoad, out int chunkX, out int chunkZ)
        {
            //start with dumb starter values in case we don't find anything
            chunkX = 0;
            chunkZ = 0;

            lock (CacheLock)
            {
                int centerX = XMin + Radius;
                int centerZ = ZMin + Radius;

                int bestScore = int.MaxValue;
                CacheData bestData = null;

                //find the closest necessary square to the center
                for (int x = XMin; x <= XMax; x++)
                {
                    for (int z = ZMin; z <= ZMax; z++)
                    {
                        CacheData tempData = this[x, z];
                        int score = Math.Abs(x - centerX) + Math.Abs(z - centerZ);

                        if (score < bestScore && tempData.NeedsToBeLoaded)
                        {
                            bestScore = score;
                            bestData = tempData;

                            chunkX = x;
                            chunkZ = z;
                        }
                    }
                }

                //this is how we know we found something :)
                foundChunkToLoad = (bestData != null);
            }
        }

        /// <summary>
        /// Part of the helper thread method.  Locates the appropriate
        /// piece of CacheData, fills it with good data, then marks it
        /// as prepared.
        /// </summary>
        /// <param name="loadChunkX"></param>
        /// <param name="loadChunkZ"></param>
        private void MakeAndCacheChunk(int loadChunkX, int loadChunkZ)
        {
            CacheData cacheData;

            lock (CacheLock)
            {
                cacheData = this[loadChunkX, loadChunkZ];
                if (cacheData.Chunk == null)
                    cacheData.GiveBlankChunk(BlockHandler, Map);
            }

            cacheData.Chunk.OverwriteChunkDataWith(loadChunkX, loadChunkZ);

            lock (CacheLock)
            {
                cacheData.CleanAndValidate();
            }
        }
        #endregion

        #region CacheData Class
        private class CacheData
        {
            public Chunk Chunk { get; private set; }
            public bool NeedsToBeLoaded { get; private set; }

            public HashSet<Entity> TouchingEntities { get; private set; }

            public bool EntitiesSpawned { get; private set; }

            public CacheData()
            {
                Chunk = null;
                NeedsToBeLoaded = true;

                EntitiesSpawned = false;
                TouchingEntities = new HashSet<Entity>();
            }

            /// <summary>
            /// Called when the data in this object is no longer what we
            /// want it to be.  The arguments are used to determine if we
            /// really need to mark this as a new task (if it's already
            /// saved, don't bother, it's just extra work).
            /// </summary>
            /// <param name="newChunkX"></param>
            /// <param name="newChunkZ"></param>
            /// <param name="cache"></param>
            public void Invalidate(int newChunkX, int newChunkZ, ChunkCache cache)
            {
                if (cache.HasSavedChunk(newChunkX, newChunkZ))
                    NeedsToBeLoaded = false;
                else
                    NeedsToBeLoaded = true;

                EntitiesSpawned = false;
                TouchingEntities.Clear();
            }

            /// <summary>
            /// Called when the data in this object is now (but not previously)
            /// what we want it to be.
            /// </summary>
            public void CleanAndValidate()
            {
                TouchingEntities.Clear();
                NeedsToBeLoaded = false;
                EntitiesSpawned = false;
            }

            public void Despawn()
            {
                EntitiesSpawned = false;
            }

            public void Spawn()
            {
                EntitiesSpawned = true;
            }

            public void GiveBlankChunk(BlockHandler handler, Map map)
            {
                Chunk = new Chunk(handler, map);
            }
        }
        #endregion

        #region Entity Collision Caching
        public void RemoveEntity(Entity entity, int chunkX, int chunkZ)
        {
            if (HasSavedChunk(chunkX, chunkZ))
            {
                savedChunkData[new ChunkCoordinate(chunkX, chunkZ)].TouchingEntities.Remove(entity);
            }
            else if (IsInGridRange(chunkX, chunkZ) && IsReady(chunkX, chunkZ))
            {
                this[chunkX, chunkZ].TouchingEntities.Remove(entity);
            }
            else
            {
                throw new IndexOutOfRangeException("Specified chunk is not loaded!");
            }
        }

        public void AddEntity(Entity entity, int chunkX, int chunkZ)
        {
            if (HasSavedChunk(chunkX, chunkZ))
            {
                savedChunkData[new ChunkCoordinate(chunkX, chunkZ)].TouchingEntities.Add(entity);
            }
            else if (IsInGridRange(chunkX, chunkZ) && IsReady(chunkX, chunkZ))
            {
                this[chunkX, chunkZ].TouchingEntities.Add(entity);
            }
            else
            {
                throw new IndexOutOfRangeException("Specified chunk is not loaded!");
            }
        }

        public IEnumerable<Entity> CachedEntities(int chunkX, int chunkZ)
        {
            if (HasSavedChunk(chunkX, chunkZ))
            {
                foreach (Entity e in savedChunkData[new ChunkCoordinate(chunkX, chunkZ)].TouchingEntities)
                    yield return e;
            }
            else if (IsInGridRange(chunkX, chunkZ) && IsReady(chunkX, chunkZ))
            {
                foreach (Entity e in this[chunkX, chunkZ].TouchingEntities)
                    yield return e;
            }
            else
            {
                throw new IndexOutOfRangeException("Specified chunk is not loaded!");
            }
        }

        public IEnumerable<Entity> GenerateAvailableEntities(int chunkX, int chunkZ, EntityBuilder builder, WorldManager manager)
        {
            //the following is a big block of checks to avoid 
            if (!IsInGridRange(chunkX, chunkZ) || !IsReady(chunkX, chunkZ))
                yield break;

            int dist = Math.Abs(chunkX - IntendedCenterX) + Math.Abs(chunkZ - IntendedCenterZ);
            if (dist > EntitySpawnRadius)
                yield break;

            CacheData dat = this[chunkX, chunkZ];

            if (dat.EntitiesSpawned)
                yield break;

            dat.Spawn();

            foreach (Entity e in dat.Chunk.MakeGeneratedEntities(builder, manager))
                yield return e;
        }
        #endregion
    }
}

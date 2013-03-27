using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Voxelist.Utilities;

namespace Voxelist.Mapping
{
    public class ChunkCache
    {
        //The cache is a square, and this is a side length.
        //This will always be odd, because it's entered as a radius
        //(so you get 2*r+1 on each side).  This is actually used in
        //one method (the loader method, to help it circle the center)
        //so be aware of that if you change it.
        private int Cache_Size;

        private Chunk[,] cachedChunks;
        private Map map;

        #region Dimensions
        public int ChunkXMin { get; private set; }
        public int ChunkXMax
        {
            get { return ChunkXMin + Cache_Size - 1; }
            private set { ChunkXMin = value - Cache_Size + 1; }
        }

        public int ChunkZMin { get; private set; }
        public int ChunkZMax
        {
            get { return ChunkZMin + Cache_Size - 1; }
            private set { ChunkZMin = value - Cache_Size + 1; }
        }

        private int xStartIndex;
        private int zStartIndex;
        #endregion

        #region Cache Loading Thread Stuff
        private bool[,] cacheIsValid;
        private Object validBitsLock = new Object();

        private Thread cacheLoaderThread;

        private void LoaderThreadMethod()
        {
            while (keepCacheLoaderRunning)
            {
                int chunkXtoFind, chunkZtoFind;
                bool foundGoal = false;
                Chunk copyOver = null;

                //first, find your next favorite target
                lock (validBitsLock)
                {
                    chunkXtoFind = 0;
                    chunkZtoFind = 0;

                    int centerX = ChunkXMin + (Cache_Size / 2);
                    int centerZ = ChunkZMin + (Cache_Size / 2);

                    int centerXIndex = Numerical.IntMod(centerX - ChunkXMin + xStartIndex, Cache_Size);
                    int centerZIndex = Numerical.IntMod(centerZ - ChunkZMin + zStartIndex, Cache_Size);

                    if (!cacheIsValid[centerXIndex, centerZIndex])
                    {
                        chunkXtoFind = centerX;
                        chunkZtoFind = centerZ;
                        copyOver = cachedChunks[centerXIndex, centerZIndex];

                        foundGoal = true;
                    }
                    else
                    {
                        //we go in this circular pattern, which gives good results
                        //but looks fairly weird in code form
                        int radius = 1;
                        while (!foundGoal && radius * 2 + 1 <= Cache_Size)
                        {
                            int leftX = centerX - radius;
                            int rightX = centerX + radius;
                            int topZ = centerZ - radius;
                            int bottomZ = centerZ + radius;

                            int leftActualIndex = Numerical.IntMod(leftX - ChunkXMin + xStartIndex, Cache_Size);
                            int rightActualIndex = Numerical.IntMod(rightX - ChunkXMin + xStartIndex, Cache_Size);
                            int topActualIndex = Numerical.IntMod(topZ - ChunkZMin + zStartIndex, Cache_Size);
                            int bottomActualIndex = Numerical.IntMod(bottomZ - ChunkZMin + zStartIndex, Cache_Size);

                            //check the left/right edges of the square...
                            for (int z = topZ; !foundGoal && z <= bottomZ; z++)
                            {
                                int actualZIndex = Numerical.IntMod(z - ChunkZMin + zStartIndex, Cache_Size);
                                if (!cacheIsValid[leftActualIndex, actualZIndex])
                                {
                                    chunkXtoFind = leftX;
                                    chunkZtoFind = z;
                                    foundGoal = true;
                                    copyOver = cachedChunks[leftActualIndex, actualZIndex];
                                }
                                else if (!cacheIsValid[rightActualIndex, actualZIndex])
                                {
                                    chunkXtoFind = rightX;
                                    chunkZtoFind = z;
                                    copyOver = cachedChunks[rightActualIndex, actualZIndex];
                                    foundGoal = true;
                                }
                            }

                            //check the top/bottom edges of the square...
                            for (int x = leftX; !foundGoal && x <= rightX; x++)
                            {
                                int actualXIndex = Numerical.IntMod(x - ChunkXMin + xStartIndex, Cache_Size);
                                if (!cacheIsValid[actualXIndex, topActualIndex])
                                {
                                    chunkXtoFind = x;
                                    chunkZtoFind = topZ;
                                    foundGoal = true;
                                    copyOver = cachedChunks[actualXIndex, topActualIndex];
                                }
                                else if (!cacheIsValid[actualXIndex, bottomActualIndex])
                                {
                                    chunkXtoFind = x;
                                    chunkZtoFind = bottomZ;
                                    foundGoal = true;
                                    copyOver = cachedChunks[actualXIndex, bottomActualIndex];
                                }
                            }

                            radius += 1;
                        }
                    }
                }

                //if we didn't find anything, take a break and let the game run harder
                if (!foundGoal)
                {
                    Thread.Sleep(10);
                }
                else
                {
                    //otherwise, actually make the chunk (this could take a while)
                    if (copyOver == null)
                        copyOver = new Chunk(map.MakeChunkBlocks(chunkXtoFind, chunkZtoFind), map.BlockHandler);
                    else
                        copyOver.OverwriteWith(map.MakeChunkBlocks(chunkXtoFind, chunkZtoFind));

                    //now stick it back in
                    lock (validBitsLock)
                    {
                        //sometimes the goalposts move on you ...
                        if (chunkXtoFind < ChunkXMin || chunkXtoFind > ChunkXMax || chunkZtoFind < ChunkZMin || chunkZtoFind > ChunkZMax)
                            continue;

                        int xIndex = Numerical.IntMod(chunkXtoFind - ChunkXMin + xStartIndex, Cache_Size);
                        int zIndex = Numerical.IntMod(chunkZtoFind - ChunkZMin + zStartIndex, Cache_Size);

                        cacheIsValid[xIndex, zIndex] = true;
                        cachedChunks[xIndex, zIndex] = copyOver;
                    }
                }

                Thread.Sleep(10);
            }

            cacheLoaderIsRunning = false;
        }
        #endregion

        public ChunkCache(Map map, int cacheRadius)
        {
            this.map = map;

            Cache_Size = cacheRadius * 2 + 1;

            cachedChunks = new Chunk[Cache_Size, Cache_Size];
            cacheIsValid = new bool[Cache_Size, Cache_Size];

            cacheLoaderThread = new Thread(this.LoaderThreadMethod);
        }

        private bool keepCacheLoaderRunning = true;
        private bool cacheLoaderIsRunning = false;

        public void Dispose()
        {
            keepCacheLoaderRunning = false;
        }

        public void SetCacheDimensions(int cacheDimensions)
        {
            keepCacheLoaderRunning = false;

            while (cacheLoaderIsRunning)
                Thread.Sleep(0);

            keepCacheLoaderRunning = true;

            //something
            throw new NotImplementedException();
        }

        public void StartCaching(int chunkX, int chunkZ, int requiredStartRadius)
        {
            lock (validBitsLock)
            {
                this.ChunkXMin = chunkX - Cache_Size / 2;
                this.ChunkZMin = chunkZ - Cache_Size / 2;

                this.xStartIndex = 0;
                this.zStartIndex = 0;

                if (requiredStartRadius * 2 + 1 >= Cache_Size)
                    requiredStartRadius = Cache_Size / 2;

                for (int x = chunkX - requiredStartRadius; x <= chunkX + requiredStartRadius; x++)
                {
                    for (int z = chunkZ - requiredStartRadius; z <= chunkZ + requiredStartRadius; z++)
                    {
                        int xIndex = Numerical.IntMod(x - ChunkXMin + xStartIndex, Cache_Size);
                        int zIndex = Numerical.IntMod(z - ChunkZMin + zStartIndex, Cache_Size);

                        cachedChunks[xIndex, zIndex] = new Chunk(map.MakeChunkBlocks(x, z), map.BlockHandler);
                        cacheIsValid[xIndex, zIndex] = true;
                    }
                }
            }

            cacheLoaderIsRunning = true;
            cacheLoaderThread.Start();
        }

        /// <summary>
        /// Determines whether the specified chunk coordinate is currently
        /// read to load and use.  If the answer is no, this method
        /// automatically puts in a request for it (equivalent to calling
        /// Request).
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool IsReady(int chunkX, int chunkZ)
        {
            lock (validBitsLock)
            {
                if (chunkX >= ChunkXMin && chunkX <= ChunkXMax && chunkZ <= ChunkZMax && chunkZ >= ChunkZMin)
                {

                    int xIndex = chunkX - ChunkXMin + xStartIndex;
                    if (xIndex >= Cache_Size)
                        xIndex -= Cache_Size;

                    int zIndex = chunkZ - ChunkZMin + zStartIndex;
                    if (zIndex >= Cache_Size)
                        zIndex -= Cache_Size;

                    return cacheIsValid[xIndex, zIndex];
                }
            }

            Request(chunkX, chunkZ);
            return false;
        }

        public Chunk GetChunk(int chunkX, int chunkZ)
        {
            lock (validBitsLock)
            {
                int xIndex = chunkX - ChunkXMin + xStartIndex;

                if (xIndex >= Cache_Size)
                    xIndex -= Cache_Size;

                int zIndex = chunkZ - ChunkZMin + zStartIndex;

                if (zIndex >= Cache_Size)
                    zIndex -= Cache_Size;

                if (!cacheIsValid[xIndex, zIndex])
                    throw new ArgumentOutOfRangeException("Chunk is not yet loaded.");

                return cachedChunks[xIndex, zIndex];
            }
        }

        #region Cache Moving
        /// <summary>
        /// This makes sure the desired chunk is within the cache bounds,
        /// but does not create any new cached chunks right now.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        public void Request(int chunkX, int chunkZ)
        {
            lock (validBitsLock)
            {
                while (chunkZ < ChunkZMin)
                    addTopRow();

                while (chunkZ > ChunkZMax)
                    addBottomRow();

                while (chunkX < ChunkXMin)
                    addLeftColumn();

                while (chunkX > ChunkXMax)
                    addRightColumn();
            }
        }

        private void addTopRow()
        {
            //allows for a new top row
            ChunkZMin--;

            //fixes the indexing so that the old data is still indexed correctly
            zStartIndex = Numerical.IntMod(zStartIndex - 1, Cache_Size);

            //now fix the new top row
            for (int x = 0; x < Cache_Size; x++)
            {
                cacheIsValid[x, zStartIndex] = false;
            }
        }

        private void addBottomRow()
        {
            //set up the new bottom row, which replaces the old top row
            for (int x = 0; x < Cache_Size; x++)
            {
                cacheIsValid[x, zStartIndex] = false;
            }

            //updates the minimum
            ChunkZMin++;

            //fixes the indexing
            zStartIndex = Numerical.IntMod(zStartIndex + 1, Cache_Size);
        }

        private void addLeftColumn()
        {
            //allows for the next left column
            ChunkXMin--;

            //fixes the indexing
            xStartIndex = Numerical.IntMod(xStartIndex - 1, Cache_Size);

            //fix the new left column
            for (int z = 0; z < Cache_Size; z++)
            {
                cacheIsValid[xStartIndex, z] = false;
            }
        }

        private void addRightColumn()
        {
            //set up the new right column, which replaces the old left column
            for (int z = 0; z < Cache_Size; z++)
            {
                cacheIsValid[xStartIndex, z] = false;
            }

            //updates the minimum
            ChunkXMin++;

            //fixes the indexing
            xStartIndex = Numerical.IntMod(xStartIndex + 1, Cache_Size);
        }
        #endregion

        /// <summary>
        /// This adds a chunk to the grid immediately, without waiting
        /// for the backup thread to take care of it.  If the specified
        /// coordinates are outside the ideal cache range, it will either
        /// drop the request (if forceSave is false) or move the cache
        /// around in order to force it to be contained.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        public void AddChunk(int chunkX, int chunkZ)
        {
            Request(chunkX, chunkZ);

            lock (validBitsLock)
            {
                if (chunkX < ChunkXMin || chunkX > ChunkXMax || chunkZ < ChunkZMin || chunkZ > ChunkZMax)
                    return;

                int xIndex = Numerical.IntMod(chunkX - ChunkXMin + xStartIndex, Cache_Size);
                int zIndex = Numerical.IntMod(chunkZ - ChunkZMin + zStartIndex, Cache_Size);

                cacheIsValid[xIndex, zIndex] = true;

                if (cachedChunks[xIndex, zIndex] == null)
                    cachedChunks[xIndex, zIndex] = new Chunk(map.MakeChunkBlocks(chunkX, chunkZ), map.BlockHandler);
                else
                    cachedChunks[xIndex, zIndex].OverwriteWith(map.MakeChunkBlocks(chunkX, chunkZ));
            }
        }
    }
}

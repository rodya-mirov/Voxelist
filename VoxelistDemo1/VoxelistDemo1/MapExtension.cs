using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Mapping;
using Voxelist.Utilities;
using Voxelist.BlockHandling;
using Voxelist.Entities;

namespace VoxelistDemo1
{
    public class MapExtension : Map
    {
        public MapExtension(BlockHandlerExtension handler)
            : base(handler)
        {
        }

        protected override int ChunkViewDistance
        {
            get { return 7; }
        }

        public override int EntitySpawnRadius
        {
            get { return 4; }
        }

        public override void MakeChunkData(int chunkX, int chunkZ, Block[, ,] arrayToFill, List<EntitySchema> entityDataToFill)
        {
            addEntityData(entityDataToFill);
            addBlockData(chunkX, chunkZ, arrayToFill);
        }

        private static void addBlockData(int chunkX, int chunkZ, Block[, ,] arrayToFill)
        {
            for (int xIndex = -1; xIndex <= GameConstants.CHUNK_X_WIDTH; xIndex++)
            {
                for (int y = 0; y < GameConstants.CHUNK_Y_HEIGHT; y++)
                {
                    for (int zIndex = -1; zIndex <= GameConstants.CHUNK_Z_LENGTH; zIndex++)
                    {
                        int currentChunkX = chunkX;
                        int currentChunkZ = chunkZ;

                        int x = xIndex;
                        int z = zIndex;

                        if (x == -1)
                        {
                            x += GameConstants.CHUNK_X_WIDTH;
                            currentChunkX--;
                        }
                        else if (x == GameConstants.CHUNK_X_WIDTH)
                        {
                            x -= GameConstants.CHUNK_X_WIDTH;
                            currentChunkX++;
                        }

                        if (z == -1)
                        {
                            z += GameConstants.CHUNK_Z_LENGTH;
                            currentChunkZ--;
                        }
                        else if (z == GameConstants.CHUNK_Z_LENGTH)
                        {
                            z -= GameConstants.CHUNK_Z_LENGTH;
                            currentChunkZ++;
                        }

                        Block block;

                        //ground everywhere, and pyramid mountains
                        //that are right on the corners of the chunks

                        if (y == 0) //uniform ground everywhere, but sometimes ice
                        {
                            if ((currentChunkX & 1) == 0 || (currentChunkZ & 1) == 0)
                                block = new Block(1);
                            else
                                block = new Block(2);
                        }
                        else if (y == 10 && x == 5 && z == 5)
                        {
                            block = new Block(1);
                        }
                        else if ((currentChunkX & 1) == 0 && (currentChunkZ & 1) == 0) //even/even chunk coords, etc.
                        {
                            if (z >= x + y + 5)
                                block = new Block(1);
                            else
                                block = new Block(0);
                        }
                        else if ((currentChunkX & 1) == 1 && (currentChunkZ & 1) == 0)
                        {
                            if (z >= (GameConstants.CHUNK_X_WIDTH - x) + y + 5)
                                block = new Block(1);
                            else
                                block = new Block(0);
                        }
                        else if ((currentChunkX & 1) == 0 && (currentChunkZ & 1) == 1)
                        {
                            if ((GameConstants.CHUNK_Z_LENGTH - z - 2) >= x + y + 5)
                                block = new Block(1);
                            else
                                block = new Block(0);
                        }
                        else
                        {
                            if ((GameConstants.CHUNK_Z_LENGTH - z - 3) >= (GameConstants.CHUNK_X_WIDTH - x - 1) + y + 5)
                                block = new Block(1);
                            else
                                block = new Block(0);
                        }

                        arrayToFill[xIndex + 1, y, zIndex + 1] = block;
                    }
                }
            }
        }

        private static void addEntityData(List<EntitySchema> entityDataToFill)
        {
            //one scootblock per chunk
            entityDataToFill.Add(new EntitySchema(0, 4, 40, 4));

            //4 scenery blocks per chunk
            /*
            int inc = 8;
            for (int x = 0; x < GameConstants.CHUNK_X_WIDTH; x += inc)
            {
                for (int z = 0; z < GameConstants.CHUNK_Z_LENGTH; z += inc)
                {
                    entityDataToFill.Add(new EntitySchema(1, x, 100, z));
                }
            }//*/
        }
    }
}

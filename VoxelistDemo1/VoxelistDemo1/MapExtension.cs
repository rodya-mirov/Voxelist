﻿using System;
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
        public MapExtension()
            : base(0, 0)
        {
        }

        public override int ViewRadius
        {
            get { return 9; }
        }

        public override int EntitySpawnRadius
        {
            get { return 5; }
        }

        public override void MakeChunkData(int chunkX, int chunkZ, Block[, ,] arrayToFill, List<EntitySchema> entityDataToFill)
        {
            addEntityData(entityDataToFill);
            addBlockData(chunkX, chunkZ, arrayToFill);
        }

        private static void addBlockData(int chunkX, int chunkZ, Block[, ,] arrayToFill)
        {
            for (int x = 0; x < GameConstants.CHUNK_X_WIDTH; x++)
            {
                for (int y = 0; y < GameConstants.CHUNK_Y_HEIGHT; y++)
                {
                    for (int z = 0; z < GameConstants.CHUNK_Z_LENGTH; z++)
                    {
                        Block block;

                        //ground everywhere, and pyramid mountains
                        //that are right on the corners of the chunks

                        if (y == 0) //uniform ground everywhere, but sometimes ice
                        {
                            if ((chunkX & 1) == 0 || (chunkZ & 1) == 0)
                                block = new Block(1);
                            else
                                block = new Block(2);
                        }
                        else if (y == 10 && x == 5 && z == 5)
                        {
                            block = new Block(1);
                        }
                        else if ((chunkX & 1) == 0 && (chunkZ & 1) == 0) //even/even chunk coords, etc.
                        {
                            if (z >= x + y + 5)
                                block = new Block(1);
                            else
                                block = new Block(0);
                        }
                        else if ((chunkX & 1) == 1 && (chunkZ & 1) == 0)
                        {
                            if (z >= (GameConstants.CHUNK_X_WIDTH - x) + y + 5)
                                block = new Block(1);
                            else
                                block = new Block(0);
                        }
                        else if ((chunkX & 1) == 0 && (chunkZ & 1) == 1)
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

                        arrayToFill[x, y, z] = block;
                    }
                }
            }
        }

        private static void addEntityData(List<EntitySchema> entityDataToFill)
        {
            entityDataToFill.Clear();

            //one scootblock per chunk
            entityDataToFill.Add(new EntitySchema(0, 4, 40, 4));

            //4 scenery blocks per chunk
            int inc = 8;
            for (int x = inc / 2; x < GameConstants.CHUNK_X_WIDTH; x += inc)
            {
                for (int z = inc / 2; z < GameConstants.CHUNK_Z_LENGTH; z += inc)
                {
                    entityDataToFill.Add(new EntitySchema(1, x, 100, z));
                }
            }
        }
    }
}

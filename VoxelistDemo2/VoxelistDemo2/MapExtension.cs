using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Mapping;
using Voxelist.BlockHandling;
using Voxelist.Utilities;
using Voxelist.Entities;

namespace VoxelistDemo2
{
    public class MapExtension : Map
    {
        public MapExtension(BlockHandlerExtension handler)
            : base(handler, 0, 0)
        {
        }

        protected override int ChunkViewDistance
        {
            get { return 6; }
        }

        public override void MakeChunkData(int chunkX, int chunkZ, Block[, ,] arrayToFill, List<EntitySchema> entityDataToFill)
        {
            for (int x = -1; x <= GameConstants.CHUNK_X_WIDTH; x++)
            {
                for (int z = -1; z <= GameConstants.CHUNK_Z_LENGTH; z++)
                {
                    int height = FindHeight(chunkX * GameConstants.CHUNK_X_WIDTH + x, chunkZ * GameConstants.CHUNK_Z_LENGTH + z);

                    for (int y = 0; y < GameConstants.CHUNK_Y_HEIGHT; y++)
                    {
                        Block block;

                        if (y <= height)
                            block = new Block(1);
                        else
                            block = new Block(0);

                        arrayToFill[x + 1, y, z + 1] = block;
                    }
                }
            }
        }

        private int FindHeight(int xCoordinate, int zCoordinate)
        {
            return ValueNoise(xCoordinate, zCoordinate, 8, 0.25f);
        }

        private int ValueNoise(int x, int y, int numOctaves, double scale)
        {
            double output = RandomHelper.randomDouble(RandomHelper.combineToSingleSeed(x, y, true), 0);

            for (int octave = 0; octave < numOctaves; octave++)
            {
                int leftX = (x >> octave) << octave;
                int rightX = leftX + (1 << octave);

                int topY = (y >> octave) << octave;
                int bottomY = topY + (1 << octave);

                double LT = RandomHelper.randomDouble(RandomHelper.combineToSingleSeed(leftX, topY, true), octave + 1);
                double LB = RandomHelper.randomDouble(RandomHelper.combineToSingleSeed(leftX, bottomY, true), octave + 1);

                double RT = RandomHelper.randomDouble(RandomHelper.combineToSingleSeed(rightX, topY, true), octave + 1);
                double RB = RandomHelper.randomDouble(RandomHelper.combineToSingleSeed(rightX, bottomY, true), octave + 1);

                double left = (LT * (bottomY - y) + LB * (y - topY)) / (bottomY - topY);
                double right = (RT * (bottomY - y) + RB * (y - topY)) / (bottomY - topY);

                double scaledMid = left * (rightX - x) + right * (x - leftX);

                output += scaledMid;
            }

            return (int)(output * scale);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Mapping;
using Voxelist.BlockHandling;
using Voxelist.Utilities;

namespace VoxelistDemo2
{
    public class MapExtension : Map
    {
        public MapExtension(BlockHandlerExtension handler)
            : base(handler)
        {
        }

        protected override int ChunkViewDistance
        {
            get { return 6; }
        }

        protected override int CacheRadius
        {
            get { return ChunkViewDistance; }
        }

        public override void MakeChunkBlocks(int chunkX, int chunkZ, Block[, ,] arrayToFill)
        {
            for (int x = -1; x <= GameConstants.CHUNK_X_WIDTH; x++)
            {
                for (int z = -1; z <= GameConstants.CHUNK_Z_LENGTH; z++)
                {
                    int height = FindHeight(chunkX * GameConstants.CHUNK_X_WIDTH + x, chunkZ * GameConstants.CHUNK_Z_LENGTH + z);

                    int fullSquares = Numerical.IntDivide(height, BlockHandlerExtension.GRANULARITY);
                    int leftover = Numerical.IntMod(height, BlockHandlerExtension.GRANULARITY);

                    if (fullSquares > GameConstants.CHUNK_Y_HEIGHT - 1)
                        fullSquares = GameConstants.CHUNK_Y_HEIGHT - 1;

                    if (fullSquares <= 0)
                        throw new ArgumentException();

                    for (int y = 0; y < GameConstants.CHUNK_Y_HEIGHT; y++)
                    {
                        Block block;

                        if (y < fullSquares)
                            block = new Block(BlockHandlerExtension.GRANULARITY);
                        else if (y == fullSquares)
                            block = new Block(leftover);
                        else
                            block = new Block(0);

                        arrayToFill[x + 1, y, z + 1] = block;
                    }
                }
            }
        }

        private int FindHeight(int xCoordinate, int zCoordinate)
        {
            return BlockHandlerExtension.GRANULARITY + ValueNoise(xCoordinate, zCoordinate, 8, BlockHandlerExtension.GRANULARITY / 4.0f);
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

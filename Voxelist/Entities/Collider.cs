using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.BlockHandling;
using Voxelist.Utilities;

namespace Voxelist.Entities
{
    public struct Collider
    {
        public BoundingBox GetRelativeBoundingBox(int myChunkX, int myChunkZ)
        {
            Vector3 translation = new Vector3(
                GameConstants.CHUNK_X_WIDTH * (colliderChunkX - myChunkX),
                0,
                GameConstants.CHUNK_Z_LENGTH * (colliderChunkZ - myChunkZ)
                );

            return new BoundingBox(
                StartingBoundingBox.Min + translation,
                StartingBoundingBox.Max + translation
                );
        }

        public BoundingBox StartingBoundingBox;

        public float Friction;
        public Vector3 FrictionVelocity;

        public int colliderChunkX, colliderChunkZ;

        public Object collidedObject;

        /// <summary>
        /// Construct a new collider object for a block.  Note the specified coordinates are for
        /// the block itself (describing its position in space) and not for the thing it's colliding with.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="chunkX"></param>
        /// <param name="blockX"></param>
        /// <param name="chunkZ"></param>
        /// <param name="blockZ"></param>
        /// <param name="handler"></param>
        public Collider(Block block, int chunkX, int chunkZ, int blockX, int blockY, int blockZ)
        {
            this.collidedObject = block;

            this.colliderChunkX = chunkX;
            this.colliderChunkZ = chunkZ;

            this.StartingBoundingBox = BlockHandler.PhysicalBlockingBox(block);
            Vector3 translation = new Vector3(blockX, blockY, blockZ);

            this.StartingBoundingBox = new BoundingBox(
                StartingBoundingBox.Min + translation, StartingBoundingBox.Max + translation);

            this.Friction = BlockHandler.Friction(block);
            this.FrictionVelocity = BlockHandler.FrictionVelocity(block);
        }

        public Collider(Entity other)
        {
            this.collidedObject = other;

            this.colliderChunkX = other.Position.chunkX;
            this.colliderChunkZ = other.Position.chunkZ;

            this.StartingBoundingBox = other.BoundingBox;

            this.Friction = other.Friction_Induced;
            this.FrictionVelocity = other.FrictionVelocity_Induced;
        }
    }
}

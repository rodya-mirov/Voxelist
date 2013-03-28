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
        public BoundingBox BoundingBox;

        public float Friction;
        public Vector3 FrictionVelocity;

        public Object collidedObject;

        public Collider(Block block, Vector3 translation, BlockHandler handler)
        {
            this.collidedObject = block;

            this.BoundingBox = handler.PhysicalBlockingBox(block);
            this.BoundingBox = new BoundingBox(BoundingBox.Min + translation, BoundingBox.Max + translation);

            this.Friction = handler.Friction(block);
            this.FrictionVelocity = handler.FrictionVelocity(block);
        }

        public Collider(Entity other, int myChunkX, int myChunkZ)
        {
            this.collidedObject = other;

            Vector3 translation = new Vector3(
                GameConstants.CHUNK_X_WIDTH * (other.Position.chunkX - myChunkX),
                0,
                GameConstants.CHUNK_Z_LENGTH * (other.Position.chunkZ - myChunkZ));

            this.BoundingBox = other.BoundingBox;
            this.BoundingBox = new BoundingBox(
                this.BoundingBox.Min + translation,
                this.BoundingBox.Max + translation);

            this.Friction = other.Friction_Induced;
            this.FrictionVelocity = other.FrictionVelocity_Induced;
        }
    }
}

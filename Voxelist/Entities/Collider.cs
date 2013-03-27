using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.BlockHandling;

namespace Voxelist.Entities
{
    public struct Collider
    {
        public BoundingBox BoundingBox;

        public float Friction;
        public Vector3 FrictionVelocity;

        public Collider(Block block, Vector3 translation, BlockHandler handler)
        {
            this.BoundingBox = handler.PhysicalBlockingBox(block);
            this.BoundingBox = new BoundingBox(BoundingBox.Min + translation, BoundingBox.Max + translation);

            this.Friction = handler.Friction(block);
            this.FrictionVelocity = handler.FrictionVelocity(block);
        }
    }
}

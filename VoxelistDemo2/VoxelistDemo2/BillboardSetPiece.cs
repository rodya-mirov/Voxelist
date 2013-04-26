using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.GeometryPrimitives;
using Voxelist.Utilities;
using Voxelist.Rendering;

namespace VoxelistDemo2
{
    public class BillboardSetPiece : Entity
    {
        public BillboardSetPiece(WorldPosition position, WorldManager manager)
            : base(position, manager)
        {
        }

        private static Texture2D blobTexture;
        private static GeometryPrimitive[] billboardPrimitives;

        private const float billboardWidth = .9f;
        private const float billboardHeight = .9f;

        private static Vector3 size = Vector3.One * .9f;

        public static void LoadContent(Game game)
        {
            blobTexture = game.Content.Load<Texture2D>("Textures/SetPieces/GreenBlob");

            billboardPrimitives = new GeometryPrimitive[1];
            billboardPrimitives[0] = GeometryPrimitive.MakeRectangle(
                Vector3.Zero, Vector3.Forward, Vector3.Up,
                Vector2.Zero, Vector2.One,
                billboardWidth, billboardHeight);
        }

        #region Physics
        public override bool CollidesWithMapGeometry
        {
            get { return true; }
        }

        private bool IsStopped()
        {
            if (Velocity.X != 0 || Velocity.Z != 0)
                return false;

            return (Velocity.Y <= 0 && YCollidedDown);
        }

        private static double fallingWaitTime = .2; //how long to wait between attempts to fall
        private static double timeSinceLastCheck = fallingWaitTime;

        public override void Update(GameTime gametime)
        {
            if (timeSinceLastCheck >= fallingWaitTime)
            {
                base.physicsUpdate(gametime);

                if (IsStopped())
                {
                    timeSinceLastCheck = 0;
                }
            }
            else
            {
                timeSinceLastCheck += gametime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override bool HasPhysicsInteractions
        {
            get { return false; }
        }

        public override float Friction_Induced
        {
            get { return 0; }
        }

        protected override Vector3 GroundIntendedVelocity
        {
            get { return Vector3.Zero; }
        }

        public override BoundingBox BoundingBox
        {
            get { return new BoundingBox(Position.InChunkPosition, Position.InChunkPosition + size); }
        }
        #endregion

        #region Rendering
        public override bool IsVisible
        {
            get { return true; }
        }

        public override BoundingBox VisualBoundingBox
        {
            get { return BoundingBox; }
        }

        public override Vector3 DrawingOffset
        {
            get { return size / 2; }
        }

        public override Texture2D DrawableTexture
        {
            get { return blobTexture; }
        }

        public override DrawType DrawingType
        {
            get { return DrawType.Billboards; }
        }

        public override GeometryPrimitive[] BillboardGeometries
        {
            get { return billboardPrimitives; }
        }
        #endregion
    }
}

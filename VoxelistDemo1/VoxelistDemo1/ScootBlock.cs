﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Entities;
using Voxelist.Mapping;
using Voxelist.Rendering;
using Voxelist.GeometryPrimitives;
using Voxelist.Utilities;

namespace VoxelistDemo1
{
    public class ScootBlock : Entity
    {
        public ScootBlock(WorldPosition position, WorldManager manager)
            : base(position, manager)
        {
            intendedVelocity = new Vector3(1.5f, 0, 0);
        }

        private static GeometryPrimitive entityPrimitive;
        private static Texture2D texture;

        private const float buffer = 0.25f;
        private const float size = 1.0f - 2.0f * buffer;

        public static void LoadContent(Game game)
        {
            entityPrimitive = GeometryPrimitive.Make3DRectangle(
                Vector3.One * buffer, Vector3.One * size,
                Vector2.Zero, new Vector2(1.0f, 1.0f),
                true, true, true, true, true, true);

            texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");
        }

        public override BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(
                    Position.InChunkPosition + buffer * Vector3.One,
                    Position.InChunkPosition + (buffer + size) * Vector3.One);
            }
        }

        public override BoundingBox VisualBoundingBox
        {
            get { return BoundingBox; }
        }

        public override bool IsVisible
        {
            get { return true; }
        }

        public override float Friction_Induced
        {
            get { return 20; }
        }

        private Vector3 intendedVelocity;
        protected override Vector3 GroundIntendedVelocity
        {
            get { return intendedVelocity; }
        }

        public override void Update(GameTime gametime)
        {
            base.physicsUpdate(gametime);
        }

        public override GeometryPrimitive DrawableGeometryPrimitive
        {
            get { return entityPrimitive; }
        }

        public override Texture2D DrawableTexture
        {
            get { return texture; }
        }

        public override Entity.DrawType DrawingType
        {
            get { return DrawType.GeometryPrimitive; }
        }

        public override Vector3 DrawingOffset
        {
            get { return Vector3.Zero; }
        }
    }
}

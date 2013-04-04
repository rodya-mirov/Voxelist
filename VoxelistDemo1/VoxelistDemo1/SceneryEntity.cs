using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Entities;
using Voxelist.Mapping;
using Voxelist.GeometryPrimitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Rendering;
using Voxelist.Utilities;

namespace VoxelistDemo1
{
    public class SceneryEntity : Entity
    {
        public SceneryEntity(WorldPosition pos, WorldManager manager)
            : base(pos, manager)
        {
        }

        private static GeometryPrimitive entityPrimitive;
        private static int numVertices, numTriangles;

        private static BasicEffect drawingEffect;

        private const float buffer = 0.25f;
        private const float size = 1.0f - 2.0f * buffer;

        public static void LoadContent(Game game)
        {
            entityPrimitive = GeometryPrimitive.Make3DRectangle(
                Vector3.One * buffer, Vector3.One * size,
                Vector2.Zero, new Vector2(1.0f, 1.0f),
                true, true, true, true, true, true);
            numVertices = 24;
            numTriangles = 12;

            drawingEffect = new BasicEffect(game.GraphicsDevice);

            drawingEffect.TextureEnabled = true;
            drawingEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");

            //Turns them blackish and shiny, makes em stand out
            drawingEffect.DiffuseColor = Color.AntiqueWhite.ToVector3();
            drawingEffect.EnableDefaultLighting();
        }

        protected override Vector3 GroundIntendedVelocity
        {
            get { return Vector3.Zero; }
        }

        public override float Friction_Induced
        {
            get { return 50; } //whatever
        }

        //collides with walls, but ...
        public override bool CollidesWithMapGeometry
        {
            get { return true; }
        }

        //...not with other Entities
        public override bool HasPhysicsInteractions
        {
            get { return false; }
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

        public override void Update(GameTime gametime)
        {
            base.physicsUpdate(gametime);
        }

        public override void Draw(GameTime gametime)
        {
            drawingEffect.World = Matrix.CreateTranslation(Camera.objectTranslation(Position));

            drawingEffect.View = Camera.ViewMatrix;
            drawingEffect.Projection = Camera.ProjectionMatrix;

            foreach (EffectPass pass in drawingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                drawingEffect.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    entityPrimitive.Vertices,
                    0, numVertices,
                    entityPrimitive.Indices,
                    0, numTriangles
                    );
            }
        }
    }
}

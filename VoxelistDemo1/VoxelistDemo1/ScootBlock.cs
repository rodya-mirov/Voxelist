using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Entities;
using Voxelist.Mapping;
using Voxelist.Rendering;
using Voxelist.GeometryPrimitives;

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
        private static int numVertices, numTriangles;

        private static BasicEffect drawingEffect;

        private const float buffer = 0.25f;
        private const float size = 1.0f - 2.0f * buffer;

        public static void LoadContent(Game game)
        {
            entityPrimitive = GeometryPrimitive.Make3DRectangle(Vector3.One * buffer, Vector3.One * size, Vector2.Zero, new Vector2(1.0f, 1.0f));
            numVertices = 24;
            numTriangles = 12;

            drawingEffect = new BasicEffect(game.GraphicsDevice);

            drawingEffect.TextureEnabled = true;
            drawingEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");

            //Turns them blackish and shiny, makes em stand out
            drawingEffect.DiffuseColor = Color.BlueViolet.ToVector3();
            drawingEffect.EnableDefaultLighting();
        }

        public override BoundingBox BoundingBox
        {
            get { return new BoundingBox(Position.InChunkPosition + buffer * Vector3.One, Position.InChunkPosition + (buffer + size) * Vector3.One); }
        }

        private Vector3 intendedVelocity;
        protected override Vector3 Intentional_Velocity
        {
            get
            {
                if (YCollidedDown)
                    return intendedVelocity;
                else
                    return Vector3.Zero;
            }
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

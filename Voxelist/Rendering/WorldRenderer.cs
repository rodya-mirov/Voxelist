using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Mapping;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Entities;
using Voxelist.BlockHandling;
using Voxelist.GeometryPrimitives;

namespace Voxelist.Rendering
{
    public class WorldRenderer : DrawableGameComponent
    {
        private WorldManager WorldManager { get; set; }

        private Map Map { get { return WorldManager.Map; } }

        public WorldRenderer(Game game, WorldManager manager)
            : base(game)
        {
            WorldManager = manager;
        }

        public override void Initialize()
        {
            base.Initialize();

            WorldManager.Initialize();
        }

        protected virtual VoxelistEffectWrapper MakeEffectWrapper()
        {
            BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.EnableDefaultLighting();

            return new BasicEffectWrapper(basicEffect);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            drawingEffectWrapper = MakeEffectWrapper();
            WorldManager.LoadContent(Game);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            WorldManager.Update(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            WorldManager.Dispose(disposing);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawScene(gameTime);
        }

        protected VoxelistEffectWrapper drawingEffectWrapper;

        private void DrawScene(GameTime gameTime)
        {
            drawingEffectWrapper.Projection = Camera.ProjectionMatrix;
            drawingEffectWrapper.View = Camera.ViewMatrix;

            drawMap();
            drawEntities();
        }

        private void drawEntities()
        {
            Effect drawingEffect = drawingEffectWrapper.Effect;

            foreach (Entity entity in WorldManager.Entities())
            {
                if (!entity.ShouldBeDrawn())
                    continue;

                drawingEffectWrapper.Texture = entity.DrawableTexture;

                switch (entity.DrawingType)
                {
                    case Entity.DrawType.GeometryPrimitive:

                        drawingEffectWrapper.World = Matrix.CreateTranslation(Camera.objectTranslation(entity.Position) + entity.DrawingOffset);
                        entity.DrawableGeometryPrimitive.Draw(drawingEffectWrapper.Effect);

                        break;

                    case Entity.DrawType.Billboards:

                        drawingEffectWrapper.World = Camera.MakeBillboard(entity);
                        GeometryPrimitive[] billboards = entity.BillboardGeometries;

                        for (int i = 0; i < billboards.Length; i++)
                            billboards[i].Draw(drawingEffectWrapper.Effect);

                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void drawMap()
        {
            Effect drawingEffect = drawingEffectWrapper.Effect;

            for (int textureIndex = 0; textureIndex < BlockHandler.TotalNumberOfTextures; textureIndex++)
            {
                drawingEffectWrapper.Texture = BlockHandler.Texture(textureIndex);

                foreach (Tuple<Vector3, GeometryPrimitive> offsetChunkPrimitive in Map.ChunksToDraw(textureIndex))
                {
                    drawingEffectWrapper.World = Matrix.CreateTranslation(offsetChunkPrimitive.Item1);
                    GeometryPrimitive primitive = offsetChunkPrimitive.Item2;

                    if (primitive.Vertices == null || primitive.Indices == null)
                        continue;

                    foreach (EffectPass pass in drawingEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        drawingEffect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                            primitive.Vertices, 0, primitive.Vertices.Length,
                            primitive.Indices, 0, primitive.Indices.Length / 3);
                    }
                }
            }
        }
    }
}

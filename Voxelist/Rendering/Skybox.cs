using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.GeometryPrimitives;

namespace Voxelist.Rendering
{
    public class Skybox
    {
        public Skybox()
        {
        }

        private GeometryPrimitive box;
        private BasicEffect drawEffect;

        public void LoadContent(Game game, String textureLocation, float distanceAway = 500.0f)
        {
            box = GeometryPrimitive.MakeSkybox(-distanceAway * Vector3.One, distanceAway * 2.0f * Vector3.One, Vector2.Zero, Vector2.One);
            drawEffect = new BasicEffect(game.GraphicsDevice);

            drawEffect.TextureEnabled = true;
            drawEffect.Texture = game.Content.Load<Texture2D>(textureLocation);
        }

        public void Draw()
        {
            drawEffect.World = Matrix.CreateTranslation(Camera.InChunkPosition);

            drawEffect.View = Camera.ViewMatrix;
            drawEffect.Projection = Camera.ProjectionMatrix;

            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                drawEffect.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    box.Vertices, 0, 24,
                    box.Indices, 0, 12
                    );
            }
        }
    }
}

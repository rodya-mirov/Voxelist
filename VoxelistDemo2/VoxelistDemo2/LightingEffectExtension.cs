using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Rendering;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VoxelistDemo2
{
    public class LightingEffectExtension : VoxelistEffectWrapper
    {
        public LightingEffectExtension(Effect baseEffect)
            : base(baseEffect)
        {
            world = Matrix.Identity;
            view = Matrix.Identity;
            projection = Matrix.Identity;

            updateMatrices();
        }

        private Matrix world, view, projection, worldViewProjection, worldInverseTranspose;

        private void updateMatrices()
        {
            worldViewProjection = world * view * projection;
            worldInverseTranspose = Matrix.Transpose(Matrix.Invert(world));

            Effect.Parameters["WVP"].SetValue(worldViewProjection);
            Effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
        }

        public override Matrix Projection
        {
            set
            {
                projection = value;
                updateMatrices();
            }
        }

        public override Matrix View
        {
            set
            {
                view = value;
                updateMatrices();
            }
        }

        public override Matrix World
        {
            set
            {
                world = value;
                updateMatrices();
            }
        }

        public override Texture2D Texture
        {
            set { Effect.Parameters["EffectTexture"].SetValue(value); }
        }
    }
}

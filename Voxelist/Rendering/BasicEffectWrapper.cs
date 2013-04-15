using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Voxelist.Rendering
{
    internal class BasicEffectWrapper : VoxelistEffectWrapper
    {
        private BasicEffect BasicEffect { get { return (BasicEffect)Effect; } }

        public BasicEffectWrapper(BasicEffect effect)
            : base(effect)
        {
        }

        public override Matrix Projection
        {
            set { BasicEffect.Projection = value; }
        }

        public override Matrix View
        {
            set { BasicEffect.View = value; }
        }

        public override Matrix World
        {
            set { BasicEffect.World = value; }
        }

        public override Texture2D Texture
        {
            set { BasicEffect.Texture = value; }
        }
    }
}

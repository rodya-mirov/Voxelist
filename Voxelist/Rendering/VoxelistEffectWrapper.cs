using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Voxelist.Rendering
{
    public abstract class VoxelistEffectWrapper
    {
        public VoxelistEffectWrapper(Effect baseEffect)
        {
            Effect = baseEffect;
        }

        public Effect Effect { get; private set; }

        public abstract Matrix World { set; }
        public abstract Matrix View { set; }
        public abstract Matrix Projection { set; }

        public abstract Texture2D Texture { set; }
    }
}

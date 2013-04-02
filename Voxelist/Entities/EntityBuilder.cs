using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Rendering;

namespace Voxelist.Entities
{
    public abstract class EntityBuilder
    {
        public EntityBuilder()
        {
        }

        public virtual void LoadContent(Game game)
        {
        }

        public abstract Entity MakeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager);
    }
}

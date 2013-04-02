using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Entities;
using Microsoft.Xna.Framework;
using Voxelist.Rendering;

namespace VoxelistDemo2
{
    public class EntityBuilderExtension : EntityBuilder
    {
        public EntityBuilderExtension()
            : base()
        {
        }

        public override void LoadContent(Game game)
        {
            base.LoadContent(game);
        }

        public override Entity MakeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager)
        {
            throw new NotImplementedException();
        }
    }
}

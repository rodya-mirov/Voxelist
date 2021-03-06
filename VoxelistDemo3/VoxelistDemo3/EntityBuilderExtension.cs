﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Entities;
using Microsoft.Xna.Framework;
using Voxelist.Rendering;

namespace VoxelistDemo3
{
    public class EntityBuilderExtension : EntityBuilder
    {
        public EntityBuilderExtension()
            : base()
        {
        }

        protected override void loadContent(Game game)
        {
            base.loadContent(game);
        }

        protected override Entity makeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager)
        {
            throw new NotImplementedException();
        }
    }
}

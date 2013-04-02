using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Entities;
using Microsoft.Xna.Framework;
using Voxelist.Mapping;
using Voxelist.Rendering;

namespace VoxelistDemo1
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
            if (schema.TypeID != 0)
                throw new NotImplementedException();

            WorldPosition position = new WorldPosition(
                chunkX, chunkZ, schema.inChunkX, schema.inChunkY, schema.inChunkZ);

            return new ScootBlock(position, manager);
        }
    }
}

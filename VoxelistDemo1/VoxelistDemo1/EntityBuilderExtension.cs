using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Entities;
using Microsoft.Xna.Framework;
using Voxelist.Mapping;
using Voxelist.Rendering;
using Voxelist.Utilities;

namespace VoxelistDemo1
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

            ScootBlock.LoadContent(game);
            SceneryEntity.LoadContent(game);
        }

        protected override Entity makeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager)
        {
            WorldPosition position = new WorldPosition(
                chunkX, chunkZ, schema.inChunkX, schema.inChunkY, schema.inChunkZ);

            switch (schema.TypeID)
            {
                case 0:
                    return new ScootBlock(position, manager);

                case 1:
                    return new SceneryEntity(position, manager);

                default: throw new NotImplementedException();
            }
        }
    }
}

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

        protected override void loadContent(Game game)
        {
            base.loadContent(game);

            BillboardSetPiece.LoadContent(game);
        }

        protected override Entity makeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager)
        {
            Entity output = new BillboardSetPiece(
                new Voxelist.Utilities.WorldPosition(chunkX, chunkZ, schema.inChunkX, schema.inChunkY, schema.inChunkZ),
                manager);

            output.PutOnGround();

            return output;
        }
    }
}

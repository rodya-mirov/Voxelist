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
        internal static EntityBuilder instance;

        protected EntityBuilder()
        {
            if (instance != null)
                throw new InvalidProgramException("Can't instantiate two EntityBuilders!");

            instance = this;
        }

        protected Game Game { get; private set; }

        protected virtual void loadContent(Game game)
        {
            this.Game = game;
        }

        public static void LoadContent(Game game) { instance.loadContent(game); }

        protected abstract Entity makeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager);

        public static Entity MakeEntity(EntitySchema schema, int chunkX, int chunkZ, WorldManager manager)
        {
            return instance.makeEntity(schema, chunkX, chunkZ, manager);
        }
    }
}

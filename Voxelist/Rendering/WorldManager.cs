using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Mapping;
using Voxelist.BlockHandling;
using Voxelist.Entities;

namespace Voxelist.Rendering
{
    public abstract class WorldManager : DrawableGameComponent
    {
        public Map Map { get; protected set; }
        public BlockHandler BlockHandler { get; protected set; }
        protected Skybox Skybox { get; set; }

        public WorldManager(Game game, Map map, BlockHandler handler)
            : base(game)
        {
            this.Map = map;
            this.BlockHandler = handler;
            this.Skybox = null;
        }

        public abstract IEnumerable<Entity> Entities();

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Map.LoadContent(Game);
            BlockHandler.LoadContent(Game);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (Entity entity in Entities())
                entity.Update(gameTime);

            Camera.Update();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Map.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Map.Draw();

            foreach (Entity entity in Entities())
                entity.Draw(gameTime);

            if (Skybox != null)
                Skybox.Draw();
        }
    }
}

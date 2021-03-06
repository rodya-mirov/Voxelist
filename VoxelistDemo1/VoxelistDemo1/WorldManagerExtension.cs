﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Rendering;
using Voxelist.Entities;
using Voxelist.Mapping;
using Microsoft.Xna.Framework.Input;
using Voxelist.Utilities;

namespace VoxelistDemo1
{
    public class WorldManagerExtension : WorldManager
    {
        public WorldManagerExtension(MapExtension map)
            : base(map)
        {
        }

        public PlayerAvatar Avatar { get; private set; }

        protected override IEnumerable<Entity> ManualEntities()
        {
            yield return Avatar;
        }

        public override void LoadContent(Game game)
        {
            base.LoadContent(game);

            Avatar = new PlayerAvatar(new WorldPosition(0, 0, 0, 3, 0), this);
            Camera.StartFollowing(Avatar);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.P))
            {
                int count = 0;
                foreach (Entity e in Entities())
                    count++;

                if (true)
                {
                }
            }
        }
    }
}

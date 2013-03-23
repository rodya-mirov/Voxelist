﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Rendering;
using Voxelist.Entities;
using Voxelist.Mapping;

namespace VoxelistDemo1
{
    public class WorldManagerExtension : WorldManager
    {
        public WorldManagerExtension(Game game, MapExtension map, BlockHandlerExtension handler)
            : base(game, map, handler)
        {
        }

        private PlayerAvatar avatar;
        private ScootBlock[] scooters;

        public override IEnumerable<Entity> Entities()
        {
            yield return avatar;

            for (int i = 0; i < scooters.Length; i++)
                yield return scooters[i];
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            avatar = new PlayerAvatar(new WorldPosition(0, 0, 0, 3, 0), this);
            Camera.StartFollowing(avatar);

            scooters = new ScootBlock[11];

            Skybox = new Skybox();
            Skybox.LoadContent(Game, "Textures/Skyboxes/SkyboxLayout");

            for (int i = 0; i < scooters.Length; i++)
                scooters[i] = new ScootBlock(new WorldPosition(0, 0, i, 20, i % 2), this);
        }
    }
}

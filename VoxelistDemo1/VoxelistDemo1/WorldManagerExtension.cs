using System;
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
        private const string SkyboxTextureLocation = "Textures/Skyboxes/SkyboxGraphics";

        public WorldManagerExtension(Game game, MapExtension map, BlockHandlerExtension handler, EntityBuilderExtension builder)
            : base(game, map, handler, builder, SkyboxTextureLocation)
        {
        }

        public PlayerAvatar Avatar { get; private set; }

        protected override IEnumerable<Entity> ManualEntities()
        {
            yield return Avatar;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Avatar = new PlayerAvatar(new WorldPosition(0, 0, 0, 3, 0), this);
            Camera.StartFollowing(Avatar);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Rendering;
using Voxelist.Mapping;
using Voxelist.Entities;

namespace VoxelistDemo2
{
    public class WorldManagerExtension : WorldManager
    {
        public WorldManagerExtension(DemoGame game, MapExtension map, BlockHandlerExtension handler)
            : base(game, map, handler)
        {
        }

        private PlayerAvatar avatar;

        public override void Initialize()
        {
            base.Initialize();

            avatar = new PlayerAvatar(new WorldPosition(0, 0, 0, 70, 0), this);
            Camera.StartFollowing(avatar);

            Skybox = new Skybox();
            Skybox.LoadContent(Game, "Textures/Skyboxes/SkyboxLayout");
        }

        public override IEnumerable<Entity> Entities()
        {
            yield return avatar;
        }
    }
}

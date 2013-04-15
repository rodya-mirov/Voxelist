using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Rendering;
using Voxelist.Mapping;
using Voxelist.Entities;
using Voxelist.Utilities;

namespace VoxelistDemo2
{
    public class WorldManagerExtension : WorldManager
    {
        public WorldManagerExtension(DemoGame2 game, MapExtension map, BlockHandlerExtension handler, EntityBuilderExtension builder)
            : base(game, map, handler, builder)
        {
        }

        private PlayerAvatar avatar;

        public override void Initialize()
        {
            base.Initialize();

            avatar = new PlayerAvatar(new WorldPosition(0, 0, 0, 70, 0), this);
            Camera.StartFollowing(avatar);
        }

        protected override IEnumerable<Entity> ManualEntities()
        {
            yield return avatar;
        }
    }
}

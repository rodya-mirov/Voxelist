using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.Rendering;
using Voxelist.Mapping;
using Voxelist.Entities;
using Voxelist.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VoxelistDemo2
{
    public class WorldManagerExtension : WorldManager
    {
        public WorldManagerExtension(MapExtension map)
            : base(map)
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

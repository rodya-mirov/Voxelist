using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Rendering;
using Voxelist.Entities;
using Voxelist.Mapping;
using Microsoft.Xna.Framework.Input;

namespace VoxelistDemo3
{
    public class WorldManagerExtension : WorldManager
    {
        public WorldManagerExtension(Game game, MapExtension map, BlockHandlerExtension handler)
            : base(game, map, handler)
        {
        }

        public PlayerAvatar Avatar { get; private set; }
        public MouseOverBlock TestBlock { get; private set; }

        public override IEnumerable<Entity> Entities()
        {
            yield return Avatar;
            yield return TestBlock;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            MouseOverBlock.LoadContent(this.Game);

            Avatar = new PlayerAvatar(new WorldPosition(0, 0, 0, 3, -5), this);
            Camera.StartFollowing(Avatar);

            TestBlock = new MouseOverBlock(new WorldPosition(0, 0, 0, 3, -2), this, Avatar);

            Skybox = new Skybox();
            Skybox.LoadContent(Game, "Textures/Skyboxes/SkyboxGraphics");
        }

        private bool leftMouseHeld = false;
        private bool rightMouseHeld = false;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MouseState ms = Mouse.GetState();

            if (ms.LeftButton == ButtonState.Pressed && !leftMouseHeld)
                TestBlock.AlwaysOnTop = !TestBlock.AlwaysOnTop;

            if (ms.RightButton == ButtonState.Pressed && !rightMouseHeld)
                TestBlock.WantVisible = !TestBlock.WantVisible;

            leftMouseHeld = (ms.LeftButton == ButtonState.Pressed);
            rightMouseHeld = (ms.RightButton == ButtonState.Pressed);
        }
    }
}

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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            updateLighting(gameTime);
        }

        protected override IEnumerable<Entity> ManualEntities()
        {
            yield return avatar;
        }

        protected override VoxelistEffectWrapper MakeEffectWrapper()
        {
            Effect effect = Game.Content.Load<Effect>("Effects/CustomLighting");
            return new LightingEffectExtension(effect);
        }

        double sunAngle = 0;

        private void updateLighting(GameTime gameTime)
        {
            sunAngle += .1 * gameTime.ElapsedGameTime.TotalSeconds;

            while (sunAngle >= MathHelper.Pi)
                sunAngle -= MathHelper.TwoPi;

            Vector3 sunPosition = Vector3.Forward + Vector3.Transform(Vector3.Up, Matrix.CreateRotationZ((float)sunAngle));

            float diffuseStrength = sunPosition.Y;
            if (diffuseStrength < 0)
                diffuseStrength = 0;

            drawingEffectWrapper.Effect.Parameters["DiffuseIntensity"].SetValue(diffuseStrength);
            drawingEffectWrapper.Effect.Parameters["DiffuseLightDirection"].SetValue(-sunPosition);
        }
    }
}

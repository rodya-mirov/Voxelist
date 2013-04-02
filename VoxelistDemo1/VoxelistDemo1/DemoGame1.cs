using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Voxelist;
using Voxelist.Utilies;
using Voxelist.Rendering;

namespace VoxelistDemo1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DemoGame1 : VoxelistGame
    {
        private WorldManagerExtension worldManager;

        private SpriteBatch batch;
        private SpriteFont Font;

        public DemoGame1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            BlockHandlerExtension handler = new BlockHandlerExtension();
            EntityBuilderExtension builder = new EntityBuilderExtension();

            MapExtension map = new MapExtension(handler);

            worldManager = new WorldManagerExtension(this, map, handler, builder);
            Components.Add(worldManager);

            Components.Add(new FPSComponent(this, "Fonts/Segoe"));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            ScootBlock.LoadContent(this);

            batch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Fonts/Segoe");

            LoadGraphicsSettings();
        }

        private void LoadGraphicsSettings()
        {
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private bool wasHoldingF12 = false;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();

            if (ks.IsKeyDown(Keys.F12) && !wasHoldingF12)
                    ToggleFullScreen();

            wasHoldingF12 = ks.IsKeyDown(Keys.F12);

            base.Update(gameTime);
        }

        private float horizontalRotateSpeed = -0.007f;
        private float verticalRotateSpeed = -0.007f;

        protected override void MouseMoved(int xChange, int yChange)
        {
            Camera.RotateHorizontal(horizontalRotateSpeed * (float)(xChange));
            Camera.RotateVertical(verticalRotateSpeed * (float)(yChange));
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            DrawDebugString();
        }

        public void DrawDebugString()
        {
            String toDraw = Camera.HorizontalForward.ToString();
            toDraw += "\n" + worldManager.Avatar.Position.ToString();

            int x = 40;
            int y = 60;

            BlendState blend = GraphicsDevice.BlendState;
            DepthStencilState stencil = GraphicsDevice.DepthStencilState;
            SamplerState sampler = GraphicsDevice.SamplerStates[0];

            batch.Begin();
            batch.DrawString(Font, toDraw, new Vector2(x, y+1), Color.Black);
            batch.DrawString(Font, toDraw, new Vector2(x, y), Color.White);
            batch.End();

            GraphicsDevice.BlendState = blend;
            GraphicsDevice.DepthStencilState = stencil;
            GraphicsDevice.SamplerStates[0] = sampler;
        }
    }
}

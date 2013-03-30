using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Rendering;
using Voxelist.Utilities;

namespace Voxelist
{
    public abstract class VoxelistGame : Game
    {
        protected virtual bool useHyperMode { get { return false; } }

        protected virtual bool startInFullScreen { get { return false; } }
        protected bool isFullScreen { get; set; }

        protected virtual bool lockMouseToScreen { get { return true; } }
        protected int desiredMouseX, desiredMouseY;

        protected int preferredScreenWidth, preferredScreenHeight;

        protected GraphicsDeviceManager graphics { get; set; }

        public VoxelistGame()
            : base()
        {
            isFullScreen = false;
        }

        protected override void Initialize()
        {
            if (useHyperMode)
            {
                this.IsFixedTimeStep = false;
                this.graphics.SynchronizeWithVerticalRetrace = false;
                this.graphics.ApplyChanges();
            }

            if (lockMouseToScreen)
            {
                desiredMouseX = GraphicsDevice.Viewport.Width >> 1;
                desiredMouseY = GraphicsDevice.Viewport.Height >> 1;

                Mouse.SetPosition(desiredMouseX, desiredMouseY);
            }

            RandomHelper.Initialize();

            Camera.Start();

            Camera.AspectRatio = this.GraphicsDevice.Viewport.AspectRatio;

            preferredScreenWidth = GraphicsDevice.Viewport.Width;
            preferredScreenHeight = GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        protected void ToggleFullScreen()
        {
            if (isFullScreen)
            {
                isFullScreen = false;

                graphics.PreferredBackBufferWidth = preferredScreenWidth;
                graphics.PreferredBackBufferHeight = preferredScreenHeight;

                graphics.IsFullScreen = false;

                graphics.ApplyChanges();

                Camera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
            }
            else
            {
                isFullScreen = true;

                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                graphics.IsFullScreen = true;

                graphics.ApplyChanges();

                Camera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
            }

            Mouse.SetPosition(desiredMouseX, desiredMouseY);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            if (startInFullScreen)
            {
                if (!isFullScreen)
                    ToggleFullScreen();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MouseState ms = Mouse.GetState();

            if (lockMouseToScreen)
            {
                int xChange = ms.X - desiredMouseX;
                int yChange = ms.Y - desiredMouseY;

                Mouse.SetPosition(desiredMouseX, desiredMouseY);

                MouseMoved(xChange, yChange);
            }
        }

        protected virtual void MouseMoved(int xChange, int yChange)
        {
            //do nothing
        }
    }
}

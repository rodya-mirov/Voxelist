using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Voxelist.BlockHandling;
using Voxelist.GeometryPrimitives;

namespace VoxelistDemo1
{
    /// <summary>
    /// 0 is invisible
    /// 1 is dirt (with grass on top)
    /// 2 is water (semi-transparent)
    /// </summary>
    public class BlockHandlerExtension : BlockHandler
    {
        public BlockHandlerExtension()
            : base()
        {
            //2**6 == 64
            //So we'll need 64 cases to cover all the possible
            //boolean flag combinations for the DrawingPrimitive
            //calls
            fullSizeBlocks = new GeometryPrimitive[64];

            for (int i = 0; i < 64; i++)
            {
                bool includeFrontFace, includeBackFace;
                bool includeTopFace, includeBottomFace;
                bool includeLeftFace, includeRightFace;

                ConvertIntToBoolFlags(i,
                    out includeFrontFace, out includeBackFace,
                    out includeTopFace, out includeBottomFace,
                    out includeLeftFace, out includeRightFace);

                fullSizeBlocks[i] = GeometryPrimitive.Make3DRectangle(
                    new Vector3(0, 0, 0), new Vector3(1, 1, 1),
                    new Vector2(0, 0), new Vector2(1, 1),
                    includeFrontFace, includeBackFace, includeTopFace,
                    includeBottomFace, includeLeftFace, includeRightFace
                    );
            }
        }

        private GeometryPrimitive[] fullSizeBlocks;

        public override void LoadContent(Game game)
        {
            base.LoadContent(game);

            setupDrawingEffects(game);
        }

        #region Drawing Data
        public override int TotalNumberOfTextures
        {
            get { return 2; }
        }

        public override int TextureIndex(Block block)
        {
            switch (block.blockID)
            {
                case 1:
                    return 0;

                case 2:
                    return 1;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void setupDrawingEffects(Game game)
        {
            dirtEffect = new BasicEffect(game.GraphicsDevice);
            dirtEffect.TextureEnabled = true;
            dirtEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");
            dirtEffect.EnableDefaultLighting();

            iceEffect = new BasicEffect(game.GraphicsDevice);
            iceEffect.TextureEnabled = true;
            iceEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Ice");
            iceEffect.EnableDefaultLighting();
        }

        private static BasicEffect dirtEffect, iceEffect;
        public override BasicEffect DrawingEffect(int textureID)
        {
            switch (textureID)
            {
                case 0:
                    return dirtEffect;

                case 1:
                    return iceEffect;

                default: throw new NotImplementedException();
            }
        }

        public override bool IsVisible(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                case 2:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override GeometryPrimitive DrawingPrimitive(Block block,
            bool includeFrontFace = true, bool includeBackFace = true,
            bool includeTopFace = true, bool includeBottomFace = true,
            bool includeLeftFace = true, bool includeRightFace = true)
        {
            switch (block.blockID)
            {
                case 1:
                case 2:
                    int flags = ConvertBoolFlagsToInt(includeFrontFace, includeBackFace, includeTopFace, includeBottomFace, includeLeftFace, includeRightFace);
                    return fullSizeBlocks[flags];

                default: throw new ArgumentOutOfRangeException();
            }
        }
        #endregion Drawing Data

        #region Physics Data
        public override bool IsPassable(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return true;

                case 1:
                case 2:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override float Friction(Block block)
        {
            switch (block.blockID)
            {
                case 1:
                    return 20;

                case 2:
                    return 0.3f;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Vector3 FrictionVelocity(Block block)
        {
            return Vector3.Zero;
        }
        #endregion Physics
    }
}

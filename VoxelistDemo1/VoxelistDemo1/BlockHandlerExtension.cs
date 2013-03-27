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

        private static BasicEffect drawingEffect;
        public override BasicEffect DrawingEffect { get { return drawingEffect; } }

        public override void LoadContent(Game game)
        {
            base.LoadContent(game);

            drawingEffect = new BasicEffect(game.GraphicsDevice);

            drawingEffect.TextureEnabled = true;
            drawingEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");

            drawingEffect.EnableDefaultLighting();
        }

        #region Drawing Data
        public override bool IsVisible(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
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
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override BoundingBox PhysicalBlockingBox(Block block)
        {
            switch (block.blockID)
            {
                case 1:
                    return new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));

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

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Vector3 FrictionVelocity(Block block)
        {
            return Vector3.Zero;
        }
        #endregion Physics

        #region Visual Occlusion Data
        public override bool IsFullAndOpaqueToTheRight(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override bool IsFullAndOpaqueToTheLeft(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override bool IsFullAndOpaqueToTheFront(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override bool IsFullAndOpaqueToTheBack(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override bool IsFullAndOpaqueToTheTop(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override bool IsFullAndOpaqueToTheBottom(Block block)
        {
            switch (block.blockID)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default: throw new ArgumentOutOfRangeException();
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxelist.BlockHandling;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.GeometryPrimitives;
using Microsoft.Xna.Framework;

namespace VoxelistDemo2
{
    /// <summary>
    /// Summary of block information:
    /// 
    /// All blocks have the same texture, roughly.  No block is
    /// designated by blockID:0; otherwise, the block height
    /// is blockID/G (and blockID varies from 1 to G, inclusive)
    /// with full occlusion and blocking.
    /// </summary>
    public class BlockHandlerExtension : BlockHandler
    {
        public const int GRANULARITY = 2;
        private const float HEIGHT_INCREMENT = 1.0f / (float)GRANULARITY;

        public BlockHandlerExtension()
            : base()
        {
        }

        private GeometryPrimitive[,] primitives; //first index is blockID, second is the drawing flags
        private BasicEffect dirtEffect;

        public override void LoadContent(Game game)
        {
            base.LoadContent(game);

            dirtEffect = new BasicEffect(game.GraphicsDevice);

            dirtEffect.TextureEnabled = true;
            dirtEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");

            dirtEffect.EnableDefaultLighting();

            primitives = new GeometryPrimitive[GRANULARITY, 64];

            for (int i = 0; i < GRANULARITY; i++)
            {
                Vector3 min = new Vector3(0, 0, 0);
                Vector3 max = new Vector3(1, HEIGHT_INCREMENT * (i + 1), 1);
                if (max.Y > 1)
                    max.Y = 1;

                for (int j = 0; j < 64; j++)
                {
                    bool includeFrontFace, includeBackFace;
                    bool includeTopFace, includeBottomFace;
                    bool includeLeftFace, includeRightFace;

                    BlockHandler.ConvertIntToBoolFlags(j,
                        out includeFrontFace, out includeBackFace,
                        out includeTopFace, out includeBottomFace,
                        out includeLeftFace, out includeRightFace);

                    primitives[i, j] = GeometryPrimitive.Make3DRectangle(
                        min, max,
                        new Vector2(0, 0), new Vector2(1, 1),
                        includeFrontFace, includeBackFace,
                        includeTopFace, includeBottomFace,
                        includeLeftFace, includeRightFace);
                }
            }
        }

        public override int TotalNumberOfTextures
        {
            get { return 1; }
        }

        public override int TextureIndex(Block block)
        {
            return 0;
        }

        public override BasicEffect DrawingEffect(int textureIndex)
        {
            return dirtEffect;
        }

        public override GeometryPrimitive DrawingPrimitive(Block block,
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace)
        {
            int flag = BlockHandler.ConvertBoolFlagsToInt(
                includeFrontFace, includeBackFace,
                includeTopFace, includeBottomFace,
                includeLeftFace, includeRightFace);

            return primitives[block.blockID - 1, flag];
        }

        public override bool IsPassable(Block block)
        {
            return block.blockID == 0;
        }

        public override BoundingBox PhysicalBlockingBox(Block block)
        {
            Vector3 min = new Vector3(0, 0, 0);
            Vector3 max = new Vector3(1, HEIGHT_INCREMENT * block.blockID, 1);
            if (max.Y > 1)
                max.Y = 1;

            return new BoundingBox(min, max);
        }

        public override float Friction(Block block)
        {
            if (block.blockID > 0 && block.blockID <= GRANULARITY)
                return 20f;
            else
                throw new ArgumentOutOfRangeException();
        }

        public override Vector3 FrictionVelocity(Block block)
        {
            return Vector3.Zero;
        }

        public override bool IsVisible(Block block)
        {
            return block.blockID != 0;
        }

        #region Occlusion Flags
        public override bool IsFullAndOpaqueToTheTop(Block block)
        {
            return block.blockID == GRANULARITY;
        }

        public override bool IsFullAndOpaqueToTheBottom(Block block)
        {
            return block.blockID != 0;
        }

        public override bool IsFullAndOpaqueToTheFront(Block block)
        {
            return block.blockID == GRANULARITY;
        }

        public override bool IsFullAndOpaqueToTheBack(Block block)
        {
            return block.blockID == GRANULARITY;
        }

        public override bool IsFullAndOpaqueToTheLeft(Block block)
        {
            return block.blockID == GRANULARITY;
        }

        public override bool IsFullAndOpaqueToTheRight(Block block)
        {
            return block.blockID == GRANULARITY;
        }
        #endregion
    }
}

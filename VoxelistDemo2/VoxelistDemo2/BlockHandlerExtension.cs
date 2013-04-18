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
        public BlockHandlerExtension()
            : base()
        {
            loadPrimitives();
        }

        private GeometryPrimitive[] primitives; //first index is blockID, second is the drawing flags

        private Texture2D[] textures;

        protected override void loadContent(Game game)
        {
            base.loadContent(game);

            textures = new Texture2D[1];
            textures[0] = Game.Content.Load<Texture2D>("Textures/Cubes/Dirt");
        }

        protected override Texture2D texture(int textureIndex)
        {
            return textures[0];
        }

        private void loadPrimitives()
        {
            primitives = new GeometryPrimitive[64];

            Vector3 min = new Vector3(0, 0, 0);
            Vector3 max = new Vector3(1, 1, 1);
            if (max.Y > 1)
                max.Y = 1;

            for (int flag = 0; flag < 64; flag++)
            {
                bool includeFrontFace, includeBackFace;
                bool includeTopFace, includeBottomFace;
                bool includeLeftFace, includeRightFace;

                BlockHandler.ConvertIntToBoolFlags(flag,
                    out includeFrontFace, out includeBackFace,
                    out includeTopFace, out includeBottomFace,
                    out includeLeftFace, out includeRightFace);

                primitives[flag] = GeometryPrimitive.Make3DRectangle(
                    min, max,
                    new Vector2(0, 0), new Vector2(1, 1),
                    includeFrontFace, includeBackFace,
                    includeTopFace, includeBottomFace,
                    includeLeftFace, includeRightFace);
            }
        }

        protected override int totalNumberOfTextures
        {
            get { return 1; }
        }

        protected override int textureIndex(Block block)
        {
            return 0;
        }

        protected override GeometryPrimitive drawingPrimitive(Block block,
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace)
        {
            int flag = BlockHandler.ConvertBoolFlagsToInt(
                includeFrontFace, includeBackFace,
                includeTopFace, includeBottomFace,
                includeLeftFace, includeRightFace);

            return primitives[flag];
        }

        protected override bool isPassable(Block block)
        {
            return block.blockID == 0;
        }

        protected override float friction(Block block)
        {
            return 20f;
        }

        protected override Vector3 frictionVelocity(Block block)
        {
            return Vector3.Zero;
        }

        protected override bool isVisible(Block block)
        {
            return block.blockID != 0;
        }
    }
}

#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Voxelist.GeometryPrimitives;

namespace Voxelist.BlockHandling
{
    /// <summary>
    /// This is intended basically as a lookup table for Block data.
    /// At a minimum, one must be able to turn Blocks into drawing
    /// data as well as the real basic fact: whether the block exists at all.
    /// 
    /// This is a singleton class.  There should never be a need to make
    /// more than one of these!
    /// </summary>
    public abstract class BlockHandler
    {
        private static BlockHandler instance = null;

        protected BlockHandler()
        {
            if (instance != null)
                throw new InvalidProgramException("Can't instantiate two BlockHandlers!");

            instance = this;
        }

        protected Game Game { get; private set; }

        public static void LoadContent(Game game) { instance.loadContent(game); }

        protected virtual void loadContent(Game game)
        {
            this.Game = game;
        }

        #region Visual Obstruction Flags
        protected static int ConvertBoolFlagsToInt(
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace
            )
        {
            int i = 0;
            if (includeFrontFace) i = (i | 1);
            if (includeBackFace) i = (i | 2);
            if (includeTopFace) i = (i | 4);
            if (includeBottomFace) i = (i | 8);
            if (includeLeftFace) i = (i | 16);
            if (includeRightFace) i = (i | 32);

            return i;
        }

        protected static void ConvertIntToBoolFlags(int input,
            out bool includeFrontFace, out bool includeBackFace,
            out bool includeTopFace, out bool includeBottomFace,
            out bool includeLeftFace, out bool includeRightFace
            )
        {
            includeFrontFace = (input & 1) != 0;
            includeBackFace = (input & 2) != 0;
            includeTopFace = (input & 4) != 0;
            includeBottomFace = (input & 8) != 0;
            includeLeftFace = (input & 16) != 0;
            includeRightFace = (input & 32) != 0;
        }

        /// <summary>
        /// Whether or not this block, when drawn,
        /// fills the entire allotted space and is
        /// completely opaque (and in particular, is
        /// visible!).  Used for culling drawing
        /// data, so be sure to be correct :P
        /// 
        /// This particular form is whether it blocks the
        /// view to the right; that is, whether the right
        /// face is completely opaque and fills the space.
        /// 
        /// Default behavior is to return IsVisible;
        /// this need only be overridden if this block is
        /// partially transparent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected virtual bool isFullAndOpaqueToTheRight(Block block)
        {
            return instance.isVisible(block);
        }

        public static bool IsFullAndOpaqueToTheRight(Block block) { return instance.isFullAndOpaqueToTheRight(block); }

        /// <summary>
        /// Whether or not this block, when drawn,
        /// fills the entire allotted space and is
        /// completely opaque (and in particular, is
        /// visible!).  Used for culling drawing
        /// data, so be sure to be correct :P
        /// 
        /// This particular form is whether it blocks the
        /// view to the left; that is, whether the left
        /// face is completely opaque and fills the space.
        /// 
        /// Default behavior is to return IsVisible;
        /// this need only be overridden if this block is
        /// partially transparent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected virtual bool isFullAndOpaqueToTheLeft(Block block)
        {
            return instance.isVisible(block);
        }

        public static bool IsFullAndOpaqueToTheLeft(Block block) { return instance.isFullAndOpaqueToTheLeft(block); }

        /// <summary>
        /// Whether or not this block, when drawn,
        /// fills the entire allotted space and is
        /// completely opaque (and in particular, is
        /// visible!).  Used for culling drawing
        /// data, so be sure to be correct :P
        /// 
        /// This particular form is whether it blocks the
        /// view to the front; that is, whether the front
        /// face is completely opaque and fills the space.
        /// 
        /// Default behavior is to return IsVisible;
        /// this need only be overridden if this block is
        /// partially transparent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected virtual bool isFullAndOpaqueToTheFront(Block block)
        {
            return instance.isVisible(block);
        }

        public static bool IsFullAndOpaqueToTheFront(Block block) { return instance.isFullAndOpaqueToTheFront(block); }

        /// <summary>
        /// Whether or not this block, when drawn,
        /// fills the entire allotted space and is
        /// completely opaque (and in particular, is
        /// visible!).  Used for culling drawing
        /// data, so be sure to be correct :P
        /// 
        /// This particular form is whether it blocks the
        /// view to the back; that is, whether the back
        /// face is completely opaque and fills the space.
        /// 
        /// Default behavior is to return IsVisible;
        /// this need only be overridden if this block is
        /// partially transparent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected virtual bool isFullAndOpaqueToTheBack(Block block)
        {
            return instance.isVisible(block);
        }

        public static bool IsFullAndOpaqueToTheBack(Block block) { return instance.isFullAndOpaqueToTheBack(block); }

        /// <summary>
        /// Whether or not this block, when drawn,
        /// fills the entire allotted space and is
        /// completely opaque (and in particular, is
        /// visible!).  Used for culling drawing
        /// data, so be sure to be correct :P
        /// 
        /// This particular form is whether it blocks the
        /// view to the top; that is, whether the top
        /// face is completely opaque and fills the space.
        /// 
        /// Default behavior is to return IsVisible;
        /// this need only be overridden if this block is
        /// partially transparent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected virtual bool isFullAndOpaqueToTheTop(Block block)
        {
            return instance.isVisible(block);
        }

        public static bool IsFullAndOpaqueToTheTop(Block block) { return instance.isFullAndOpaqueToTheTop(block); }

        /// <summary>
        /// Whether or not this block, when drawn,
        /// fills the entire allotted space and is
        /// completely opaque (and in particular, is
        /// visible!).  Used for culling drawing
        /// data, so be sure to be correct :P
        /// 
        /// This particular form is whether it blocks the
        /// view to the bottom; that is, whether the bottom
        /// face is completely opaque and fills the space.
        /// 
        /// Default behavior is to return IsVisible;
        /// this need only be overridden if this block is
        /// partially transparent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected virtual bool isFullAndOpaqueToTheBottom(Block block)
        {
            return instance.isVisible(block);
        }

        public static bool IsFullAndOpaqueToTheBottom(Block block) { return instance.isFullAndOpaqueToTheBottom(block); }
        #endregion

        #region Physics Data
        /// <summary>
        /// Whether or not this block is passable
        /// (for walking and falling through purposes).
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected abstract bool isPassable(Block block);

        public static bool IsPassable(Block block) { return instance.isPassable(block); }

        /// <summary>
        /// The boundingbox which this square actually blocks
        /// passage through.  This method is only valid when IsPassable
        /// is false.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected BoundingBox physicalBlockingBox(Block block)
        {
            return new BoundingBox(Vector3.Zero, Vector3.One);
        }

        public static BoundingBox PhysicalBlockingBox(Block block) { return instance.physicalBlockingBox(block); }

        /// <summary>
        /// The amount of friction one achieves while moving along this block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected abstract float friction(Block block);

        public static float Friction(Block block) { return instance.friction(block); }

        protected abstract Vector3 frictionVelocity(Block block);

        public static Vector3 FrictionVelocity(Block block) { return instance.frictionVelocity(block); }
        #endregion

        #region Drawing Data
        protected abstract int totalNumberOfTextures { get; }

        public static int TotalNumberOfTextures { get { return instance.totalNumberOfTextures; } }

        protected abstract Texture2D texture(int textureIndex);

        public static Texture2D Texture(int textureIndex) { return instance.texture(textureIndex); }

        protected abstract int textureIndex(Block block);

        public static int TextureIndex(Block block) { return instance.textureIndex(block); }

        /// <summary>
        /// Whether or not to draw this block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected abstract bool isVisible(Block block);

        public static bool IsVisible(Block block) { return instance.isVisible(block); }

        /// <summary>
        /// The boundingbox which this square actually blocks
        /// passage through.  This method is only valid when IsVisible
        /// is true.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected BoundingBox visualBoundingBox(Block block)
        {
            return new BoundingBox(Vector3.Zero, Vector3.One);
        }

        public static BoundingBox VisualBoundingBox(Block block) { return instance.visualBoundingBox(block); }

        /// <summary>
        /// The model for drawing this block.  Not required to
        /// be implemented when IsVisible is false.
        /// 
        /// The optional parameters refer to possibly excluding occluded
        /// faces.  Failure to take these into account will hurt performance
        /// significantly.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected abstract GeometryPrimitive drawingPrimitive(Block block,
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace);

        public static GeometryPrimitive DrawingPrimitive(Block block,
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace)
        {
            return instance.drawingPrimitive(block,
                includeFrontFace, includeBackFace, includeTopFace,
                includeBottomFace, includeLeftFace, includeRightFace);
        }
        #endregion
    }
}

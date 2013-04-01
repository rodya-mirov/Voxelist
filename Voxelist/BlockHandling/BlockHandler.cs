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
    /// data (models, bone transforms, and relevant transformations),
    /// as well as the real basic fact: whether the block exists at all.
    /// </summary>
    public abstract class BlockHandler
    {

        protected Game Game;

        public virtual void LoadContent(Game game)
        {
            this.Game = game;
        }

        #region Visual Obstruction Flags
        public static int ConvertBoolFlagsToInt(
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

        public static void ConvertIntToBoolFlags(int input,
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
        public virtual bool IsFullAndOpaqueToTheRight(Block block)
        {
            return IsVisible(block);
        }

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
        public virtual bool IsFullAndOpaqueToTheLeft(Block block)
        {
            return IsVisible(block);
        }

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
        public virtual bool IsFullAndOpaqueToTheFront(Block block)
        {
            return IsVisible(block);
        }

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
        public virtual bool IsFullAndOpaqueToTheBack(Block block)
        {
            return IsVisible(block);
        }

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
        public virtual bool IsFullAndOpaqueToTheTop(Block block)
        {
            return IsVisible(block);
        }

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
        public virtual bool IsFullAndOpaqueToTheBottom(Block block)
        {
            return IsVisible(block);
        }
        #endregion

        #region Physics Data
        /// <summary>
        /// Whether or not this block is passable
        /// (for walking and falling through purposes).
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public abstract bool IsPassable(Block block);

        /// <summary>
        /// The boundingbox which this square actually blocks
        /// passage through.  This method is only valid when IsPassable
        /// is false.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public BoundingBox PhysicalBlockingBox(Block block)
        {
            return new BoundingBox(Vector3.Zero, Vector3.One);
        }

        /// <summary>
        /// The amount of friction one achieves while moving along this block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public abstract float Friction(Block block);

        public abstract Vector3 FrictionVelocity(Block block);
        #endregion

        #region Drawing Data
        public abstract int TotalNumberOfTextures { get; }

        public abstract BasicEffect DrawingEffect(int textureIndex);

        public abstract int TextureIndex(Block block);

        /// <summary>
        /// Whether or not to draw this block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public abstract bool IsVisible(Block block);

        /// <summary>
        /// The boundingbox which this square actually blocks
        /// passage through.  This method is only valid when IsVisible
        /// is true.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public BoundingBox VisualBoundingBox(Block block)
        {
            return new BoundingBox(Vector3.Zero, Vector3.One);
        }

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
        public abstract GeometryPrimitive DrawingPrimitive(Block block,
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace);
        #endregion
    }
}

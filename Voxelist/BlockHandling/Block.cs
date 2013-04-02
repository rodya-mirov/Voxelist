using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Voxelist.BlockHandling
{
    /// <summary>
    /// This is a very simple struct, which contains the minimum data required
    /// to be a useful Block (in-world cube), but requires a lookup table (in
    /// the form of a BlockHandler) to be of any real use.
    /// </summary>
    /// <typeparam name="BlockHandlerType"></typeparam>
    public struct Block
    {
        public int blockID;

        public Block(int blockID)
        {
            this.blockID = blockID;
        }
    }

    public enum Face { LEFT, RIGHT, TOP, BOTTOM, BACK, FRONT }
}

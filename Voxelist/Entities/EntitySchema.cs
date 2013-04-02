using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxelist.Entities
{
    /// <summary>
    /// This represents enough data, in a minimal form, to *construct*
    /// an Entity (for an EntityHandler).  That is, like the Block struct,
    /// this is a recipe for constructing Entities.  It can be stored as
    /// part of a Chunk, allowing for procedural generation of Entities
    /// as with blocks.
    /// </summary>
    public struct EntitySchema
    {
        /// <summary>
        /// Representing the type of the Entity which is built.
        /// </summary>
        public int TypeID;

        /// <summary>
        /// Representing the position of the Entity to be built, 
        /// within the intended chunk.
        /// </summary>
        public float inChunkX, inChunkY, inChunkZ;

        public EntitySchema(int typeID, float inChunkX, float inChunkY, float inChunkZ)
        {
            this.TypeID = typeID;

            this.inChunkX = inChunkX;
            this.inChunkY = inChunkY;
            this.inChunkZ = inChunkZ;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxelist.Utilities
{
    public struct ChunkCoordinate
    {
        public int X, Z;

        public ChunkCoordinate(int x, int z)
        {
            this.X = x;
            this.Z = z;
        }

        public bool Equals(ChunkCoordinate other)
        {
            return other.X == this.X && other.Z == this.Z;
        }

        public override bool Equals(object o)
        {
            if (o is Point3)
                return this == (ChunkCoordinate)o;
            else
                return false;
        }

        public static bool operator ==(ChunkCoordinate a, ChunkCoordinate b)
        {
            return a.X == b.X && a.Z == b.Z;
        }

        public static bool operator !=(ChunkCoordinate a, ChunkCoordinate b)
        {
            return a.X != b.X || a.Z != b.Z;
        }

        public override int GetHashCode()
        {
            return (X << 16) + (Z & 65535);
        }
    }
}

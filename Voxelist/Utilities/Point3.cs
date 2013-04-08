using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Voxelist.Utilities
{
    public struct Point3 : IEquatable<Point3>
    {
        public int X;
        public int Y;
        public int Z;

        public Point3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static implicit operator Vector3(Point3 point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        public static Point3 RoundDown(Vector3 vec)
        {
            int xmin = (int)vec.X;
            while (xmin > vec.X)
                xmin--;

            int ymin = (int)vec.Y;
            while (ymin > vec.Y)
                ymin--;

            int zmin = (int)vec.Z;
            while (zmin > vec.Z)
                zmin--;

            return new Point3(xmin, ymin, zmin);
        }

        public static Point3 RoundUp(Vector3 vec)
        {
            int xmax = (int)vec.X;
            while (xmax < vec.X)
                xmax++;

            int ymax = (int)vec.Y;
            while (ymax < vec.Y)
                ymax++;

            int zmax = (int)vec.Z;
            while (zmax < vec.Z)
                zmax++;

            return new Point3(xmax, ymax, zmax);
        }

        public static Point3 operator +(Point3 a, Point3 b)
        {
            return new Point3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3 operator -(Point3 a)
        {
            return new Point3(-a.X, -a.Y, -a.Z);
        }

        public static Point3 operator -(Point3 a, Point3 b)
        {
            return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3 operator *(Point3 a, int k)
        {
            return new Point3(a.X * k, a.Y * k, a.Z * k);
        }

        public static Point3 operator *(int k, Point3 a)
        {
            return new Point3(a.X * k, a.Y * k, a.Z * k);
        }

        public bool Equals(Point3 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public override bool Equals(object o)
        {
            if (o is Point3)
                return this.Equals((Point3)o);
            else
                return false;
        }

        public static bool operator ==(Point3 a, Point3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Point3 a, Point3 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public override int GetHashCode()
        {
            return X ^ Y ^ Z;
        }

        public int MaxNorm()
        {
            return Math.Max(Math.Max(Math.Abs(X), Math.Abs(Y)), Math.Abs(Z));
        }
    }
}

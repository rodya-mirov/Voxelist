using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxelist.Utilities
{
    public class TorusBoolArray
    {
        private bool[,] data;
        public int Radius { get; private set; }

        public int Width { get { return Radius * 2 + 1; } }
        public int Height { get { return Radius * 2 + 1; } }

        public int XMin { get; private set; }
        public int XMax { get { return XMin + Width - 1; } }

        public int ZMin { get; private set; }
        public int ZMax { get { return ZMin + Height - 1; } }

        private int xStartIndex { get; set; }
        private int zStartIndex { get; set; }

        private bool DefaultValue;

        public TorusBoolArray(int radius, int centerX, int centerZ, bool defaultValue)
        {
            this.Radius = radius;
            this.DefaultValue = defaultValue;

            data = new bool[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Height; z++)
                {
                    data[x, z] = DefaultValue;
                }
            }

            XMin = centerX - Radius;
            ZMin = centerZ - Radius;

            xStartIndex = 0;
            zStartIndex = 0;
        }

        public bool this[int x, int z]
        {
            get
            {
                if (x < XMin || x > XMax || z < ZMin || z > ZMax)
                    throw new IndexOutOfRangeException();

                int xIndex = Numerical.IntMod(x - XMin + xStartIndex, Width);
                int zIndex = Numerical.IntMod(z - ZMin + zStartIndex, Height);

                return data[xIndex, zIndex];
            }
            set
            {
                if (x < XMin || x > XMax || z < ZMin || z > ZMax)
                    throw new IndexOutOfRangeException();

                int xIndex = Numerical.IntMod(x - XMin + xStartIndex, Width);
                int zIndex = Numerical.IntMod(z - ZMin + zStartIndex, Height);

                data[xIndex, zIndex] = value;
            }
        }

        public void AdjustRange(int dx, int dz)
        {
            HorizontalMove(dx);
            VerticalMove(dz);
        }

        private void HorizontalMove(int dx)
        {
            //the following aren't as symmetric as it feels like they
            //should be, because the minimum is the fixed index, so
            //the indexing isn't symmetric in the direction of the change

            if (dx > 0)
            {
                //clear the (soon-to-be) right columns out
                for (int x = 0; x < dx; x++)
                {
                    for (int z = 0; z < Height; z++)
                    {
                        int xIndex = Numerical.IntMod(x + xStartIndex, Width);
                        int zIndex = z;

                        data[xIndex, zIndex] = DefaultValue;
                    }
                }
            }
            else if (dx < 0)
            {
                //clear the left columns out ...
                for (int x = dx; x < 0; x++)
                {
                    for (int z = 0; z < Height; z++)
                    {
                        int xIndex = Numerical.IntMod(x + xStartIndex, Width);
                        int zIndex = z;

                        data[xIndex, zIndex] = DefaultValue;
                    }
                }
            }

            xStartIndex = Numerical.IntMod(xStartIndex + dx, Width); //xStartIndex -= dx, but in range
            XMin += dx;
        }

        private void VerticalMove(int dz)
        {
            //as before ...

            if (dz > 0)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int z = 0; z < dz; z++)
                    {
                        int xIndex = x;
                        int zIndex = Numerical.IntMod(z + zStartIndex, Height);

                        data[xIndex, zIndex] = DefaultValue;
                    }
                }
            }
            else if (dz < 0)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int z = dz; z < 0; z++)
                    {
                        int xIndex = x;
                        int zIndex = Numerical.IntMod(z + zStartIndex, Height);

                        data[xIndex, zIndex] = DefaultValue;
                    }
                }
            }

            zStartIndex = Numerical.IntMod(zStartIndex + dz, Height);
            ZMin += dz;
        }
    }
}

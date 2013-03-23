using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Voxelist.GeometryPrimitives
{
    public struct GeometryPrimitive
    {
        public VertexPositionNormalTexture[] Vertices;
        public int[] Indices;

        /// <summary>
        /// Constructs a new GeometryPrimitive from this original and a scaling factor.
        /// </summary>
        /// <param name="scalingFactor"></param>
        /// <returns></returns>
        public GeometryPrimitive Scale(float scalingFactor)
        {
            GeometryPrimitive output = new GeometryPrimitive();

            output.Indices = new int[Indices.Length];
            output.Vertices = new VertexPositionNormalTexture[Vertices.Length];

            for (int i = 0; i < Indices.Length; i++)
                output.Indices[i] = Indices[i];

            for (int i = 0; i < Vertices.Length; i++)
            {
                output.Vertices[i] = Vertices[i];
                output.Vertices[i].Position *= scalingFactor;
            }

            return output;
        }

        /// <summary>
        /// Translates the given GeometryPrimitive by the specified vector.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public GeometryPrimitive Translate(Vector3 translation)
        {
            GeometryPrimitive output = new GeometryPrimitive();

            output.Indices = new int[Indices.Length];
            output.Vertices = new VertexPositionNormalTexture[Vertices.Length];

            for (int i = 0; i < Indices.Length; i++)
                output.Indices[i] = Indices[i];

            for (int i = 0; i < Vertices.Length; i++)
            {
                output.Vertices[i] = Vertices[i];
                output.Vertices[i].Position += translation;
            }

            return output;
        }

        /// <summary>
        /// Combines an array of GeometryPrimitives into a single primitive.
        /// The order and all characteristics of the vertices are preserved.
        /// </summary>
        /// <param name="primitives"></param>
        /// <returns></returns>
        public static GeometryPrimitive Combine(GeometryPrimitive[] primitives)
        {
            int numVertices = 0;
            int numIndices = 0;

            for (int i = 0; i < primitives.Length; i++)
            {
                numVertices += primitives[i].Vertices.Length;
                numIndices += primitives[i].Indices.Length;
            }

            GeometryPrimitive output = new GeometryPrimitive();

            output.Vertices = new VertexPositionNormalTexture[numVertices];
            output.Indices = new int[numIndices];

            int vertIndex = 0;
            int indIndex = 0;

            for (int i = 0; i < primitives.Length; i++)
            {
                for (int j = 0; j < primitives[i].Vertices.Length; j++)
                    output.Vertices[j + vertIndex] = primitives[i].Vertices[j];

                for (int j = 0; j < primitives[i].Indices.Length; j++)
                    output.Indices[j + indIndex] = primitives[i].Indices[j] + vertIndex;

                vertIndex += primitives[i].Vertices.Length;
                indIndex += primitives[i].Indices.Length;
            }

            return output;
        }

        /// <summary>
        /// Construct a rectangle at the specified location, which displays a
        /// portion of a texture.
        /// </summary>
        /// <param name="center">The center of the rectangle.</param>
        /// <param name="normal">UNIT VECTOR facing directly out of the rectangle.
        /// If you're facing the rectangle (and you want it to be visible)
        /// then this should be pointing at you.</param>
        /// <param name="up">UNIT VECTOR pointing in the direction that should be
        /// "up" on the texture.  Messing this up may flip the texture.</param>
        /// <param name="textureCornerUL">The "upper left corner" of the relevant
        /// part of the texture in UV coordinates.</param>
        /// <param name="textureSize">The "size" of the relevant part of the
        /// texture in UV coordinates.  For clarity, note that the lower left
        /// corner should be textureCornerUL + size.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns></returns>
        public static GeometryPrimitive MakeRectangle(
            Vector3 center, Vector3 normal, Vector3 up,
            Vector2 textureCornerUL, Vector2 textureSize,
            float width, float height)
        {
            GeometryPrimitive quad = new GeometryPrimitive();

            Vector3 left = Vector3.Cross(normal, up);
            Vector3 upperCenter = center + up * (height / 2.0f);

            Vector3 upperLeft = upperCenter + left * (width / 2.0f);
            Vector3 upperRight = upperCenter - left * (width / 2.0f);

            Vector3 lowerLeft = upperLeft - up * height;
            Vector3 lowerRight = upperRight - up * height;

            Vector2 textureUL = new Vector2(textureCornerUL.X, textureCornerUL.Y);
            Vector2 textureUR = new Vector2(textureCornerUL.X + textureSize.X, textureCornerUL.Y);
            Vector2 textureDL = new Vector2(textureCornerUL.X, textureCornerUL.Y + textureSize.Y);
            Vector2 textureDR = new Vector2(textureCornerUL.X + textureSize.X, textureCornerUL.Y + textureSize.Y);

            //with all those preliminaries set up, just define the corners!
            quad.Vertices = new VertexPositionNormalTexture[4];

            quad.Vertices[0] = new VertexPositionNormalTexture(lowerLeft, normal, textureDL);
            quad.Vertices[1] = new VertexPositionNormalTexture(upperLeft, normal, textureUL);
            quad.Vertices[2] = new VertexPositionNormalTexture(lowerRight, normal, textureDR);
            quad.Vertices[3] = new VertexPositionNormalTexture(upperRight, normal, textureUR);

            quad.Indices = new int[] { 0, 1, 2, 2, 1, 3 };

            return quad;
        }

        /// <summary>
        /// This constructs a 3D rectangle (interval in 3-space if you prefer) with the 
        /// specified "minimal corner" and size vector.  Minimal corner means the point where
        /// x, y, and z are all lowest, and size should be a strictly positive vector in all
        /// components.
        /// 
        /// It also associates a texture to it, in UV coordinates, and the texture usage
        /// looks like this:
        /// 
        ///    --------------------
        ///    | BK | T  | BT |   |
        ///    --------------------
        ///    | L  | F  | R  |   |
        ///    --------------------
        ///    
        /// This uses only the specified portion of the texture.  For example, if the relevant
        /// texture portion is the upper-right quarter, textureMinimalCorner should be (0, .5f),
        /// and textureSize should be (.5f, .5f).
        /// 
        /// There are a big pile of optional bools, indicating which (if any) faces to include.
        /// All default to true.
        /// </summary>
        /// <param name="minimalCorner">Corner where x, y, and z are minimal.</param>
        /// <param name="size">Positive vector expressing the x, y, and z size.</param>
        /// <param name="textureMinimalCorner">Where the relevant part of the texture starts, in UV coordinates.</param>
        /// <param name="textureSize">How big the relevant part of the texture is, in UV coordinates.</param>
        /// <returns></returns>
        public static GeometryPrimitive Make3DRectangle(Vector3 minimalCorner, Vector3 size,
            Vector2 textureMinimalCorner, Vector2 textureSize,
            bool includeFrontFace = true, bool includeBackFace = true,
            bool includeTopFace = true, bool includeBottomFace = true,
            bool includeLeftFace = true, bool includeRightFace = true)
        {
            int faceCount = 0;

            if (includeFrontFace) faceCount++;
            if (includeBackFace) faceCount++;
            if (includeTopFace) faceCount++;
            if (includeBottomFace) faceCount++;
            if (includeLeftFace) faceCount++;
            if (includeRightFace) faceCount++;

            //physical dimensions
            float xmin = minimalCorner.X;
            float xmid = minimalCorner.X + (size.X / 2.0f);
            float xmax = minimalCorner.X + size.X;

            float xSize = size.X;

            float ymin = minimalCorner.Y;
            float ymid = minimalCorner.Y + (size.Y / 2.0f);
            float ymax = minimalCorner.Y + size.Y;

            float ySize = size.Y;

            float zmin = minimalCorner.Z;
            float zmid = minimalCorner.Z + (size.Z / 2.0f);
            float zmax = minimalCorner.Z + size.Z;

            float zSize = size.Z;

            //texture dimensions
            float texIncX = textureSize.X * 0.25f; //texture increment x
            float texIncY = textureSize.Y * 0.5f;  //texture increment y

            Vector2 faceTextureSize = new Vector2(texIncX, texIncY);

            //and now for the geometry!
            GeometryPrimitive[] faces = new GeometryPrimitive[faceCount];

            int faceIndex = 0;

            //Front face
            if (includeFrontFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymid, zmin),
                    Vector3.Forward, Vector3.Up,
                    new Vector2(texIncX, texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, ySize);
            }

            //Back face
            if (includeBackFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymid, zmax),
                    Vector3.Backward, Vector3.Up,
                    new Vector2(0, 0) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, ySize);
            }

            //top face
            if (includeTopFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymax, zmid),
                    Vector3.Up, Vector3.Forward,
                    new Vector2(texIncX, 0) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, zSize);
            }

            //bottom face
            if (includeBottomFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymin, zmid),
                    Vector3.Down, Vector3.Forward,
                    new Vector2(2.0f * texIncX, 0) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, zSize);
            }

            //left face
            if (includeLeftFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmin, ymid, zmid),
                    Vector3.Left, Vector3.Up,
                    new Vector2(0, texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    zSize, ySize);
            }

            //right face
            if (includeRightFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmax, ymid, zmid),
                    Vector3.Right, Vector3.Up,
                    new Vector2(2.0f * texIncX, texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    zSize, ySize);
            }

            return GeometryPrimitive.Combine(faces);
        }

        /// <summary>
        /// This is exactly like the Make3DRectangle method, including texture usage,
        /// except that all the faces are looking IN (as would be expected).  So you
        /// can hang this around the camera and that'll look fine.
        /// </summary>
        /// <param name="minimalCorner"></param>
        /// <param name="size"></param>
        /// <param name="textureMinimalCorner"></param>
        /// <param name="textureSize"></param>
        /// <returns></returns>
        public static GeometryPrimitive MakeSkybox(Vector3 minimalCorner, Vector3 size,
            Vector2 textureMinimalCorner, Vector2 textureSize)
        {
            //physical dimensions
            float xmin = minimalCorner.X;
            float xmid = minimalCorner.X + (size.X / 2.0f);
            float xmax = minimalCorner.X + size.X;

            float xSize = size.X;

            float ymin = minimalCorner.Y;
            float ymid = minimalCorner.Y + (size.Y / 2.0f);
            float ymax = minimalCorner.Y + size.Y;

            float ySize = size.Y;

            float zmin = minimalCorner.Z;
            float zmid = minimalCorner.Z + (size.Z / 2.0f);
            float zmax = minimalCorner.Z + size.Z;

            float zSize = size.Z;

            //texture dimensions
            float texIncX = textureSize.X * 0.25f; //texture increment x
            float texIncY = textureSize.Y * 0.5f;  //texture increment y

            Vector2 faceTextureSize = new Vector2(texIncX, texIncY);

            //and now for the geometry!
            GeometryPrimitive[] faces = new GeometryPrimitive[6];

            //Front face
            faces[0] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymid, zmin),
                Vector3.Backward, Vector3.Up,
                new Vector2(texIncX, texIncY) + textureMinimalCorner,
                faceTextureSize,
                xSize, ySize);

            //Back face
            faces[1] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymid, zmax),
                Vector3.Forward, Vector3.Up,
                new Vector2(0, 0) + textureMinimalCorner,
                faceTextureSize,
                xSize, ySize);

            //top face
            faces[2] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymax, zmid),
                Vector3.Down, Vector3.Forward,
                new Vector2(texIncX, 0) + textureMinimalCorner,
                faceTextureSize,
                xSize, zSize);

            //bottom face
            faces[3] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymin, zmid),
                Vector3.Up, Vector3.Forward,
                new Vector2(2.0f * texIncX, 0) + textureMinimalCorner,
                faceTextureSize,
                xSize, zSize);

            //left face
            faces[4] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmin, ymid, zmid),
                Vector3.Right, Vector3.Up,
                new Vector2(0, texIncY) + textureMinimalCorner,
                faceTextureSize,
                zSize, ySize);

            //right face
            faces[5] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmax, ymid, zmid),
                Vector3.Left, Vector3.Up,
                new Vector2(2.0f * texIncX, texIncY) + textureMinimalCorner,
                faceTextureSize,
                zSize, ySize);

            return GeometryPrimitive.Combine(faces);
        }

        /*
        /// <summary>
        /// Construct a new BlockDrawingPrimitive.  Start with the (lex)-least coordinate, then
        /// give a size, all of whose coordinates are positive.  Orientations are consistent with
        /// the naming conventions of the Vector3 class; in particular, the forward face is the one
        /// whose z-coordinate is minimal.
        /// </summary>
        /// <param name="lowerCorner"></param>
        /// <param name="size"></param>
        public static GeometryPrimitive BlockDrawingPrimitive(Vector3 lowerCorner, Vector3 size)
        {
            GeometryPrimitive output = new GeometryPrimitive();

            //first, set up the actual vertices of a block, of which there are 8
            Vector3 LDF = new Vector3(lowerCorner.X, lowerCorner.Y, lowerCorner.Z); //left-down-front
            Vector3 RDF = new Vector3(lowerCorner.X + size.X, lowerCorner.Y, lowerCorner.Z); //right-down-front
            Vector3 LUF = new Vector3(lowerCorner.X, lowerCorner.Y + size.Y, lowerCorner.Z); //left-up-front
            Vector3 RUF = new Vector3(lowerCorner.X + size.X, lowerCorner.Y + size.Y, lowerCorner.Z); //right-up-front

            Vector3 LDB = new Vector3(lowerCorner.X, lowerCorner.Y, lowerCorner.Z + size.Z); //left-down-back
            Vector3 RDB = new Vector3(lowerCorner.X + size.X, lowerCorner.Y, lowerCorner.Z + size.Z); //right-down-back
            Vector3 LUB = new Vector3(lowerCorner.X, lowerCorner.Y + size.Y, lowerCorner.Z + size.Z); //left-up-back
            Vector3 RUB = new Vector3(lowerCorner.X + size.X, lowerCorner.Y + size.Y, lowerCorner.Z + size.Z); //right-up-back

            //next, set up the texture corners we'll use
            Vector2 T1 = new Vector2(0.00f, 0); //top-1, and etc.
            Vector2 T2 = new Vector2(0.25f, 0);
            Vector2 T3 = new Vector2(0.50f, 0);
            Vector2 T4 = new Vector2(0.75f, 0);
            Vector2 T5 = new Vector2(1.00f, 0);

            Vector2 M1 = new Vector2(0.00f, 0.5f); //middle-1, and etc.
            Vector2 M2 = new Vector2(0.25f, 0.5f);
            Vector2 M3 = new Vector2(0.50f, 0.5f);
            Vector2 M4 = new Vector2(0.75f, 0.5f);
            Vector2 M5 = new Vector2(1.00f, 0.5f);

            Vector2 B1 = new Vector2(0.00f, 1); //bottom-1, and etc.
            Vector2 B2 = new Vector2(0.25f, 1);
            Vector2 B3 = new Vector2(0.50f, 1);
            Vector2 B4 = new Vector2(0.75f, 1);
            Vector2 B5 = new Vector2(1.00f, 1);

            //for each square, it goes 0, 1, 2, 2, 1, 3 (plus the start square index, which goes up by 4)
            //so that turns into this:
            output.Indices = new int[] { 0, 1, 2, 2, 1, 3, 4, 5, 6, 6, 5, 7, 8, 9, 10, 10, 9, 11, 12, 13, 14, 14, 13, 15, 16, 17, 18, 18, 17, 19, 20, 21, 22, 22, 21, 23 };

            //the vertices take more work
            output.Vertices = new VertexPositionNormalTexture[24];

            for (int i = 0; i < 24; i++)
                output.Vertices[i] = new VertexPositionNormalTexture();

            //first, for the FRONT face, so the normal is FORWARD
            for (int i = 0; i < 4; i++)
                output.Vertices[i].Normal = Vector3.Forward;

            output.Vertices[0].Position = RDF;
            output.Vertices[1].Position = RUF;
            output.Vertices[2].Position = LDF;
            output.Vertices[3].Position = LUF;

            output.Vertices[0].TextureCoordinate = B2;
            output.Vertices[1].TextureCoordinate = M2;
            output.Vertices[2].TextureCoordinate = B3;
            output.Vertices[3].TextureCoordinate = M3;

            //second, for the BACK face, the normal is BACKWARD
            for (int i = 4; i < 8; i++)
                output.Vertices[i].Normal = Vector3.Backward;

            output.Vertices[4].Position = LDB;
            output.Vertices[5].Position = LUB;
            output.Vertices[6].Position = RDB;
            output.Vertices[7].Position = RUB;

            output.Vertices[4].TextureCoordinate = M1;
            output.Vertices[5].TextureCoordinate = T1;
            output.Vertices[6].TextureCoordinate = M2;
            output.Vertices[7].TextureCoordinate = T2;

            //for the LEFT face...
            for (int i = 8; i < 12; i++)
                output.Vertices[i].Normal = Vector3.Left;

            output.Vertices[8].Position = LDF;
            output.Vertices[9].Position = LUF;
            output.Vertices[10].Position = LDB;
            output.Vertices[11].Position = LUB;

            output.Vertices[8].TextureCoordinate = B1;
            output.Vertices[9].TextureCoordinate = M1;
            output.Vertices[10].TextureCoordinate = B2;
            output.Vertices[11].TextureCoordinate = M2;

            //for the right face...
            for (int i = 12; i < 16; i++)
                output.Vertices[i].Normal = Vector3.Right;

            output.Vertices[12].Position = RDB;
            output.Vertices[13].Position = RUB;
            output.Vertices[14].Position = RDF;
            output.Vertices[15].Position = RUF;

            output.Vertices[12].TextureCoordinate = B3;
            output.Vertices[13].TextureCoordinate = M3;
            output.Vertices[14].TextureCoordinate = B4;
            output.Vertices[15].TextureCoordinate = M4;

            //for the top face!
            for (int i = 16; i < 20; i++)
                output.Vertices[i].Normal = Vector3.Up;

            output.Vertices[16].Position = LUB;
            output.Vertices[17].Position = LUF;
            output.Vertices[18].Position = RUB;
            output.Vertices[19].Position = RUF;

            output.Vertices[16].TextureCoordinate = M2;
            output.Vertices[17].TextureCoordinate = T2;
            output.Vertices[18].TextureCoordinate = M3;
            output.Vertices[19].TextureCoordinate = T3;

            //and finally for the bottom face!
            for (int i = 20; i < 24; i++)
                output.Vertices[i].Normal = Vector3.Down;

            output.Vertices[20].Position = LDB;
            output.Vertices[21].Position = RDB;
            output.Vertices[22].Position = LDF;
            output.Vertices[23].Position = RDF;

            output.Vertices[20].TextureCoordinate = M3;
            output.Vertices[21].TextureCoordinate = T3;
            output.Vertices[22].TextureCoordinate = M4;
            output.Vertices[23].TextureCoordinate = T4;

            return output;
        }*/
    }
}

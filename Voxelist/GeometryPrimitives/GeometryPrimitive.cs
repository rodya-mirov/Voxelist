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
            bool includeFrontFace, bool includeBackFace,
            bool includeTopFace, bool includeBottomFace,
            bool includeLeftFace, bool includeRightFace)
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
            float texIncY = textureSize.Y * 0.25f;  //texture increment y

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
                    new Vector2(texIncX, 2.0f * texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, ySize);
            }

            //Back face
            if (includeBackFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymid, zmax),
                    Vector3.Backward, Vector3.Down,
                    new Vector2(texIncX, 0) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, ySize);
            }

            //top face
            if (includeTopFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymax, zmid),
                    Vector3.Up, Vector3.Forward,
                    new Vector2(texIncX, texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, zSize);
            }

            //bottom face
            if (includeBottomFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmid, ymin, zmid),
                    Vector3.Down, Vector3.Forward,
                    new Vector2(texIncX, 3.0f * texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    xSize, zSize);
            }

            //left face
            if (includeLeftFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmin, ymid, zmid),
                    Vector3.Left, Vector3.Forward,
                    new Vector2(0, texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    ySize, zSize);
            }

            //right face
            if (includeRightFace)
            {
                faces[faceIndex++] = GeometryPrimitive.MakeRectangle(
                    new Vector3(xmax, ymid, zmid),
                    Vector3.Right, Vector3.Forward,
                    new Vector2(2.0f * texIncX, texIncY) + textureMinimalCorner,
                    faceTextureSize,
                    ySize, zSize);
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
            float texIncY = textureSize.Y * 0.25f;  //texture increment y

            Vector2 faceTextureSize = new Vector2(texIncX, texIncY);

            //and now for the geometry!
            GeometryPrimitive[] faces = new GeometryPrimitive[6];

            //Front face
            faces[0] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymid, zmin),
                Vector3.Backward, Vector3.Up,
                new Vector2(texIncX, 2.0f * texIncY) + textureMinimalCorner,
                faceTextureSize,
                xSize, ySize);

            //Back face
            faces[1] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymid, zmax),
                Vector3.Forward, Vector3.Down,
                new Vector2(texIncX, 0) + textureMinimalCorner,
                faceTextureSize,
                xSize, ySize);

            //top face
            faces[2] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymax, zmid),
                Vector3.Down, Vector3.Backward,
                new Vector2(texIncX, texIncY) + textureMinimalCorner,
                faceTextureSize,
                xSize, zSize);

            //bottom face
            faces[3] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmid, ymin, zmid),
                Vector3.Up, Vector3.Forward,
                new Vector2(texIncX, 3.0f * texIncY) + textureMinimalCorner,
                faceTextureSize,
                xSize, zSize);

            //left face
            faces[4] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmin, ymid, zmid),
                Vector3.Right, Vector3.Backward,
                new Vector2(0, texIncY) + textureMinimalCorner,
                faceTextureSize,
                zSize, ySize);

            //right face
            faces[5] = GeometryPrimitive.MakeRectangle(
                new Vector3(xmax, ymid, zmid),
                Vector3.Left, Vector3.Backward,
                new Vector2(2.0f * texIncX, texIncY) + textureMinimalCorner,
                faceTextureSize,
                zSize, ySize);

            return GeometryPrimitive.Combine(faces);
        }
    }
}

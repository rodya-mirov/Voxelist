using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.BlockHandling;
using Voxelist.GeometryPrimitives;
using Voxelist.Utilities;
using Voxelist.Rendering;

namespace Voxelist.Mapping
{
    /// <summary>
    /// Roughly, a chunk is a 3D grid of Cubes.  It can have other behavior as desired, but
    /// this is fundamentally what is going on.
    /// </summary>
    /// <typeparam name="CubeType"></typeparam>
    public class Chunk
    {
        protected BlockHandler handler;
        protected Block[, ,] containedCubes;

        public Chunk(BlockHandler handler)
        {
            this.handler = handler;
            containedCubes = new Block[GameConstants.CHUNK_X_WIDTH + 2, GameConstants.CHUNK_Y_HEIGHT, GameConstants.CHUNK_Z_LENGTH + 2];
        }

        public void OverwriteBlockDataWith(Map map, int chunkX, int chunkZ)
        {
            map.MakeChunkBlocks(chunkX, chunkZ, containedCubes);
            setupDrawingAssistance();
        }

        /// <summary>
        /// Gets the block at the appropriate (local) cube coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Block this[int x, int y, int z]
        {
            get { return containedCubes[x + 1, y, z + 1]; }
        }

        #region Drawing assistance
        private GeometryPrimitive[] combinedPrimitives;
        private int[] combinedVerticesCount, combinedTrianglesCount;
        private bool[] usesTextureIndex;

        private void setupDrawingAssistance()
        {
            combinedPrimitives = new GeometryPrimitive[handler.TotalNumberOfTextures];
            combinedVerticesCount = new int[handler.TotalNumberOfTextures];
            combinedTrianglesCount = new int[handler.TotalNumberOfTextures];
            usesTextureIndex = new bool[handler.TotalNumberOfTextures];

            for (int textureIndex = 0; textureIndex < handler.TotalNumberOfTextures; textureIndex++)
            {
                List<GeometryPrimitive> buildingBlocks = new List<GeometryPrimitive>();

                for (int x = 0; x < GameConstants.CHUNK_X_WIDTH; x++)
                {
                    for (int y = 0; y < GameConstants.CHUNK_Y_HEIGHT; y++)
                    {
                        for (int z = 0; z < GameConstants.CHUNK_Z_LENGTH; z++)
                        {
                            Block relevantBlock = this[x, y, z];

                            if (!handler.IsVisible(relevantBlock) || handler.TextureIndex(relevantBlock) != textureIndex)
                                continue;

                            bool includeFrontFace, includeBackFace;
                            bool includeTopFace, includeBottomFace;
                            bool includeLeftFace, includeRightFace;

                            MakeOcclusionTags(x, y, z,
                                out includeFrontFace, out includeBackFace,
                                out includeTopFace, out includeBottomFace,
                                out includeLeftFace, out includeRightFace);

                            GeometryPrimitive drawingPrimitive = handler.DrawingPrimitive(relevantBlock,
                                includeFrontFace, includeBackFace, includeTopFace, includeBottomFace,
                                includeLeftFace, includeRightFace);

                            buildingBlocks.Add(drawingPrimitive.Translate(new Vector3(x, y, z)));
                        }
                    }
                }

                GeometryPrimitive[] primitivesArray = new GeometryPrimitive[buildingBlocks.Count];
                buildingBlocks.CopyTo(primitivesArray);

                usesTextureIndex[textureIndex] = primitivesArray.Length > 0;

                if (usesTextureIndex[textureIndex])
                {
                    combinedPrimitives[textureIndex] = GeometryPrimitive.Combine(primitivesArray);
                    combinedVerticesCount[textureIndex] = combinedPrimitives[textureIndex].Vertices.Length;
                    combinedTrianglesCount[textureIndex] = combinedPrimitives[textureIndex].Indices.Length / 3;
                }

                if (combinedVerticesCount[textureIndex] == 0)
                    usesTextureIndex[textureIndex] = false;
            }
        }

        private void MakeOcclusionTags(int x, int y, int z,
            out bool includeFrontFace, out bool includeBackFace,
            out bool includeTopFace, out bool includeBottomFace,
            out bool includeLeftFace, out bool includeRightFace)
        {
            if (handler.IsFullAndOpaqueToTheRight(this[x - 1, y, z]))
                includeLeftFace = false;
            else
                includeLeftFace = true;

            if (handler.IsFullAndOpaqueToTheLeft(this[x + 1, y, z]))
                includeRightFace = false;
            else
                includeRightFace = true;

            if (y == 0)
                includeBottomFace = false;
            else if (handler.IsFullAndOpaqueToTheTop(this[x, y - 1, z]))
                includeBottomFace = false;
            else
                includeBottomFace = true;

            if (y + 1 == GameConstants.CHUNK_Y_HEIGHT)
                includeTopFace = true;
            else if (handler.IsFullAndOpaqueToTheBottom(this[x, y + 1, z]))
                includeTopFace = false;
            else
                includeTopFace = true;

            if (handler.IsFullAndOpaqueToTheBack(this[x, y, z - 1]))
                includeFrontFace = false;
            else
                includeFrontFace = true;

            if (handler.IsFullAndOpaqueToTheFront(this[x, y, z + 1]))
                includeBackFace = false;
            else
                includeBackFace = true;
        }
        #endregion

        /// <summary>
        /// Draws this Chunk of blocks at the specified location.
        /// The supplied location should be the minimal corner of
        /// the chunk, accounting for relative chunk positions.
        /// </summary>
        /// <typeparam name="HandlerType"></typeparam>
        /// <param name="chunk"></param>
        /// <param name="drawLocation"></param>
        public void Draw(Vector3 drawLocation, int textureIndex)
        {
            if (Camera.ChunkIsCompletelyOffScreen(drawLocation))
                return;

            if (!usesTextureIndex[textureIndex])
                return;

            BasicEffect drawingEffect = handler.DrawingEffect(textureIndex);

            drawingEffect.World = Matrix.CreateTranslation(drawLocation);
            drawingEffect.Projection = Camera.ProjectionMatrix;
            drawingEffect.View = Camera.ViewMatrix;

            foreach (EffectPass pass in drawingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                drawingEffect.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    combinedPrimitives[textureIndex].Vertices,
                    0, combinedVerticesCount[textureIndex],
                    combinedPrimitives[textureIndex].Indices,
                    0, combinedTrianglesCount[textureIndex]
                    );
            }
        }
    }
}

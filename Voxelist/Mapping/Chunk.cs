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
using Voxelist.Entities;

namespace Voxelist.Mapping
{
    /// <summary>
    /// Roughly, a chunk is a 3D grid of Cubes.  It can have other behavior as desired, but
    /// this is fundamentally what is going on.
    /// </summary>
    /// <typeparam name="CubeType"></typeparam>
    public class Chunk
    {
        private BlockHandler BlockHandler { get; set; }
        private Map Map { get; set; }

        private Block[, ,] containedCubes;
        private List<EntitySchema> entitySchemata;

        public int chunkX { get; private set; }
        public int chunkZ { get; private set; }

        public Chunk(BlockHandler handler, Map map)
        {
            this.Map = map;
            this.BlockHandler = handler;
            containedCubes = new Block[GameConstants.CHUNK_X_WIDTH, GameConstants.CHUNK_Y_HEIGHT, GameConstants.CHUNK_Z_LENGTH];
            entitySchemata = new List<EntitySchema>();
        }

        public void OverwriteChunkDataWith(int chunkX, int chunkZ)
        {
            this.chunkX = chunkX;
            this.chunkZ = chunkZ;

            entitySchemata.Clear();

            Map.MakeChunkData(chunkX, chunkZ, containedCubes, entitySchemata);

            RecalculateVisualGeometry();
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
            get { return containedCubes[x, y, z]; }
            set { containedCubes[x, y, z] = value; }
        }

        public IEnumerable<Entity> MakeGeneratedEntities(EntityBuilder builder, WorldManager manager)
        {
            foreach (EntitySchema schema in entitySchemata)
                yield return builder.MakeEntity(schema, chunkX, chunkZ, manager);
        }

        #region Drawing assistance
        private GeometryPrimitive[] combinedPrimitives;
        private int[] combinedVerticesCount, combinedTrianglesCount;
        private bool[] usesTextureIndex;

        private BoundingBox visualBoundingBox;

        /// <summary>
        /// Recalculates the Visual geometry for the Chunk.  The supplied chunks can be null;
        /// if not, will be used for visual occlusion (if null, will be assumed see-through).
        /// </summary>
        public void RecalculateVisualGeometry()
        {
            Chunk leftChunk, rightChunk, forwardChunk, backwardChunk;
            LoadNeighborChunks(out leftChunk, out rightChunk, out forwardChunk, out backwardChunk);

            GeometryPrimitive[] newCombinedPrimitives = new GeometryPrimitive[BlockHandler.TotalNumberOfTextures];
            int[] newCombinedVerticesCount = new int[BlockHandler.TotalNumberOfTextures];
            int[] newCombinedTrianglesCount = new int[BlockHandler.TotalNumberOfTextures];
            bool[] newUsesTextureIndex = new bool[BlockHandler.TotalNumberOfTextures];

            float visualXMin = GameConstants.CHUNK_X_WIDTH;
            float visualXMax = 0;

            float visualYMin = GameConstants.CHUNK_Y_HEIGHT;
            float visualYMax = 0;

            float visualZMin = GameConstants.CHUNK_Z_LENGTH;
            float visualZMax = 0;

            for (int textureIndex = 0; textureIndex < BlockHandler.TotalNumberOfTextures; textureIndex++)
            {
                List<GeometryPrimitive> buildingBlocks = new List<GeometryPrimitive>();

                for (int x = 0; x < GameConstants.CHUNK_X_WIDTH; x++)
                {
                    for (int y = 0; y < GameConstants.CHUNK_Y_HEIGHT; y++)
                    {
                        for (int z = 0; z < GameConstants.CHUNK_Z_LENGTH; z++)
                        {
                            Block relevantBlock = this[x, y, z];

                            if (!BlockHandler.IsVisible(relevantBlock) || BlockHandler.TextureIndex(relevantBlock) != textureIndex)
                                continue;

                            bool includeFrontFace, includeBackFace;
                            bool includeTopFace, includeBottomFace;
                            bool includeLeftFace, includeRightFace;

                            MakeOcclusionTags(x, y, z,
                                out includeFrontFace, out includeBackFace,
                                out includeTopFace, out includeBottomFace,
                                out includeLeftFace, out includeRightFace,
                                leftChunk, rightChunk,
                                forwardChunk, backwardChunk);

                            bool hasVisibleFace = (includeTopFace || includeBottomFace || includeBackFace || includeFrontFace || includeRightFace || includeLeftFace);

                            if (!hasVisibleFace)
                                continue;

                            visualXMin = MathHelper.Min(visualXMin, x);
                            visualXMax = MathHelper.Max(visualXMax, x + 1);

                            visualYMin = MathHelper.Min(visualYMin, y);
                            visualYMax = MathHelper.Max(visualYMax, y + 1);

                            visualZMin = MathHelper.Min(visualZMin, z);
                            visualZMax = MathHelper.Max(visualZMax, z + 1);

                            GeometryPrimitive drawingPrimitive = BlockHandler.DrawingPrimitive(relevantBlock,
                                includeFrontFace, includeBackFace, includeTopFace, includeBottomFace,
                                includeLeftFace, includeRightFace);

                            if (drawingPrimitive.Vertices.Length > 0)
                                buildingBlocks.Add(drawingPrimitive.Translate(new Vector3(x, y, z)));
                        }
                    }
                }


                GeometryPrimitive[] primitivesArray = new GeometryPrimitive[buildingBlocks.Count];
                buildingBlocks.CopyTo(primitivesArray);

                newUsesTextureIndex[textureIndex] = (primitivesArray.Length > 0);

                if (newUsesTextureIndex[textureIndex])
                {
                    newCombinedPrimitives[textureIndex] = GeometryPrimitive.Combine(primitivesArray);
                    newCombinedVerticesCount[textureIndex] = newCombinedPrimitives[textureIndex].Vertices.Length;
                    newCombinedTrianglesCount[textureIndex] = newCombinedPrimitives[textureIndex].Indices.Length / 3;
                }
            }

            lock (this)
            {
                combinedPrimitives = newCombinedPrimitives;
                combinedTrianglesCount = newCombinedTrianglesCount;
                combinedVerticesCount = newCombinedVerticesCount;
                usesTextureIndex = newUsesTextureIndex;

                visualBoundingBox = new BoundingBox(
                    new Vector3(visualXMin, visualYMin, visualZMin),
                    new Vector3(visualXMax, visualYMax, visualZMax));
            }
        }

        private void LoadNeighborChunks(out Chunk leftChunk, out Chunk rightChunk,
            out Chunk forwardChunk, out Chunk backwardChunk)
        {
            leftChunk = Map.GetChunk(chunkX - 1, chunkZ, false, false);
            rightChunk = Map.GetChunk(chunkX + 1, chunkZ, false, false);
            forwardChunk = Map.GetChunk(chunkX, chunkZ - 1, false, false);
            backwardChunk = Map.GetChunk(chunkX, chunkZ + 1, false, false);
        }

        private void MakeOcclusionTags(int x, int y, int z,
            out bool includeFrontFace, out bool includeBackFace,
            out bool includeTopFace, out bool includeBottomFace,
            out bool includeLeftFace, out bool includeRightFace,
            Chunk leftChunk, Chunk rightChunk, Chunk forwardChunk, Chunk backwardChunk)
        {
            if (x == 0)
            {
                if (leftChunk != null)
                    includeLeftFace = !BlockHandler.IsFullAndOpaqueToTheRight(leftChunk[GameConstants.CHUNK_X_WIDTH - 1, y, z]);
                else
                    includeLeftFace = true;
            }
            else if (BlockHandler.IsFullAndOpaqueToTheRight(this[x - 1, y, z]))
                includeLeftFace = false;
            else
                includeLeftFace = true;

            if (x + 1 == GameConstants.CHUNK_X_WIDTH)
            {
                if (rightChunk != null)
                    includeRightFace = !BlockHandler.IsFullAndOpaqueToTheLeft(rightChunk[0, y, z]);
                else
                    includeRightFace = true;
            }
            else if (BlockHandler.IsFullAndOpaqueToTheLeft(this[x + 1, y, z]))
                includeRightFace = false;
            else
                includeRightFace = true;

            if (y == 0)
                includeBottomFace = false;
            else if (BlockHandler.IsFullAndOpaqueToTheTop(this[x, y - 1, z]))
                includeBottomFace = false;
            else
                includeBottomFace = true;

            if (y + 1 == GameConstants.CHUNK_Y_HEIGHT)
                includeTopFace = true;
            else if (BlockHandler.IsFullAndOpaqueToTheBottom(this[x, y + 1, z]))
                includeTopFace = false;
            else
                includeTopFace = true;

            if (z == 0)
            {
                if (forwardChunk != null)
                    includeFrontFace = !BlockHandler.IsFullAndOpaqueToTheBack(forwardChunk[x, y, GameConstants.CHUNK_Z_LENGTH - 1]);
                else
                    includeFrontFace = true;
            }
            else if (BlockHandler.IsFullAndOpaqueToTheBack(this[x, y, z - 1]))
                includeFrontFace = false;
            else
                includeFrontFace = true;

            if (z + 1 == GameConstants.CHUNK_Z_LENGTH)
            {
                if (backwardChunk != null)
                    includeBackFace = !BlockHandler.IsFullAndOpaqueToTheFront(backwardChunk[x, y, 0]);
                else
                    includeBackFace = true;
            }
            else if (BlockHandler.IsFullAndOpaqueToTheFront(this[x, y, z + 1]))
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
            BoundingBox box = new BoundingBox(
                drawLocation + visualBoundingBox.Min,
                drawLocation + visualBoundingBox.Max);

            if (Camera.IsOffScreen(box))
                return;

            if (!usesTextureIndex[textureIndex])
                return;

            Effect drawingEffect = BlockHandler.DrawingEffect(
                textureIndex,
                Matrix.CreateTranslation(drawLocation),
                Camera.ViewMatrix,
                Camera.ProjectionMatrix);

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

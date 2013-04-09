using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Entities;
using Voxelist.Mapping;
using Voxelist.Rendering;
using Voxelist.GeometryPrimitives;
using Voxelist.BlockHandling;
using Voxelist.Utilities;

namespace VoxelistDemo3
{
    public class MouseOverBlock : Entity
    {
        private Map map;

        public MouseOverBlock(WorldPosition position, WorldManager manager, Map map, PlayerAvatar parent)
            : base(position, manager)
        {
            this.parent = parent;
            this.map = map;
            this.HasFixedPosition = false;
        }

        private PlayerAvatar parent;

        private static GeometryPrimitive entityPrimitive;
        private static int numVertices, numTriangles;

        private static BasicEffect drawingEffect;

        private const float buffer = 0.125f;
        private const float size = 1f - 2f * (buffer);

        private ChunkCoordinate fixedChunkCoordinate;
        private Point3 fixedBlockCoordinate;

        public static void LoadContent(Game game)
        {
            entityPrimitive = GeometryPrimitive.Make3DRectangle(
                Vector3.One * buffer, Vector3.One * size,
                Vector2.Zero, new Vector2(1.0f, 1.0f),
                true, true, true, true, true, true);
            numVertices = 24;
            numTriangles = 12;

            drawingEffect = new BasicEffect(game.GraphicsDevice);

            drawingEffect.TextureEnabled = true;
            drawingEffect.Texture = game.Content.Load<Texture2D>("Textures/Cubes/Dirt");

            //Turns them blackish and shiny, makes em stand out
            drawingEffect.DiffuseColor = Color.BlueViolet.ToVector3();
            drawingEffect.EnableDefaultLighting();
        }

        #region Physics (we're turning them all off :))
        public override BoundingBox BoundingBox
        {
            get { throw new NotImplementedException(); }
        }

        public override BoundingBox VisualBoundingBox
        {
            get
            {
                Vector3 min = Vector3.One * buffer;
                Vector3 max = Vector3.One * (buffer + size);

                return new BoundingBox(
                    min + Position.InChunkPosition,
                    max + Position.InChunkPosition);
            }
        }

        public override float Friction_Induced
        {
            get { throw new NotImplementedException(); }
        }

        protected override Vector3 GroundIntendedVelocity
        {
            get { throw new NotImplementedException(); }
        }

        public override float AirborneDrag_Modifier
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CollidesWithMapGeometry
        {
            get { return false; }
        }

        public override bool IsAWallFor(Entity other)
        {
            return false;
        }
        #endregion

        private const int TestDistance = 40;

        public override void Update(GameTime gametime)
        {
            if (WantVisible)
            {
                //no physics here!
                //base.physicsUpdate(gametime); // :)~~

                int chunkX, chunkZ;
                Block lookAtBlock;
                Point3 blockPosition;
                Face faceTouched;
                bool successful;

                WorldManager.BlockLookedAt(TestDistance, true, true, out lookAtBlock,
                    out chunkX, out chunkZ, out blockPosition, out faceTouched, out successful);

                if (successful)
                {
                    HasFixedPosition = true;

                    switch (faceTouched)
                    {
                        case Face.LEFT:
                            blockPosition.X--;
                            break;

                        case Face.RIGHT:
                            blockPosition.X++;
                            break;

                        case Face.TOP:
                            blockPosition.Y++;
                            break;

                        case Face.BOTTOM:
                            blockPosition.Y--;
                            break;

                        case Face.FRONT:
                            blockPosition.Z--;
                            break;

                        case Face.BACK:
                            blockPosition.Z++;
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    fixedChunkCoordinate = new ChunkCoordinate(chunkX, chunkZ);
                    fixedBlockCoordinate = blockPosition;
                    HelperMethods.FixCoordinates(ref fixedChunkCoordinate, ref fixedBlockCoordinate);

                    this.Position = new WorldPosition(fixedChunkCoordinate.X, fixedChunkCoordinate.Z,
                        fixedBlockCoordinate.X, fixedBlockCoordinate.Y, fixedBlockCoordinate.Z);
                }
                else
                {
                    HasFixedPosition = false;
                }
            }
        }

        public bool HasFixedPosition { get; private set; }
        public override bool IsVisible { get { return HasFixedPosition && WantVisible; } }
        public bool WantVisible { get; set; }

        public override void Draw(GameTime gametime)
        {
            drawingEffect.World = Matrix.CreateTranslation(Camera.objectTranslation(Position));

            drawingEffect.View = Camera.ViewMatrix;
            drawingEffect.Projection = Camera.ProjectionMatrix;

            foreach (EffectPass pass in drawingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                drawingEffect.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    entityPrimitive.Vertices,
                    0, numVertices,
                    entityPrimitive.Indices,
                    0, numTriangles
                    );
            }
        }

        public void SaveFixedBlock()
        {
            if (!HasFixedPosition)
                return;

            map.ChangeBlock(fixedChunkCoordinate.X, fixedChunkCoordinate.Z,
                fixedBlockCoordinate.X, fixedBlockCoordinate.Y, fixedBlockCoordinate.Z,
                new Block(1));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Mapping;
using Voxelist.BlockHandling;
using Voxelist.Entities;
using Voxelist.Utilities;

namespace Voxelist.Rendering
{
    public abstract class WorldManager : DrawableGameComponent
    {
        private Map Map { get; set; }
        private BlockHandler BlockHandler { get; set; }
        private EntityBuilder EntityBuilder { get; set; }

        private Skybox Skybox { get; set; }

        public WorldManager(Game game, Map map, BlockHandler handler, EntityBuilder entityBuilder, string SkyboxTextureLocation)
            : base(game)
        {
            this.Map = map;

            this.BlockHandler = handler;
            this.EntityBuilder = entityBuilder;

            this.Skybox = new Skybox(SkyboxTextureLocation);
        }

        private Queue<Entity> generatedEntities = new Queue<Entity>();

        public IEnumerable<Entity> Entities()
        {
            foreach (Entity entity in generatedEntities)
                yield return entity;

            foreach (Entity entity in ManualEntities())
                yield return entity;
        }

        protected virtual IEnumerable<Entity> ManualEntities()
        {
            yield break;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            BlockHandler.LoadContent(Game);
            EntityBuilder.LoadContent(Game);

            Map.LoadContent(Game);
            Skybox.LoadContent(Game);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Update(gameTime);
            Map.Update(gameTime);
            ManageGeneratedEntities(gameTime);

            //handle extra
            foreach (Entity entity in ManualEntities())
                entity.Update(gameTime);
        }

        private Queue<Entity> backupEntityQueue = new Queue<Entity>();

        private void ManageGeneratedEntities(GameTime gameTime)
        {
            backupEntityQueue.Clear();

            foreach (Entity entity in generatedEntities)
            {
                if (entity.ShouldAutoDespawn())
                {
                    //don't update, and don't put it back in the queue
                    //just let it handle its despawn stuff
                    entity.AutoDespawn();
                }
                else
                {
                    //don't despawn
                    //update and add it back to the queue
                    entity.Update(gameTime);
                    backupEntityQueue.Enqueue(entity);
                }
            }

            Numerical.Swap(ref generatedEntities, ref backupEntityQueue);

            //add new managed entities
            foreach (Entity entity in Map.GenerateAllAvailableEntities(EntityBuilder, this))
            {
                int cx = entity.Position.chunkX;
                int cz = entity.Position.chunkZ;

                generatedEntities.Enqueue(entity);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Map.Dispose();
        }

        public int CenterChunkX { get { return Map.CenterChunkX; } }
        public int CenterChunkZ { get { return Map.CenterChunkZ; } }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Map.Draw();

            foreach (Entity entity in Entities())
                entity.Draw(gameTime);

            if (Skybox != null)
                Skybox.Draw();
        }

        #region Map Method Forwarding
        /// <summary>
        /// Determines whether the given chunk coordinates dictate
        /// that an Entity is "far away," in terms of the Map properties.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public bool EntityFarAway(int chunkX, int chunkZ)
        {
            int dist = Math.Max(
                Math.Abs(chunkX - CenterChunkX),
                Math.Abs(chunkZ - CenterChunkZ));

            return dist >= Map.EntityDespawnRadius;
        }

        /// <summary>
        /// Enumerates the blocks in the Map which intersect the given (chunk-adjusted) boundingbox.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public IEnumerable<Collider> IntersectingBlocks(int chunkX, int chunkZ, BoundingBox boundingBox)
        {
            foreach (Collider block in Map.IntersectingBlocks(chunkX, chunkZ, boundingBox))
                yield return block;
        }

        /// <summary>
        /// This takes as input a maximum distance, then finds the first Block impacted
        /// by the Camera's Forward Ray on this map, if any.  Return types are through
        /// a large number of "out" parameters.  Note it will never return the
        /// cell which contains the start of the Ray.
        /// 
        /// This is just a convenient default parameter set for the other BlockLookedAt
        /// method.  The effect and output are the same as filling in the arguments with
        /// information from Camera.
        /// 
        /// Note: if successful is FALSE, the returned data will be garbage (since it's
        /// not easily nullable).  So be aware of that.
        /// </summary>
        /// <param name="maxDistance">The maximum distance along the ray that collisions
        /// will be considered.  Uses the MAX-norm.</param>
        /// <param name="requireVisible">Whether or not to skip invisible blocks.</param>
        /// <param name="requireImpassable">Whether or not to skip passable blocks.</param>
        /// <param name="foundBlock">The block that was actually found.</param>
        /// <param name="chunkX">The chunk(X) position we end on.</param>
        /// <param name="chunkZ">The chunk(Z) position we end on.</param>
        /// <param name="blockPosition">The in-chunk (integer) position of the block that was found.</param>
        /// <param name="faceTouched">The face that was first touched by the Ray.</param>
        /// <param name="successful">Whether ot not anything was found</param>
        public void BlockLookedAt(int maxDistance, bool requireVisible, bool requireImpassable,
            out Block foundBlock, out int chunkX, out int chunkZ, out Point3 blockPosition,
            out Face faceTouched, out bool successful)
        {
            chunkX = Camera.ChunkX;
            chunkZ = Camera.ChunkZ;

            Map.BlockLookedAt(ref chunkX, ref chunkZ, Camera.ForwardRay, maxDistance,
                requireVisible, requireImpassable,
                out foundBlock, out blockPosition, out faceTouched,
                out successful);
        }

        /// <summary>
        /// This takes as input a position and a Ray and a maximum distance, then finds
        /// the first Block impacted by the Ray on this map, if any.  Return types are
        /// through a large number of "out" parameters.  Note it will never return the
        /// cell which contains the start of the Ray.
        /// 
        /// Note: if successful is FALSE, the returned data will be garbage (since it's
        /// not easily nullable).  So be aware of that.
        /// </summary>
        /// <param name="chunkX">The chunk(X) position to start the Ray from.  This will be
        /// set to the chunk(X) position we end on.</param>
        /// <param name="chunkZ">The chunk(Z) position to start the Ray from.  This will be
        /// set to the chunk(Z) position we end on.</param>
        /// <param name="lookRay">The Ray to look along.</param>
        /// <param name="maxDistance">The maximum distance along the ray that collisions
        /// will be considered.  Uses the MAX distance.</param>
        /// <param name="requireVisible">Whether or not to skip invisible blocks.</param>
        /// <param name="requireImpassable">Whether or not to skip passable blocks.</param>
        /// <param name="foundBlock">The block that was actually found.</param>
        /// <param name="blockPosition">The in-chunk (integer) position of the block that was found.</param>
        /// <param name="faceTouched">The face that was first touched by the Ray.</param>
        /// <param name="successful">Whether ot not anything was found</param>
        public void BlockLookedAt(ref int chunkX, ref int chunkZ, Ray lookRay, int maxDistance,
            bool requireVisible, bool requireImpassable,
            out Block foundBlock, out Point3 blockPosition,
            out Face faceTouched, out bool successful)
        {
            Map.BlockLookedAt(ref chunkX, ref chunkZ, lookRay, maxDistance, requireVisible, requireImpassable,
                out foundBlock, out blockPosition, out faceTouched, out successful);
        }
        #endregion
    }
}

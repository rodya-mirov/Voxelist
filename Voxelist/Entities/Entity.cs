using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Rendering;
using Voxelist.Mapping;
using Voxelist.Utilities;
using Voxelist.GeometryPrimitives;

namespace Voxelist.Entities
{
    public abstract class Entity
    {
        public WorldManager WorldManager { get; protected set; }

        protected Entity(WorldPosition position, WorldManager manager)
        {
            this.WorldManager = manager;
            this.Position = position;

            this.GroundFrictionScale_Experienced = 100f;
            this.GroundFrictionVelocity_Experienced = Vector3.Zero;

            this.DragVelocityExperienced = Vector3.Zero;
        }

        #region Auto-Spawning and -Despawning
        public void AutoSpawn()
        {
            WorldManager.AddEntityToCache(this);
        }

        public void AutoDespawn()
        {
            WorldManager.RemoveEntityFromCache(this);
        }

        public bool ShouldAutoDespawn()
        {
            int chunkX = Position.chunkX;
            int chunkZ = Position.chunkZ;

            return WorldManager.EntityFarAway(chunkX, chunkZ);
        }
        #endregion

        private WorldPosition _position;

        /// <summary>
        /// This represents a "position" for the Entity, including
        /// chunk position.  It's up to the implementer, and not
        /// terribly important, whether this is a center or favorite
        /// corner or favorite unrelated point.
        /// 
        /// What IS important is that shifting this position by a certain
        /// vector makes sense, and that it results in shifting the
        /// BoundingBox property by that same amount.  This fact is
        /// intrinsic to the physics calculations.
        /// </summary>
        public WorldPosition Position
        {
            get { return _position; }
            protected set
            {
                _position = value;
            }
        }

        /// <summary>
        /// When following this target, you don't necessarily want to sit
        /// the camera right at the Position.  This allows you to separate
        /// the two fields.
        /// 
        /// Default behavior is to make it equal to Position.
        /// </summary>
        public virtual WorldPosition CameraFollowPosition { get { return Position; } }

        public abstract void Update(GameTime gametime);

        #region Physics
        /// <summary>
        /// This represents a "bounding box" for the Entity, not
        /// including chunk position.  It is assumed that all the coordinates
        /// of the BoundingBox are in the same Chunk as the Position (which
        /// does have chunk coordinates).  It's OK to cross chunk borders,
        /// of course, which results in BoundingBox coordinates outside
        /// the usual inChunk range (e.g. negative coordinates, or greater
        /// than CHUNK_WIDTH, etc.)
        /// </summary>
        public abstract BoundingBox BoundingBox { get; }

        /// <summary>
        /// This indicates whether or not to keep experiencing "friction" when you're
        /// in mid-air.  It's unrealistic, but in practice, if this is false, it's really
        /// hard to jump onto things.  When this is true, you can move around in midair
        /// as if you were in the ground (subject to the usual forces of course).
        /// </summary>
        protected virtual bool RetainFrictionInAir { get { return false; } }

        /// <summary>
        /// Whether or not to use your intended velocity in midair.  This is sort of a weak
        /// form of "RetainFrictionInAir," and is probably enough to get you over walls,
        /// without making jumping feel too "in control."
        /// </summary>
        protected virtual bool UseIntendedVelocityInMidAir { get { return true; } }

        protected bool XCollidedLeft { get; set; }
        protected bool YCollidedDown { get; set; }
        protected bool ZCollidedForward { get; set; }

        protected bool XCollidedRight { get; set; }
        protected bool YCollidedUp { get; set; }
        protected bool ZCollidedBackward { get; set; }

        /// <summary>
        /// Whether or not this Entity is stopped by map objects and passability.
        /// Default is always true.
        /// </summary>
        public virtual bool CollidesWithMapGeometry { get { return true; } }

        /// <summary>
        /// Whether or not this Entity has PHYSICS interactions with any other Entity.
        /// This is a voluntary flag and defaults to true, but setting this to false
        /// (when appropriate) will dramatically improve performance.
        /// </summary>
        public virtual bool HasPhysicsInteractions { get { return true; } }

        /// <summary>
        /// Whether or not this object acts as a wall for the given other object
        /// (that is, if other tries to run through this, is it stopped?)
        /// 
        /// Default is:
        ///     other.CollidesWithMapGeometry
        ///        && other.HasPhysicsInteractions
        ///        && other != this
        ///     
        /// Try to maintain the following:
        ///     ! this.IsAWallFor(this)
        ///     this.IsAWallFor(other) == other.IsAWallFor(this)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool IsAWallFor(Entity other)
        {
            return other.CollidesWithMapGeometry && other.HasPhysicsInteractions && other != this;
        }

        protected Vector3 Acceleration { get; set; }

        protected Vector3 Velocity { get; set; }

        protected abstract Vector3 GroundIntendedVelocity { get; }

        protected bool OnGround { get { return YCollidedDown; } }

        /// <summary>
        /// In meters/sec^2.
        /// 
        /// The default is (0, -9.8f, 0), or rather,
        /// 9.8 m/s/s straight down.  This is the usual
        /// gravity acceleration.
        /// </summary>
        protected virtual Vector3 GravityAcceleration
        {
            get { return new Vector3(0, -9.8f, 0); }
        }

        protected float GroundFrictionScale_Experienced
        {
            get;
            private set;
        }

        public virtual float GroundFrictionModifier
        {
            get { return 1; }
        }

        protected Vector3 GroundFrictionVelocity_Experienced
        {
            get;
            private set;
        }

        public abstract float Friction_Induced { get; }
        public virtual Vector3 FrictionVelocity_Induced { get { return Velocity; } }

        protected float AirborneDrag
        {
            get { return .2f; }
        }

        public virtual float AirborneDrag_Modifier
        {
            get { return 1; }
        }

        protected Vector3 DragVelocityExperienced
        {
            get;
            private set;
        }

        protected void physicsUpdate(GameTime gametime)
        {
            physicsUpdate_Position(gametime);
            physicsUpdate_Velocity(gametime);
            physicsUpdate_Acceleration(gametime);
        }

        private void physicsUpdate_Acceleration(GameTime gametime)
        {
            Acceleration = Vector3.Zero;

            Acceleration += GravityAcceleration;

            #region Drag (always occurs)
            float dragScale = AirborneDrag * AirborneDrag_Modifier;
            if (dragScale * gametime.ElapsedGameTime.TotalSeconds > .8)
                dragScale = (float)(.8 / gametime.ElapsedGameTime.TotalSeconds);

            Vector3 baseDragContribution = DragVelocityExperienced - Velocity;

            if (UseIntendedVelocityInMidAir)
                baseDragContribution += GroundIntendedVelocity;

            Acceleration += baseDragContribution * dragScale;
            #endregion

            #region Friction (only occurs on ground, or with the special tag)
            if (OnGround || RetainFrictionInAir)
            {
                Vector3 frictionContribution = GroundIntendedVelocity;
                frictionContribution += GroundFrictionVelocity_Experienced - Velocity;

                float frictionScale = GroundFrictionScale_Experienced * GroundFrictionModifier;

                if (frictionScale * gametime.ElapsedGameTime.TotalSeconds > .8)
                    frictionScale = (float)(.8 / gametime.ElapsedGameTime.TotalSeconds);

                Acceleration += frictionContribution * frictionScale;
            }
            #endregion
        }

        private void physicsUpdate_Velocity(GameTime gametime)
        {
            Velocity += Acceleration * (float)(gametime.ElapsedGameTime.TotalSeconds);
        }

        private void physicsUpdate_Position(GameTime gametime)
        {
            Vector3 intendedChange = Velocity * (float)(gametime.ElapsedGameTime.TotalSeconds);

            if (HasPhysicsInteractions)
                WorldManager.RemoveEntityFromCache(this);

            LoadPossibleEntityCollisions(intendedChange);
            MoveAndResolveCollisions(intendedChange); //Position actually changes here

            if (HasPhysicsInteractions)
                WorldManager.AddEntityToCache(this);
        }

        #region Collision Resolution
        private Queue<Collider> possibleEntityCollisions = new Queue<Collider>();

        /// <summary>
        /// Resolve collisions with other objects. That is, given a
        /// change vector (intendedChange), reduce this vector as needed
        /// to give the maximum amount of allowable movement without colliding
        /// with anything.
        /// 
        /// Also repairs the physics_velocity wrt the collisions; eg. if you
        /// slam into a wall, you lose your forward momentum.
        /// 
        /// It assumes (quite reasonably!) that this entity is not currently
        /// intersecting any map geometry or other Entities.
        /// 
        /// It also assumes the FPS is high, so the time delta is small, so we
        /// can safely separate the directions into three separate
        /// transformations.
        /// </summary>
        /// <param name="intendedChange"></param>
        /// <returns></returns>
        private void MoveAndResolveCollisions(Vector3 intendedChange)
        {
            bool StartedOnGround = OnGround;

            Vector3 originalIntendedChange = intendedChange;
            Vector3 pv = Velocity;

            float relevantFriction;
            Vector3 frictionVelocity;

            ResetCollisionTrackers();

            //now resolve lateral movement (x)
            bool collidedX = false;
            intendedChange.X = DetectAndFixCollisions(BoundingBox, intendedChange.X, ref collidedX, Dimension.X,
                out relevantFriction, out frictionVelocity);

            if (collidedX)
            {
                pv.X = 0;
                if (originalIntendedChange.X <= 0)
                    XCollidedLeft = true;
                if (originalIntendedChange.X >= 0)
                    XCollidedRight = true;
            }

            Position += new Vector3(intendedChange.X, 0, 0);

            //...and lateral movement (z)
            bool collidedZ = false;
            intendedChange.Z = DetectAndFixCollisions(BoundingBox, intendedChange.Z, ref collidedZ, Dimension.Z,
                out relevantFriction, out frictionVelocity);

            if (collidedZ)
            {
                pv.Z = 0;
                if (originalIntendedChange.Z <= 0)
                    ZCollidedForward = true;
                if (originalIntendedChange.Z >= 0)
                    ZCollidedBackward = true;
            }

            Position += new Vector3(0, 0, intendedChange.Z);

            //and finally do any y-changes
            bool collidedY = false;
            intendedChange.Y = DetectAndFixCollisions(BoundingBox, intendedChange.Y, ref collidedY, Dimension.Y,
                out relevantFriction, out frictionVelocity);

            if (collidedY)
            {
                pv.Y = 0;
                if (originalIntendedChange.Y <= 0)
                {
                    YCollidedDown = true;

                    GroundFrictionScale_Experienced = relevantFriction;
                    GroundFrictionVelocity_Experienced = frictionVelocity;
                }
                if (originalIntendedChange.Y >= 0)
                    YCollidedUp = true;
            }

            Position += new Vector3(0, intendedChange.Y, 0);

            Velocity = pv;
        }

        private void LoadPossibleEntityCollisions(Vector3 intendedChange)
        {
            possibleEntityCollisions.Clear();

            if (HasPhysicsInteractions)
            {
                BoundingBox stretchedBox = HelperMethods.StretchBox(BoundingBox, intendedChange);

                var touchedEntities = WorldManager.PossibleTouchedEntities(this, stretchedBox);

                foreach (Entity other in touchedEntities)
                {
                    BoundingBox relativeBox = other.BoundingBox;
                    Vector3 translation = new Vector3(
                        (other.Position.chunkX - Position.chunkX) * GameConstants.CHUNK_X_WIDTH,
                        0,
                        (other.Position.chunkZ - Position.chunkZ) * GameConstants.CHUNK_Z_LENGTH);
                    relativeBox = new BoundingBox(relativeBox.Min + translation, relativeBox.Max + translation);

                    if (other.IsAWallFor(this) && relativeBox.Intersects(stretchedBox))
                        possibleEntityCollisions.Enqueue(new Collider(other));
                }
            }
        }

        private void ResetCollisionTrackers()
        {
            XCollidedLeft = false;
            XCollidedRight = false;
            YCollidedDown = false;
            YCollidedUp = false;
            ZCollidedBackward = false;
            ZCollidedForward = false;
        }

        private IEnumerable<Collider> Collisions(BoundingBox boundingBox)
        {
            if (CollidesWithMapGeometry)
            {
                foreach (Collider box in WorldManager.IntersectingBlocks(Position.chunkX, Position.chunkZ, boundingBox))
                    yield return box;
            }

            foreach (Collider collider in possibleEntityCollisions)
                yield return collider;
        }

        /// <summary>
        /// Detects map collisions.  The first three parameters describe the position and boundingbox
        /// of the object of interest.  The last parameter describes the intended change vector.  This
        /// processes possible collisions with the map data and changes that vector to the maximum
        /// amount allowable without causing any collisions.
        /// 
        /// The output is the maximum allowable change (in line with the input and the collisions).
        /// Also keeps track of whether a collision of each type actually occurred.
        /// </summary>
        /// <param name="chunkX">Chunk coordinate for perspective on the bounding box.</param>
        /// <param name="chunkZ">Chunk coordinate for perspective on the bounding box.</param>
        /// <param name="unmovedBox">The boundingBox for the object in its current position.</param>
        /// <param name="originalIntendedChange">The amount we're hoping to move.</param>
        /// <param name="collided">Set to true if a collision occurs.  Otherwise unchanged.</param>
        /// <param name="movementDimension">Which dimension of movement is currently being considered.</param>
        /// <param name="effectiveFriction">The amount of friction the object in question will be experiencing.</param>
        /// <returns></returns>
        private float DetectAndFixCollisions(BoundingBox unmovedBox, float originalIntendedChange, ref bool collided,
            Dimension movementDimension, out float effectiveFriction, out Vector3 frictionVelocity)
        {
            float relevantChange = originalIntendedChange;
            effectiveFriction = 0;
            frictionVelocity = Vector3.Zero;

            if (relevantChange == 0)
                return originalIntendedChange;

            float SIGNED_EPSILON = GameConstants.PHYSICS_COLLISION_EPSILON;
            if (relevantChange < 0) SIGNED_EPSILON = -SIGNED_EPSILON;

            Vector3 changeVector;
            switch (movementDimension)
            {
                case Dimension.X: changeVector = Vector3.UnitX; break;
                case Dimension.Y: changeVector = Vector3.UnitY; break;
                case Dimension.Z: changeVector = Vector3.UnitZ; break;
                default: throw new NotImplementedException();
            }
            changeVector *= relevantChange + SIGNED_EPSILON / 2.0f;
            BoundingBox stretchedBox = HelperMethods.StretchBox(unmovedBox, changeVector);

            foreach (Collider collider in Collisions(stretchedBox))
            {
                BoundingBox blockBounds = collider.GetRelativeBoundingBox(Position.chunkX, Position.chunkZ);

#if DEBUG
                if (blockBounds.Intersects(unmovedBox))
                {
                    if (this == collider.collidedObject)
                        throw new NotFiniteNumberException();

                    throw new NotImplementedException();
                }
#endif

                if (!blockBounds.Intersects(stretchedBox))
                    continue;

                collided = true;

                effectiveFriction = collider.Friction;
                frictionVelocity = collider.FrictionVelocity;

                if (relevantChange > 0)
                {
                    switch (movementDimension)
                    {
                        case Dimension.X:
                            stretchedBox.Max.X = blockBounds.Min.X - SIGNED_EPSILON;
                            relevantChange = stretchedBox.Max.X - unmovedBox.Max.X;
                            break;

                        case Dimension.Y:
                            stretchedBox.Max.Y = blockBounds.Min.Y - SIGNED_EPSILON;
                            relevantChange = stretchedBox.Max.Y - unmovedBox.Max.Y;
                            break;

                        case Dimension.Z:
                            stretchedBox.Max.Z = blockBounds.Min.Z - SIGNED_EPSILON;
                            relevantChange = stretchedBox.Max.Z - unmovedBox.Max.Z;
                            break;

                        default: throw new NotImplementedException();
                    }
                }
                else
                {
                    switch (movementDimension)
                    {
                        case Dimension.X:
                            stretchedBox.Min.X = blockBounds.Max.X - SIGNED_EPSILON;
                            relevantChange = stretchedBox.Min.X - unmovedBox.Min.X;
                            break;

                        case Dimension.Y:
                            stretchedBox.Min.Y = blockBounds.Max.Y - SIGNED_EPSILON;
                            relevantChange = stretchedBox.Min.Y - unmovedBox.Min.Y;
                            break;

                        case Dimension.Z:
                            stretchedBox.Min.Z = blockBounds.Max.Z - SIGNED_EPSILON;
                            relevantChange = stretchedBox.Min.Z - unmovedBox.Min.Z;
                            break;

                        default: throw new NotImplementedException();
                    }
                }
            }

            if (relevantChange * originalIntendedChange < 0)
                relevantChange = 0;

            return relevantChange;
        }
        #endregion

        #endregion

        #region Drawing
        public abstract bool IsVisible { get; }

        /// <summary>
        /// Only called if IsVisible is true.
        /// </summary>
        public abstract BoundingBox VisualBoundingBox { get; }

        public bool ShouldBeDrawn()
        {
            if (!IsVisible)
                return false;

            BoundingBox visualBoundingBox = VisualBoundingBox;
            Vector3 translation = new Vector3(
                GameConstants.CHUNK_X_WIDTH * (Camera.ChunkX - Position.chunkX),
                0,
                GameConstants.CHUNK_Z_LENGTH * (Camera.ChunkZ - Position.chunkZ));

            visualBoundingBox = new BoundingBox(
                visualBoundingBox.Min - translation,
                visualBoundingBox.Max - translation);

            return !Camera.IsOffScreen(visualBoundingBox);
        }

        public abstract Vector3 DrawingOffset { get; }
        public abstract DrawType DrawingType { get; }
        public abstract Texture2D DrawableTexture { get; }

        public virtual GeometryPrimitive DrawableGeometryPrimitive { get { throw new NotImplementedException(); } }
        public virtual Model DrawableModel { get { throw new NotImplementedException(); } }

        public enum DrawType { GeometryPrimitive, Model }
        #endregion
    }
}

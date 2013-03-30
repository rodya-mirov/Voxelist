using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxelist.Rendering;
using Voxelist.Mapping;
using Voxelist.Utilities;

namespace Voxelist.Entities
{
    public abstract class Entity
    {
        public WorldManager WorldManager { get; protected set; }
        public Map Map { get { return WorldManager.Map; } }

        protected Entity(WorldPosition position, WorldManager manager)
        {
            this.WorldManager = manager;
            this.Position = position;

            this.GroundFrictionScale_Experienced = 100f;
            this.GroundFrictionVelocity_Experienced = Vector3.Zero;
        }


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
            protected set { _position = value; }
        }
        private WorldPosition _position;

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
        /// This is the amount of regular physical obstacles which the
        /// Entity will naturally step over without need for other upward
        /// forces.  0 will appear to be sliding, and will get stuck on any
        /// obstacle; the "right" step size should depend on the size and
        /// nature of the Entity.
        /// 
        /// Note the "up-stepping" will only occur if the Entity is currently
        /// on the ground.
        /// </summary>
        protected abstract float UpStepSize { get; }

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
        /// Whether or not this object acts as a wall for the given other object
        /// (that is, if other tries to run through this, is it stopped?)
        /// 
        /// Default is:
        ///     other.CollidesWithMapGeometry && other != this;
        ///     
        /// Try to maintain the following:
        ///     ! this.IsAWallFor(this)
        ///     this.IsAWallFor(other) == other.IsAWallFor(this)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool IsAWallFor(Entity other)
        {
            return other.CollidesWithMapGeometry && other != this;
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

            Vector3 dragContribution = AirborneDrag * (-Velocity);
            Acceleration += dragContribution;

            if (OnGround)
            {
                Vector3 frictionContribution = GroundIntendedVelocity;
                frictionContribution += GroundFrictionVelocity_Experienced - Velocity;

                float frictionScale = GroundFrictionScale_Experienced * GroundFrictionModifier;

                if (frictionScale * gametime.ElapsedGameTime.TotalSeconds >= .8)
                    frictionScale = (float)(.8 / gametime.ElapsedGameTime.TotalSeconds);

                Acceleration += frictionContribution * frictionScale;
            }
        }

        private void physicsUpdate_Velocity(GameTime gametime)
        {
            Velocity += Acceleration * (float)(gametime.ElapsedGameTime.TotalSeconds);
        }

        private void physicsUpdate_Position(GameTime gametime)
        {
            Vector3 intendedChange = Velocity * (float)(gametime.ElapsedGameTime.TotalSeconds);

            MoveAndResolveCollisions(intendedChange);
        }

        protected bool UseStep { get { return true; } }

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

            float upstep = UpStepSize;
            bool upStepHitCeiling = false;

            float relevantFriction;
            Vector3 frictionVelocity;

            ResetCollisionTrackers();

            //first step up
            if (UseStep && StartedOnGround)
            {
                upstep = DetectAndFixCollisions(BoundingBox, upstep, ref upStepHitCeiling, Dimension.Y,
                    out relevantFriction, out frictionVelocity);
                Position += new Vector3(0, upstep, 0);
            }

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

            float downstep = -upstep;
            bool downStepHitFloor = false;

            //now step down..
            if (UseStep && StartedOnGround)
            {
                downstep = DetectAndFixCollisions(BoundingBox, downstep, ref downStepHitFloor, Dimension.Y,
                    out relevantFriction, out frictionVelocity);
                Position += new Vector3(0, downstep, 0);
            }

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

        private void ResetCollisionTrackers()
        {
            XCollidedLeft = false;
            XCollidedRight = false;
            YCollidedDown = false;
            YCollidedUp = false;
            ZCollidedBackward = false;
            ZCollidedForward = false;
        }

        private IEnumerable<Collider> Collisions(BoundingBox currentBoundingBox)
        {
            if (CollidesWithMapGeometry)
            {
                foreach (Collider box in Map.IntersectingBlocks(Position.chunkX, Position.chunkZ, currentBoundingBox))
                    yield return box;
            }

            foreach (Entity other in WorldManager.Entities())
            {
                if (other.IsAWallFor(this))
                    yield return new Collider(other, Position.chunkX, Position.chunkZ);
            }
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

            Vector3 boxmin = unmovedBox.Min;
            Vector3 boxmax = unmovedBox.Max;

            if (relevantChange > 0)//if the relevant direction is POSITIVE...
            {
                switch (movementDimension)//then affect the relevant maximum
                {
                    case Dimension.X: boxmax.X += relevantChange + GameConstants.PHYSICS_COLLISION_EPSILON / 2.0f; break;
                    case Dimension.Y: boxmax.Y += relevantChange + GameConstants.PHYSICS_COLLISION_EPSILON / 2.0f; break;
                    case Dimension.Z: boxmax.Z += relevantChange + GameConstants.PHYSICS_COLLISION_EPSILON / 2.0f; break;
                    default: throw new NotImplementedException();
                }
            }
            else //if the relevant direction is NEGATIVE...
            {
                switch (movementDimension)//then affect the relevant minimum
                {
                    case Dimension.X: boxmin.X += relevantChange - GameConstants.PHYSICS_COLLISION_EPSILON / 2.0f; break;
                    case Dimension.Y: boxmin.Y += relevantChange - GameConstants.PHYSICS_COLLISION_EPSILON / 2.0f; break;
                    case Dimension.Z: boxmin.Z += relevantChange - GameConstants.PHYSICS_COLLISION_EPSILON / 2.0f; break;
                    default: throw new NotImplementedException();
                }
            }

            BoundingBox stretchedBox = new BoundingBox(boxmin, boxmax);

            foreach (Collider collider in Collisions(stretchedBox))
            {
                BoundingBox blockBounds = collider.BoundingBox;

                if (blockBounds.Intersects(unmovedBox))
                    throw new NotImplementedException();

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
                            stretchedBox.Max.X = blockBounds.Min.X - GameConstants.PHYSICS_COLLISION_EPSILON;
                            relevantChange = stretchedBox.Max.X - unmovedBox.Max.X;
                            break;

                        case Dimension.Y:
                            stretchedBox.Max.Y = blockBounds.Min.Y - GameConstants.PHYSICS_COLLISION_EPSILON;
                            relevantChange = stretchedBox.Max.Y - unmovedBox.Max.Y;
                            break;

                        case Dimension.Z:
                            stretchedBox.Max.Z = blockBounds.Min.Z - GameConstants.PHYSICS_COLLISION_EPSILON;
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
                            stretchedBox.Min.X = blockBounds.Max.X + GameConstants.PHYSICS_COLLISION_EPSILON;
                            relevantChange = stretchedBox.Min.X - unmovedBox.Min.X;
                            break;

                        case Dimension.Y:
                            stretchedBox.Min.Y = blockBounds.Max.Y + GameConstants.PHYSICS_COLLISION_EPSILON;
                            relevantChange = stretchedBox.Min.Y - unmovedBox.Min.Y;
                            break;

                        case Dimension.Z:
                            stretchedBox.Min.Z = blockBounds.Max.Z + GameConstants.PHYSICS_COLLISION_EPSILON;
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

        public abstract void Draw(GameTime gametime);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Voxelist.Mapping;
using Voxelist.Entities;
using Voxelist.Utilities;

namespace Voxelist.Rendering
{
    public static class Camera
    {
        private static bool hasStarted = false;
        public static void Start()
        {
            if (hasStarted) return;
            hasStarted = true;

            Position = new WorldPosition(0, 0, 0, 2.5f, 0);

            fixRotation();
            fixPerspectiveMatrix();
        }

        #region Motion Smoothing
        public static float MoveSpeed = float.MaxValue;

        public static double RotationSpeed = 19;

        private static float desiredXRotation = 0.0f;
        private static float desiredYRotation = 0.0f;

        private static WorldPosition DesiredPosition
        {
            get
            {
                if (IsFollowingSomething)
                    return FollowTarget.CameraFollowPosition;
                else
                    return Camera.Position;
            }
        }
        #endregion

        public static void Update(GameTime gametime)
        {
            updateRotations(gametime);
            updatePosition(gametime);

            fixViewMatrix();
            fixPerspectiveMatrix();
        }

        private static void updatePosition(GameTime gametime)
        {
            Vector3 desiredMove = DesiredPosition - Position;

            float length = desiredMove.Length();
            float maxDist = (float)(MoveSpeed * gametime.ElapsedGameTime.TotalSeconds);

            if (length > maxDist)
                desiredMove *= (maxDist / length);

            Position += desiredMove;
        }

        private static void updateRotations(GameTime gametime)
        {
            float rotationScale = (float)(RotationSpeed * gametime.ElapsedGameTime.TotalSeconds);
            if (rotationScale > 1)
                rotationScale = 1;

            float desiredXRotationChange = desiredXRotation - xRotation;
            float desiredYRotationChange = desiredYRotation - yRotation;

            while (desiredXRotationChange < -Math.PI)
                desiredXRotationChange += MathHelper.TwoPi;
            while (desiredXRotationChange > Math.PI)
                desiredXRotationChange -= MathHelper.TwoPi;
            while (desiredYRotationChange < -Math.PI)
                desiredYRotationChange += MathHelper.TwoPi;
            while (desiredYRotationChange > Math.PI)
                desiredYRotationChange -= MathHelper.TwoPi;

            RotateX(desiredXRotationChange * rotationScale);
            RotateY(desiredYRotationChange * rotationScale);
        }

        #region Entity Following
        public static bool IsFollowingSomething { get; private set; }
        public static Entity FollowTarget { get; private set; }

        /// <summary>
        /// Stops following any target, and returns to its default mode.
        /// Returns true if and only if it was previously following something.
        /// </summary>
        /// <returns></returns>
        public static bool StopFollowing()
        {
            bool output = IsFollowingSomething;

            FollowTarget = null;
            IsFollowingSomething = false;

            return output;
        }

        /// <summary>
        /// Starts following the selected target.  If the selected target is
        /// null, the effect is the same as calling StopFollowing().
        /// 
        /// Returns true if and only if it was previously following something.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool StartFollowing(Entity target)
        {
            if (target == null)
                return StopFollowing();

            bool output = IsFollowingSomething;

            IsFollowingSomething = true;
            FollowTarget = target;
            Position = FollowTarget.CameraFollowPosition;

            return output;
        }
        #endregion

        private static void fixMatrices()
        {
            fixViewMatrix();
            fixPerspectiveMatrix();
        }

        #region View Matrix
        private static WorldPosition positionDirect;
        public static WorldPosition Position
        {
            get { return positionDirect; }

            private set
            {
                positionDirect = value;
                fixMatrices();
            }
        }

        /// <summary>
        /// Returns the translation that an object should experience
        /// in order to be drawn at the appropriate location.  This accounts
        /// for the use of "relative" chunk positions when drawing.
        /// </summary>
        /// <param name="objectPosition"></param>
        /// <returns></returns>
        public static Vector3 objectTranslation(WorldPosition objectPosition)
        {
            return new Vector3(
                (objectPosition.chunkX - ChunkX) * GameConstants.CHUNK_X_WIDTH + objectPosition.inChunkX,
                objectPosition.inChunkY,
                (objectPosition.chunkZ - ChunkZ) * GameConstants.CHUNK_Z_LENGTH + objectPosition.inChunkZ
                );
        }

        /// <summary>
        /// The Position vector of the Camera.  Technically this only relates
        /// to the "in-chunk position," but can be set as though it were the
        /// entire position (the chunk coordinates help scaling).
        /// </summary>
        public static Vector3 InChunkPosition
        {
            get { return Position.InChunkPosition; }
            set
            {
                WorldPosition pos = Position;
                pos.InChunkPosition = value;
                Position = pos;
            }
        }

        public static int ChunkX { get { return Position.chunkX; } }
        public static int ChunkZ { get { return Position.chunkZ; } }

        #region Rotation
        private static Vector3 forward = Vector3.Forward;
        private static Vector3 horizontalForward = Vector3.Forward;

        public static Vector3 HorizontalForward
        {
            get { return horizontalForward; }
        }

        private static Vector3 cameraUp = Vector3.Up;

        private static float yRotation = 0.0f; //this is "horizontal" rotation
        private static float xRotation = 0.0f; //this is "vertical" rotation

        private static Matrix horizontalRotationMatrix;
        private static Matrix fullRotationMatrix;

        /// <summary>
        /// Resets the rotation to straight on.
        /// </summary>
        public static void ResetRotation()
        {
            xRotation = 0.0f;
            yRotation = 0.0f;

            fixRotation();
        }

        /// <summary>
        /// This is the intuitive "horizontal" rotation- probably your first
        /// instinct for the proper response to "turn around"
        /// 
        /// This is also just a mask for RotateY.
        /// </summary>
        /// <param name="radians"></param>
        public static void RotateHorizontal(float radians)
        {
            desiredYRotation += radians;

            while (desiredYRotation < -MathHelper.Pi)
                desiredYRotation += MathHelper.TwoPi;
            while (desiredYRotation > MathHelper.Pi)
                desiredYRotation -= MathHelper.TwoPi;
        }

        /// <summary>
        /// This is the intuitive "vertical" rotation- probably your first
        /// instinct for the proper response to "look up"
        /// 
        /// This is also just a mask for RotateX.
        /// </summary>
        /// <param name="radians"></param>
        public static void RotateVertical(float radians)
        {
            desiredXRotation += radians;

            while (desiredXRotation < -MathHelper.Pi)
                desiredXRotation += MathHelper.TwoPi;
            while (desiredXRotation > MathHelper.Pi)
                desiredXRotation -= MathHelper.TwoPi;
        }

        /// <summary>
        /// Rotate the specified number of radians, holding the X-axis fixed.
        /// This is, intuitively, "looking up" or "looking down"
        /// </summary>
        /// <param name="radians"></param>
        private static void RotateX(float radians)
        {
            xRotation += radians;

            while (xRotation < -MathHelper.Pi)
                xRotation += MathHelper.TwoPi;
            while (xRotation > MathHelper.Pi)
                xRotation -= MathHelper.TwoPi;

            fixRotation();
        }

        /// <summary>
        /// Rotate the specified number of radians, holding the Y-axis fixed.
        /// This is, intuitively, "looking left" or "looking right"
        /// </summary>
        /// <param name="radians"></param>
        private static void RotateY(float radians)
        {
            yRotation += radians;

            while (yRotation < -MathHelper.Pi)
                yRotation += MathHelper.TwoPi;
            while (yRotation > MathHelper.Pi)
                yRotation -= MathHelper.TwoPi;

            fixRotation();
        }

        /// <summary>
        /// Fixes the direction vector and rotation matrix to
        /// fit with the xRotation and yRotation fields.
        /// </summary>
        private static void fixRotation()
        {
            if (xRotation < -MathHelper.PiOver2)
                xRotation = -MathHelper.PiOver2;

            if (xRotation > MathHelper.PiOver2)
                xRotation = MathHelper.PiOver2;

            horizontalRotationMatrix = Matrix.CreateRotationY(yRotation);
            horizontalForward = Vector3.Transform(Vector3.Forward, horizontalRotationMatrix);

            fullRotationMatrix = Matrix.CreateRotationX(xRotation) * horizontalRotationMatrix;

            forward = Vector3.Transform(Vector3.Forward, fullRotationMatrix);
            cameraUp = Vector3.Transform(Vector3.Up, fullRotationMatrix);

            fixMatrices();
        }
        #endregion Rotation

        private static Matrix viewMatrix;
        public static Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        private static BoundingFrustum boundingFrustrum;

        private static void fixViewMatrix()
        {
            viewMatrix = Matrix.CreateLookAt(InChunkPosition, InChunkPosition + forward, cameraUp);
            boundingFrustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        #endregion View Matrix

        #region Projection Matrix

        private static float nearPlaneDistance = 0.1f;
        public static float NearPlaneDistance
        {
            get { return nearPlaneDistance; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Near plane distance must be nonnegative.");
                if (value >= farPlaneDistance)
                    throw new ArgumentOutOfRangeException("Near plane distance must be less than far plane distance.");

                nearPlaneDistance = value;
                fixPerspectiveMatrix();
            }
        }

        private static float farPlaneDistance = 1000f;
        public static float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Far plane distance must be nonnegative.");
                if (value <= nearPlaneDistance)
                    throw new ArgumentOutOfRangeException("Near plane distance must be less than far plane distance.");

                farPlaneDistance = value;
                fixPerspectiveMatrix();
            }
        }

        private static float fieldOfView = MathHelper.PiOver4;
        private static float cosSquared = (float)(Math.Cos(fieldOfView) * Math.Cos(fieldOfView));

        public static float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("FOV must be positive.");
                if (value >= MathHelper.PiOver2)
                    throw new ArgumentOutOfRangeException("FOV must be less than Pi/2 (90 degrees).");

                fieldOfView = value;
                cosSquared = (float)(Math.Cos(fieldOfView) * Math.Cos(fieldOfView));

                fixPerspectiveMatrix();
            }
        }

        private static float aspectRatio = 16.0f / 9.0f;
        public static float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Aspect ratio must be positive.");

                aspectRatio = value;
                fixPerspectiveMatrix();
            }
        }

        private static Matrix projectionMatrix = Matrix.Identity;

        private static void fixPerspectiveMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
            boundingFrustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }

        public static Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }
        #endregion Projection Matrix

        #region Drawing Assistance
        private static float distanceFarAway = 60f;
        private static float squareDistanceFarAway = distanceFarAway * distanceFarAway; //if the square distance from the center of the chunk to the camera center is greater than this, use "spaced out" chunk drawing

        public static float DistanceFarAwayCutoff
        {
            get { return distanceFarAway; }

            set
            {
                distanceFarAway = value;
                squareDistanceFarAway = value * value;
            }
        }

        private static float CUBE_DRAW_CUTOFF_DISTANCE = 120f;
        private static float CUBE_DRAW_CUTOFF_SQUARE_DISTANCE = CUBE_DRAW_CUTOFF_DISTANCE * CUBE_DRAW_CUTOFF_DISTANCE;

        public static float MaximumDrawDistance
        {
            get { return CUBE_DRAW_CUTOFF_DISTANCE; }

            set
            {
                CUBE_DRAW_CUTOFF_DISTANCE = value;
                CUBE_DRAW_CUTOFF_SQUARE_DISTANCE = value * value;
            }
        }

        /// <summary>
        /// Determines whether the chunk is completely offscreen; there may be
        /// some false negatives (which hurt performance) but this is better
        /// than false positives (which make whole chunks turn invisible for
        /// no reason).
        /// </summary>
        /// <param name="drawLocation"></param>
        /// <returns></returns>
        public static bool ChunkIsCompletelyOffScreen(Vector3 drawLocation)
        {
            BoundingBox box = new BoundingBox(drawLocation, drawLocation + GameConstants.CHUNK_SIZE);

            return !boundingFrustrum.Intersects(box);
        }

        public static bool ChunkIsFarAway(Vector3 chunkCornerLocation)
        {
            chunkCornerLocation.X += GameConstants.CHUNK_X_WIDTH >> 1;
            chunkCornerLocation.Y += GameConstants.CHUNK_Y_HEIGHT >> 1;
            chunkCornerLocation.Z += GameConstants.CHUNK_Z_LENGTH >> 1;

            Vector3 effectiveChange = chunkCornerLocation - InChunkPosition;
            effectiveChange.Y = 0;

            return effectiveChange.LengthSquared() > squareDistanceFarAway;
        }

        public static bool IsOffScreen(BoundingBox box)
        {
            return !boundingFrustrum.Intersects(box);
        }
        #endregion
    }
}

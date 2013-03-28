using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Voxelist.Entities;
using Voxelist.Mapping;
using Voxelist.Rendering;

namespace VoxelistDemo1
{
    public class PlayerAvatar : Entity
    {
        private Vector3 Size = new Vector3(.8f, 1.7f, .8f);

        public PlayerAvatar(WorldPosition position, WorldManager manager)
            : base(position, manager)
        {
        }

        public override BoundingBox BoundingBox
        {
            get { return new BoundingBox(Position.InChunkPosition - Size / 2f, Position.InChunkPosition + Size / 2f); }
        }

        public override WorldPosition CameraFollowPosition
        {
            get { return Position + Vector3.Up * 0.7f; }
        }

        private float walkSpeed = 1.4f;
        private float runSpeed = 4.9f;

        private float jumpSpeed = 5f;
        private bool spaceHeld = false;

        private Vector3 intentionalVelocity = Vector3.Zero;

        protected override Vector3 GroundIntendedVelocity
        {
            get { return intentionalVelocity; }
        }

        public override float Friction_Induced
        {
            get { return 50; }
        }

        public override void Update(GameTime gametime)
        {
            KeyboardState ks = Keyboard.GetState();

            #region Walking
            intentionalVelocity = Vector3.Zero;

            float speed;

            if (ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift))
                speed = walkSpeed;
            else
                speed = runSpeed;

            float angle = 0;

            bool moving = true;

            if (ks.IsKeyDown(Keys.W) && !ks.IsKeyDown(Keys.S))
            {
                if (ks.IsKeyDown(Keys.A) && !ks.IsKeyDown(Keys.D))
                    angle = MathHelper.PiOver4;
                else if (ks.IsKeyDown(Keys.D) && !ks.IsKeyDown(Keys.A))
                    angle = -MathHelper.PiOver4;
                else
                    angle = 0;
            }
            else if (ks.IsKeyDown(Keys.S) && !ks.IsKeyDown(Keys.W))
            {
                if (ks.IsKeyDown(Keys.A) && !ks.IsKeyDown(Keys.D))
                    angle = MathHelper.PiOver4 * 3.0f;
                else if (ks.IsKeyDown(Keys.D) && !ks.IsKeyDown(Keys.A))
                    angle = -MathHelper.PiOver4 * 3.0f;
                else
                    angle = MathHelper.Pi;
            }
            else if (ks.IsKeyDown(Keys.A) && !ks.IsKeyDown(Keys.D))
            {
                angle = MathHelper.PiOver2;
            }
            else if (ks.IsKeyDown(Keys.D) && !ks.IsKeyDown(Keys.A))
            {
                angle = -MathHelper.PiOver2;
            }
            else
            {
                moving = false;
            }

            if (moving)
                intentionalVelocity = speed * Vector3.Transform(Camera.HorizontalForward, Matrix.CreateRotationY(angle));

            #endregion Walking

            if (OnGround) //this is a good way to see if you're on the ground
            {
                //now for jumping
                if (ks.IsKeyDown(Keys.Space) && !spaceHeld)
                {
                    Vector3 pv = Velocity;
                    pv.Y = jumpSpeed;
                    Velocity = pv;
                }

                spaceHeld = ks.IsKeyDown(Keys.Space);
            }

            //now do the physics!
            base.physicsUpdate(gametime);
        }

        public override void Draw(GameTime gametime)
        {
            //invisible
        }

        protected override float UpStepSize
        {
            get { return 1.2f; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Sputnik {
    class AIController : ShipController
    {
        bool goingStart;
        Vector2 start, finish;
        public GameEnvironment env;

        /// <summary>
        ///  Creates a new AI with given start and finish positions of patrol path and given environment
        /// </summary>
        public AIController(Vector2 s, Vector2 f, GameEnvironment e)
        {
            start = s;
            finish = f;
            env = e;
            goingStart = true;
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime)
        {
            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            // Changed from: float wantedDirection = (float)Math.Atan2(destination.Y - s.position.Y, destination.X - s.position.X);
            float wantedDirection = (float)Math.Atan2(destination.Y - s.Position.Y, destination.X - s.Position.X);
            while (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2.0f;
            // Changed from while (s.direction < 0)
            while (s.Rotation < 0)
                s.Rotation += MathHelper.Pi * 2.0f;
            s.Rotation %= MathHelper.Pi * 2.0f;
            wantedDirection %= MathHelper.Pi * 2.0f;
            if (Vector2.Distance(s.Position, destination) < s.maxSpeed/env.FPS) //This number needs tweaking, 0 does not work
            {
                goingStart = !goingStart;
                // Changed from: s.velocity = Vector2.Zero;
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Math.Abs(wantedDirection-s.Rotation) < s.maxTurn)
            {
                // changed from: s.velocity = new Vector2((float)Math.Cos(s.direction) * s.maxSpeed, (float)Math.Sin(s.direction) * s.maxSpeed);
                s.DesiredVelocity = new Vector2((float)Math.Cos(s.Rotation) * s.maxSpeed, (float)Math.Sin(s.Rotation) * s.maxSpeed);
                //s.SetPhysicsVelocityOnce(new Vector2((float)Math.Cos(s.Rotation) * s.maxSpeed, (float)Math.Sin(s.Rotation) * s.maxSpeed));
            }
            else
            {
                // Changed from: s.velocity = Vector2.Zero;
                s.DesiredVelocity = Vector2.Zero;
                float counterclockwiseDistance = Math.Abs(wantedDirection - (s.Rotation + s.maxTurn)%(MathHelper.Pi * 2));
                float clockwiseDistance = Math.Abs(wantedDirection - (s.Rotation - s.maxTurn + MathHelper.Pi * 2) % (MathHelper.Pi * 2));
                if (counterclockwiseDistance < clockwiseDistance)
                {
                    if (counterclockwiseDistance < s.maxTurn)
                    {
                        s.Rotation = wantedDirection;
                    }
                    else
                    {
                        s.Rotation += s.maxTurn;
                    }
                }
                else
                {
                    if (clockwiseDistance < s.maxTurn)
                    {
                        s.Rotation = wantedDirection;
                    }
                    else
                    {
                        s.Rotation -= s.maxTurn;
                    }
                }
            }
            //Theoretically I should shoot when player is in front, but this is funner
            Random r = new Random();
            // Changed from: s.shoot = r.NextDouble() < 0.5;
            if (r.NextDouble() < 0.1)
                s.Shoot(elapsedTime);
        }

    }
}

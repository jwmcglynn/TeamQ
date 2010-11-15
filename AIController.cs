﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using System.IO;

namespace Sputnik {
    class AIController : ShipController
    {
        bool goingStart;
        Vector2 start, finish;
        public GameEnvironment env;
        Vector2 positionHit;

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
            float wantedDirection = (float)Math.Atan2(destination.Y - s.Position.Y, destination.X - s.Position.X);
            while (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2.0f;
            while (s.Rotation < 0)
                s.Rotation += MathHelper.Pi * 2.0f;
            s.Rotation %= MathHelper.Pi * 2.0f;
            wantedDirection %= MathHelper.Pi * 2.0f;
            if (Vector2.Distance(s.Position, destination) < s.maxSpeed * elapsedTime) //This number needs tweaking, 0 does not work
            {
                goingStart = !goingStart;
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Math.Abs(wantedDirection-s.Rotation) < s.maxTurn)
            {
                s.DesiredVelocity = new Vector2((float)Math.Cos(s.Rotation) * s.maxSpeed, (float)Math.Sin(s.Rotation) * s.maxSpeed);    
            }
            else
            {
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
            if (CanSee(s,FindSputnik()))
                s.Shoot(elapsedTime);
        }

        /// <summary>
        ///  For Testing purposes, finds Sputnik's ship
        /// </summary>

        private Ship FindSputnik()
        {
            foreach (Entity e in env.Children)
            {
                if (e is SputnikShip)
                    return (Ship)e;
            }
            return null;
        }

        /// <summary>
        /// Preliminary Vision, given starting Ship s and target Ship f, can s see f 
        /// </summary>
        private bool CanSee(Ship s, Ship f)
        {
            float theta = (float)(Math.Atan2(f.Position.Y - s.Position.Y, f.Position.X - s.Position.X));
            if (theta < 0)
                theta += MathHelper.TwoPi;
            if (Math.Abs(theta - s.Rotation) < (MathHelper.ToRadians(20)))
            {
                //Why do I have to use the collision Body's Position.  Does it relate to our relative positions?
                //env.CollisionWorld.RayCast(RayCastHit, s.Position, f.Position);
                env.CollisionWorld.RayCast(RayCastHit, s.CollisionBody.Position, f.CollisionBody.Position);
                if (positionHit.Equals(f.CollisionBody.Position))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public float RayCastHit(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            positionHit = fixture.Body.Position;
            return 0;
        }

    }
}

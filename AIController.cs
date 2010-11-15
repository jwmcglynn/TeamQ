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
        Ship ship;
        /// <summary>
        ///  Creates a new AI with given start and finish positions of patrol path and given environment
        /// </summary>
        public AIController(Vector2 s, Vector2 f, Ship sh, GameEnvironment e)
        {
            start = s;
            finish = f;
            env = e;
            goingStart = true;
            ship = sh;
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(float elapsedTime)
        {
            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            float wantedDirection = (float)Math.Atan2(destination.Y - ship.Position.Y, destination.X - ship.Position.X);
            while (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2.0f;
            while (ship.Rotation < 0)
                ship.Rotation += MathHelper.Pi * 2.0f;
            ship.Rotation %= MathHelper.Pi * 2.0f;
            wantedDirection %= MathHelper.Pi * 2.0f;
            if (Vector2.Distance(ship.Position, destination) < ship.maxSpeed * elapsedTime) //This number needs tweaking, 0 does not work
            {
                goingStart = !goingStart;
                ship.DesiredVelocity = Vector2.Zero;
            }
            else if (Math.Abs(wantedDirection - ship.Rotation) < ship.maxTurn)
            {
                ship.DesiredVelocity = new Vector2((float)Math.Cos(ship.Rotation) * ship.maxSpeed, (float)Math.Sin(ship.Rotation) * ship.maxSpeed);    
            }
            else
            {
                ship.DesiredVelocity = Vector2.Zero;
                float counterclockwiseDistance = Math.Abs(wantedDirection - (ship.Rotation + ship.maxTurn) % (MathHelper.Pi * 2));
                float clockwiseDistance = Math.Abs(wantedDirection - (ship.Rotation - ship.maxTurn + MathHelper.Pi * 2) % (MathHelper.Pi * 2));
                if (counterclockwiseDistance < clockwiseDistance)
                {
                    if (counterclockwiseDistance < ship.maxTurn)
                    {
                        ship.Rotation = wantedDirection;
                    }
                    else
                    {
                        ship.Rotation += ship.maxTurn;
                    }
                }
                else
                {
                    if (clockwiseDistance < ship.maxTurn)
                    {
                        ship.Rotation = wantedDirection;
                    }
                    else
                    {
                        ship.Rotation -= ship.maxTurn;
                    }
                }
            }
            if (CanSee(FindSputnik()))
                ship.Shoot(elapsedTime);
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
            //Ur screwed if you can't find Sputnik
            return null;
        }

        private List<Ship> GetSeeables()
        {
            return null;
        }

        /// <summary>
        /// Preliminary Vision, given starting Ship s and target Ship f, can s see f 
        /// </summary>
        private bool CanSee(Ship f)
        {
            float theta = (float)(Math.Atan2(f.Position.Y - ship.Position.Y, f.Position.X - ship.Position.X));
            if (theta < 0)
                theta += MathHelper.TwoPi;
            if (Math.Abs(theta - ship.Rotation) < (MathHelper.ToRadians(20)))
            {
                //Why do I have to use the collision Body's Position.  Does it relate to our relative positions?
                //env.CollisionWorld.RayCast(RayCastHit, s.Position, f.Position);
                env.CollisionWorld.RayCast(RayCastHit, ship.CollisionBody.Position, f.CollisionBody.Position);
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

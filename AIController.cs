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
        Ship target;
        enum State { Allied, Neutral, Alert, Hostile};
        State nextState;


        /// <summary>
        ///  Creates a new AI with given start and finish positions of patrol path and given environment
        /// </summary>
        public AIController(Vector2 s, Vector2 f, GameEnvironment e)
        {
            start = s;
            finish = f;
            env = e;
            goingStart = true;
            nextState = State.Neutral;
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime)
        {
            if (nextState == State.Allied)
            {
                //Not implemented
            }
            else if((nextState == State.Neutral))
            {
                Neutral(s, elapsedTime);
            }
            else if ((nextState == State.Alert)) 
            { 
                Alert(s, elapsedTime);
            }
            else 
            {
                Hostile(s, elapsedTime);
            }


        }

        private void Neutral(Ship s, float elapsedTime)
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
            if (Vector2.Distance(s.Position, destination) < s.maxSpeed/2 * elapsedTime) //This number needs tweaking, 0 does not work
            {
                goingStart = !goingStart;
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Math.Abs(wantedDirection - s.Rotation) < s.maxTurn)
            {
                s.DesiredVelocity = new Vector2((float)Math.Cos(s.Rotation) * s.maxSpeed/2, (float)Math.Sin(s.Rotation) * s.maxSpeed/2);
            }
            else
            {
                s.DesiredVelocity = Vector2.Zero;
                float counterclockwiseDistance = Math.Abs(wantedDirection - (s.Rotation + s.maxTurn) % (MathHelper.Pi * 2));
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
            Ship newTarget = SawPlayerShoot(s);
            if (newTarget != null)
            {
                nextState = State.Alert;
                target = newTarget;
            }
            else
            {
                nextState = State.Neutral;
            }
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

        private void Alert(Ship s, float elapsedTime)
        {
            Vector2 destination = target.Position;
            float wantedDirection = (float)Math.Atan2(destination.Y - s.Position.Y, destination.X - s.Position.X);
            while (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2.0f;
            while (s.Rotation < 0)
                s.Rotation += MathHelper.Pi * 2.0f;
            s.Rotation %= MathHelper.Pi * 2.0f;
            wantedDirection %= MathHelper.Pi * 2.0f;
            if (Vector2.Distance(s.Position, destination) < 100)
            {
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Math.Abs(wantedDirection - s.Rotation) < s.maxTurn)
            {
                s.DesiredVelocity = new Vector2((float)Math.Cos(s.Rotation) * s.maxSpeed, (float)Math.Sin(s.Rotation) * s.maxSpeed);
            }
            else
            {
                s.DesiredVelocity = Vector2.Zero;
                float counterclockwiseDistance = Math.Abs(wantedDirection - (s.Rotation + s.maxTurn) % (MathHelper.Pi * 2));
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
            Ship newTarget = SawPlayerShoot(s);
            if (newTarget != null)
            {
                nextState = State.Hostile;
                target = newTarget;
            }
            else
            {
                nextState = State.Alert;
            }
        }

        private Ship SawPlayerShoot(Ship s)
        {
            foreach (Entity b in env.Children)
            {
                if(b is Bullet && ((Bullet)b).ShotByPlayer && CanSee(s,b))
                    return (Ship)(((Bullet)b).owner);
            }
            return null;
        }

        private void Hostile(Ship s, float elapsedTime)
        {
            Vector2 destination = target.Position;
            float wantedDirection = (float)Math.Atan2(destination.Y - s.Position.Y, destination.X - s.Position.X);
            while (wantedDirection < 0)
                wantedDirection += MathHelper.Pi * 2.0f;
            while (s.Rotation < 0)
                s.Rotation += MathHelper.Pi * 2.0f;
            s.Rotation %= MathHelper.Pi * 2.0f;
            wantedDirection %= MathHelper.Pi * 2.0f;
            if (Vector2.Distance(s.Position, destination) < 100)
            {
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
            if(CanSee(s,target))
                s.Shoot(elapsedTime);
            //Did i Kill the target
            if (target.ShouldCull())
            {
                nextState = State.Neutral;
            }
            else
            {
                nextState = State.Hostile;
            }
        }

        /// <summary>
        /// Preliminary Vision, given starting Ship s and target Ship f, can s see f 
        /// </summary>
        private bool CanSee(Entity s, Entity f)
        {
            float theta = (float)(Math.Atan2(f.Position.Y - s.Position.Y, f.Position.X - s.Position.X));
            if (theta < 0)
                theta += MathHelper.TwoPi;
            if (Math.Abs(theta - s.Rotation) < (MathHelper.ToRadians(20)))
            {
                //TODO Im not quite sure why, but sometimes ships try to see null collisionbodys
                if (s.CollisionBody == null || f.CollisionBody== null)
                {
                    return false;
                }

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
            return fraction;
        }

    }
}

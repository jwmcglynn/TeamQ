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

			s.shooterRotation = s.Rotation;
        }

        private void Neutral(Ship s, float elapsedTime)
        {
            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            float wantedDirection = Angle.Direction(s.Position, destination);

            if (Vector2.Distance(s.Position, destination) < s.maxSpeed/2 * elapsedTime) //This number needs tweaking, 0 does not work
            {
                goingStart = !goingStart;
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Angle.DistanceMag(wantedDirection, s.Rotation) < s.MaxRotVel * elapsedTime)
            {
				s.DesiredVelocity = Angle.Vector(wantedDirection) * s.maxSpeed / 2;
            }
            else
            {
                s.DesiredVelocity = Vector2.Zero;
				s.DesiredRotation = wantedDirection;
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
            float wantedDirection = Angle.Direction(s.Position, destination);

            if (Vector2.Distance(s.Position, destination) < 100)
            {
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Angle.DistanceMag(wantedDirection, s.Rotation) < s.MaxRotVel * elapsedTime)
            {
				s.DesiredVelocity = Angle.Vector(wantedDirection) * s.maxSpeed;
            }
            else
            {
                s.DesiredVelocity = Vector2.Zero;
				s.DesiredRotation = wantedDirection;
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
                if(b is Bullet && ((Bullet)b).ShotByPlayer && CanSee(s, b))
                    return (Ship)(((Bullet)b).owner);
            }
            return null;
        }

        private void Hostile(Ship s, float elapsedTime)
        {
            Vector2 destination = target.Position;
            float wantedDirection = Angle.Direction(s.Position, destination);

            if (Vector2.Distance(s.Position, destination) < 100)
            {
                s.DesiredVelocity = Vector2.Zero;
            }
            else if (Angle.DistanceMag(wantedDirection, s.Rotation) < s.MaxRotVel * elapsedTime)
            {
				s.DesiredVelocity = Angle.Vector(wantedDirection) * s.maxSpeed;
            }
            else
            {
                s.DesiredVelocity = Vector2.Zero;
				s.DesiredRotation = wantedDirection;
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
            float theta = Angle.Direction(s.Position, f.Position);

			if (Angle.DistanceMag(theta, s.Rotation) < (MathHelper.ToRadians(20)))
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

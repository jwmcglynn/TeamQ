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
        Ship target, shotMe;
        enum State { Allied, Neutral, Alert, Hostile, Confused};
        State nextState;
		float startingAngle; //Used for confused
		bool startedRotation;
		Ship lookingFor;

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
			target = null;
			shotMe = null;
			startedRotation = false;
			lookingFor = null;
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
			else if ((nextState == State.Confused))
			{
				Confused(s, elapsedTime);
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
			else if (Angle.DistanceMag(s.Rotation, wantedDirection) < 0.01)
			{
				s.DesiredVelocity = Angle.Vector(wantedDirection) * s.maxSpeed;
				s.DesiredRotation = wantedDirection;
			}
            else
            {
                s.DesiredVelocity = Vector2.Zero;
				s.DesiredRotation = wantedDirection;
            }
            if (shotMe != null)
            {
				target = shotMe;
				shotMe = null;
                nextState = State.Alert;
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

            if (Vector2.Distance(s.Position, destination) < 200)
            {
                s.DesiredVelocity = Vector2.Zero;
				s.DesiredRotation = wantedDirection;
            }
			else if (Angle.DistanceMag(s.Rotation, wantedDirection) < 0.01)
            {
				s.DesiredVelocity = Angle.Vector(wantedDirection) * s.maxSpeed;
				s.DesiredRotation = wantedDirection;
            }
            else
            {
                s.DesiredVelocity = Vector2.Zero;
				s.DesiredRotation = wantedDirection;
            }
            if (shotMe != null)
            {
				if (shotMe == target)
				{
					nextState = State.Hostile;
					target = shotMe;
				}
				else
				{
					nextState = State.Alert;
					target = shotMe;
				}
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

            if (Vector2.Distance(s.Position, destination) < 200)
            {
                s.DesiredVelocity = Vector2.Zero;
            }
			else if (Angle.DistanceMag(s.Rotation, wantedDirection) < 0.01)
            {
				s.DesiredVelocity = Angle.Vector(wantedDirection) * s.maxSpeed;
				s.DesiredRotation = wantedDirection;
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
            //TODO Im not quite sure why, but sometimes ships try to see null collisionbodys
            if (s.CollisionBody == null || f.CollisionBody == null)
            {
                return false;
            }
            if (s.CollisionBody.Position.Equals(f.CollisionBody.Position))
            {
                //This case would only occur if the ai for the player controlled ship tries to see things.
                return false;
            }
			if (Angle.DistanceMag(theta, s.Rotation) < (MathHelper.ToRadians(20)))
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
			if (fixture.Body.IsBullet)
				return -1;
			else
			{
				positionHit = fixture.Body.Position;
				return fraction;
			}
        }

		public void GotShotBy(Ship s, Ship f)
		{
			if (CanSee(s, f))
			{
				shotMe = f;
			}
			else
			{
				lookingFor = f;
				nextState = State.Confused;
				startingAngle = s.Rotation;
				startedRotation = true;
			}
		}



		private void Confused(Ship s ,float elapsedTime)
		{
			s.DesiredVelocity = Vector2.Zero;
			if (startedRotation)
				s.DesiredRotation = s.Rotation + 1.0f;
			else
			{
				//I don't like being unable to say turn right
				if (Angle.Distance(s.Rotation, startingAngle) < MathHelper.Pi)
					s.DesiredRotation = s.Rotation + 1.0f;
				else
					s.DesiredRotation = startingAngle;
			}
			if (CanSee(s,lookingFor))
			{
				target = lookingFor;
				shotMe = null;
				nextState = State.Alert;
			}
			else
			{
				if (Angle.DistanceMag(s.Rotation, startingAngle) < 0.01f && !startedRotation)
				{
					lookingFor = null;
					nextState = State.Neutral;
				}
				else
					nextState = State.Confused;
			}
			startedRotation = false;
		}

    }
}

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
		SpawnPoint spawn;
        bool goingStart;
        Vector2 start, finish;
        public GameEnvironment env;
        Vector2 positionHit;
        GameEntity target;
		GameEntity shotMe;
        enum State{ Allied, Neutral, Alert, Hostile, Confused, Disabled };
        private State oldState,currentState,nextState;
		float startingAngle; //Used for confused
		bool startedRotation;
		GameEntity lookingFor;
		Ship currentShip;
		bool turning;

        /// <summary>
        ///  Creates a new AI with given start and finish positions of patrol path and given environment
        /// </summary>
        public AIController(SpawnPoint sp, GameEnvironment e)
        {
			spawn = sp;
            start = spawn.TopLeft;
            finish = spawn.BottomRight;
            env = e;
            goingStart = true;
            nextState = State.Neutral;
			currentState = State.Neutral;
			target = null;
			shotMe = null;
			startedRotation = false;
			lookingFor = null;
			currentShip = null;
			turning = false;
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime)
        {
			currentShip = s;
			currentState = nextState;

            if (nextState == State.Allied)
            {
                //Not implemented
            }
            else if((nextState == State.Neutral))
            {
                Neutral(elapsedTime);
            }
            else if ((nextState == State.Alert)) 
            { 
                Alert(elapsedTime);
            }
			else if ((nextState == State.Confused))
			{
				Confused(elapsedTime);
			}
			else if(nextState == State.Disabled) {
				Disabled(elapsedTime);
			}
            else
            {
                Hostile(elapsedTime);
            }
			s.shooterRotation = s.Rotation;
        }

        private void Neutral(float elapsedTime)
        {
            Vector2 destination;
            if (goingStart)
                destination = start;
            else
                destination = finish;
            float wantedDirection = Angle.Direction(currentShip.Position, destination);

            if (Vector2.Distance(currentShip.Position, destination) < currentShip.maxSpeed * elapsedTime) //I Want this number to be speed per frame																	
            {
                goingStart = !goingStart;
                currentShip.DesiredVelocity = Vector2.Zero;
				turning = true; 
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime)
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection;
				turning = false; 
			}
            else
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				turning = true;
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

		public bool Turning()
		{
			return turning;
		}

        private void Alert(float elapsedTime)
        {
            Vector2 destination = target.Position;
            float wantedDirection = Angle.Direction(currentShip.Position, destination);

            if (Vector2.Distance(currentShip.Position, destination) < 200)
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				turning = false;
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime)
            {
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection;
				turning = false;
            }
            else
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				turning = true;
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

        private void Hostile(float elapsedTime)
        {
            Vector2 destination = target.Position;
            float wantedDirection = Angle.Direction(currentShip.Position, destination);

            if (Vector2.Distance(currentShip.Position, destination) < 200)
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				turning = true;
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime)
            {
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection;
				turning = false;
            }
            else
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				turning = true;
            }
            if(CanSee(currentShip,target))
                currentShip.Shoot(elapsedTime);
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

		private void Confused(float elapsedTime)
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			turning = true;
			if (startedRotation)
				currentShip.DesiredRotation = currentShip.Rotation + 1.0f;
			else
			{
				//I don't like being unable to say turn right
				if (Angle.Distance(currentShip.Rotation, startingAngle) < MathHelper.Pi)
					currentShip.DesiredRotation = currentShip.Rotation + 1.0f;
				else
					currentShip.DesiredRotation = startingAngle;
			}
			if (CanSee(currentShip, lookingFor))
			{
				target = lookingFor;
				shotMe = null;
				nextState = State.Alert;
			}
			else
			{
				if (Angle.DistanceMag(currentShip.Rotation, startingAngle) < 0.03f && !startedRotation) // Find a good value for this
				{
					lookingFor = null;
					nextState = oldState;
				}
				else
					nextState = State.Confused;
			}
			startedRotation = false;
		}

		private void Disabled(float elapsedTime)
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			//s.DesiredRotation = 0.0f; You probably dont want to make the ship turn towards 0 radians
			if (currentShip.isTractored)
			{
				// determine the position of the tractored item.
				currentShip.Position = currentShip.tractoringShip.Position + new Vector2(100, 100);
			}
			if (currentShip.isFrozen || currentShip.isTractored)
			{
				nextState = State.Disabled;
			}
			else
			{
				nextState = oldState;
			}
		}

		public void GotFrozen()
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			//s.DesiredRotation = 0.0f; You probably dont want to make the ship turn towards 0 radians
			oldState = currentState;
			nextState = State.Disabled;
		}

		public void GotTractored()
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			//s.DesiredRotation = 0.0f; You probably dont want to make the ship turn towards 0 radians
			oldState = currentState;
			nextState = State.Disabled;
		}

        /// <summary>
        /// Preliminary Vision, given starting Ship s and target Ship f, can s see f 
        /// </summary>
        private bool CanSee(Entity s, Entity f)
        {
            
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
			float theta = Angle.Direction(s.Position, f.Position);
			if (Angle.DistanceMag(theta, s.Rotation) < (MathHelper.ToRadians(20)))
            {
                env.CollisionWorld.RayCast(RayCastHit, s.CollisionBody.Position, f.CollisionBody.Position);
				return (positionHit.Equals(f.CollisionBody.Position));
            }
            else
            {
                return false;
            }
        }

        public float RayCastHit(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
			//Faction ships can see through their own faction, unless we happen to be looking at our target
			if (currentShip.GetType() == fixture.Body.UserData.GetType() && fixture.Body.UserData != lookingFor)
				return -1;
			//Ignore Bullets
			else if (fixture.Body.IsBullet)
				return -1;
			else
			{
				positionHit = fixture.Body.Position;
				return fraction;
			}
        }

		public void GotShotBy(Ship s, GameEntity f)
		{
			lookingFor = f;
			if (CanSee(s, f))
			{
				shotMe = f;
			}
			else
			{
				if (currentState != State.Confused && currentState != State.Hostile)
				{
					oldState = currentState;
					nextState = State.Confused;
					startingAngle = s.Rotation;
					startedRotation = true;
				}
			}
		}

		public void HitWall()
		{
			if (currentState == State.Neutral)
			{
				//This works as long as both the start and finish position aren't on the other side of the wall
				goingStart = !goingStart;
			}
			else
			{
				//There is no pathfinding, might as well give up
				nextState = State.Neutral;
			}
		}

		

    }
}

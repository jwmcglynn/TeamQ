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
		private SpawnPoint spawn;  //Used for spawnpoint manipulation
		private GameEnvironment env;  //Game Environment reference
		private Vector2 hitBodyPosition;  //Position of the body hit by a raycast
		private GameEntity target; //Current target of attention
		private GameEntity lookingFor; //Entity that I can't see but I'm looking for
		private enum State { Allied, Neutral, Alert, Hostile, Confused, Disabled }; //All possible states
        private State oldState,currentState,nextState; // Used to control AI's FSM
		//private float startingAngle; //Used for confused
		//private bool startedRotation; //Used for making a ship rotate once
		private Ship currentShip;  //Current ship I'm controlling
		//private bool turning;  //Used to tell if a ship is turning, still somewhat buggy
		private bool answeringDistressCall;  //Used to help Confused State transition
		private float timeSinceLastStateChange;
		private GameEntity rayCastTarget;
		private float timeSinceHitWall;
		private bool recentlyHitWall;
		private float timeSinceChangedTargets;
		private bool recentlyChangedTargets;
		private Vector2 nextPatrolPoint;

		static Random rand = new Random();

        /// <summary>
        ///  Creates a new AI with given spawnpoint and given environment
		///  Currrently sets start and finsih for patrol to top left and bottom right of spawnpoint
		///  Initial state is Neutral and going towards start;
        /// </summary>
        public AIController(SpawnPoint sp, GameEnvironment e)
        {
			timeSinceLastStateChange = 0;
			spawn = sp;
            env = e;
            nextState = State.Neutral;
			currentState = State.Neutral;
			target = null;
			answeringDistressCall = false;
			//startedRotation = false;
			lookingFor = null;
			currentShip = null;
			//turning = false;
			timeSinceHitWall = 0; ;
			recentlyHitWall = false;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime)
        {
			if (timeSinceHitWall > 3) //I consider recent 3 seconds
			{
				recentlyHitWall = false;
				timeSinceHitWall = 0;
			}
			if (timeSinceChangedTargets > 3) //I consider recent 3 seconds
			{
				recentlyChangedTargets = false;
				timeSinceChangedTargets = 0;
			}
			if (recentlyHitWall)
				timeSinceHitWall += elapsedTime;
			if (recentlyChangedTargets)
				timeSinceChangedTargets += elapsedTime;
			timeSinceLastStateChange += elapsedTime;
			currentShip = s;

			bool init = (currentState != nextState);
			currentState = nextState;
			switch(currentState)
			{
				case State.Allied:
					Allied(elapsedTime);
					break;
				case State.Neutral:
					if (init) neutralState = NeutralState.FindNewPatrol;
					Neutral(elapsedTime);
					break;
				case State.Alert:
					Alert(elapsedTime);
					break;
				case State.Confused:
					Confused(elapsedTime);
					break;
				case State.Disabled:
					Disabled(elapsedTime);
					break;
				case State.Hostile:
					Hostile(elapsedTime);
					break;
            }			
        }

		Vector2 randomPointInsideRect(Rectangle r) {
			return new Vector2(
				rand.Next(r.Left, r.Right)
				, rand.Next(r.Top, r.Bottom)
			);
		}

		enum NeutralState {
			Waiting
			, FindNewPatrol
			, Turning
			, Moving
		};

		NeutralState neutralState = NeutralState.FindNewPatrol;
		float waitTimer = 1.0f;

		/// <summary>
		///  AI behavior for Neutral State
		///  Inputs : goingStart, currentShip
		///  Outputs : nextState
		/// </summary>
        private void Neutral(float elapsedTime)
        {
			switch (neutralState) {
				case NeutralState.Waiting:
					waitTimer -= elapsedTime;

					// Wait so we don't move so erratically.
					if (waitTimer < 0.0f) {
						neutralState = NeutralState.FindNewPatrol;
					}
					break;
				case NeutralState.FindNewPatrol:
					nextPatrolPoint = randomPointInsideRect(currentShip.SpawnPoint.Rect);
					neutralState = NeutralState.Turning;
					break;
				case NeutralState.Turning:
					currentShip.MaxRotVel = (float) Math.PI / 2;
					currentShip.DesiredRotation = Angle.Direction(currentShip.Position, nextPatrolPoint);  //Ships want to face the direction their destination is

					// Almost at the correct rotation, start moving.
					if (Angle.DistanceMag(currentShip.Rotation, currentShip.DesiredRotation) < Math.PI / 3) {
						neutralState = NeutralState.Moving;
					}
					break;
				case NeutralState.Moving:
					currentShip.MaxRotVel = (float) Math.PI / 2;
					currentShip.DesiredRotation = Angle.Direction(currentShip.Position, nextPatrolPoint);  //Ships want to face the direction their destination is
					currentShip.DesiredVelocity = Vector2.Normalize(nextPatrolPoint - currentShip.Position) * currentShip.maxSpeed / 3;

					// We've reached the target, wait before moving again.
					if (Vector2.DistanceSquared(currentShip.Position, nextPatrolPoint) < 8.0f) {
						neutralState = NeutralState.Waiting;
						currentShip.DesiredVelocity = Vector2.Zero;
						waitTimer = (float) rand.NextDouble() * 3.0f + 0.3f;
					}
					break;
			}

			nextState = State.Neutral; //I stay in neutral now
		}

		/// <summary>
		///  AI behavior for Alert State
		///  Inputs : target, currentShip
		///  Outputs : nextState
		/// </summary>
        private void Alert(float elapsedTime)
        {
			Vector2 destination = target.Position;  //Current Destination, is set to our target
            float wantedDirection = Angle.Direction(currentShip.Position, destination);  //I like facing my destination when I move

			if (Vector2.Distance(currentShip.Position, destination) < 200)  //I want to keep a certain distance away from my target
				//There is a good chance that I want to multiply this by the levelscale
            {
                currentShip.DesiredVelocity = Vector2.Zero;
			//	if (wantedDirection == currentShip.Rotation)
					//turning = false;
			//	else
					//turning = true;
				currentShip.DesiredRotation = wantedDirection;  //Even though I want to keep a certain distance, I will still turn to face you
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) //Im facing my target
            {
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection;  //Doesnt count as turning
				//turning = false;
            }
            else  //Im not facing my target
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				//turning = true;
            }

			//Might also change if I can't see my target
			if (timeSinceLastStateChange < 5)
				nextState = State.Alert;
			else
				nextState = State.Neutral;
        }

		/// <summary>
		///  AI behavior for Hostile State
		///  Inputs : target, currentShip
		///  Outputs : nextState
		/// </summary>
        private void Hostile(float elapsedTime)
        {
            Vector2 destination = target.Position;  //Im going to my target's position
            float wantedDirection = Angle.Direction(currentShip.Position, destination);  //I want to face my targets direction

            if (Vector2.Distance(currentShip.Position, destination) < 200) //Keep a certain distance from target
				//Good chance I want to incorporate levelScal in here somewhere
            {
                currentShip.DesiredVelocity = Vector2.Zero;
			//	if (wantedDirection == currentShip.Rotation)
				//	turning = false;
			//	else
				//	turning = true;
				currentShip.DesiredRotation = wantedDirection; //Even though I dont move, I want to face my targets direction
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) // Im facing my target
            {
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection; //Does not count as rotation
				//turning = false;
            }
            else //Im not facing my target
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
				//turning = true;
            }
			//TODO What do I do if I can't see my target
			//Shoot if I see my target
			if (CanSee(currentShip, target))
				currentShip.Shoot(elapsedTime, target);

            //Did i Kill the target
            if (target.ShouldCull())
            {
				target = null;
                nextState = State.Neutral;  //If I did, back to default state
				timeSinceLastStateChange = 0;
            }
			else if (target is Ship && currentShip.IsFriendly((Ship)target)) //Darn, I can't kill my target anymore
			{
				nextState = State.Neutral;
				target = null;
				timeSinceLastStateChange = 0;
			}
			else if (target is Boss && currentShip.IsFriendly((Boss)target)) //Darn, I can't kill my target anymore
			{	
				nextState = State.Neutral;
				target = null;
				timeSinceLastStateChange = 0;
			}
			else if (target is SputnikShip && env.sputnik.controlled != null) //Sputnik took control of a ship, destroy it
			{
				if (CanSee(currentShip, env.sputnik.controlled)) //I saw that sneaky Sputnik change ships
				{ //I saw Sputnik take control of that ship
					nextState = State.Hostile;
					target = env.sputnik.controlled;
					timeSinceLastStateChange = 0;
				}
				else //darn, sputnik disappeared
				{
					nextState = State.Neutral;
					timeSinceLastStateChange = 0;
				}
			}
			else
			{
				nextState = State.Hostile;  //More killing
			}
        }

		/// <summary>
		///  AI behavior for Confused State
		///  Looks around for lookingFor
		///  Inputs : lookingFor, currentShip , startedRotation, startingAngle, answeringDistressCall
		///  Outputs : target, nextState
		/// </summary>
		/// Maybe I should rotate in the direction I was shot in, NOT IMPLEMENTED
		/// Always turning CounterClockwise is fun 
		/// Current behavior: spin if lookingfor is dead, do we want this?
		private void Confused(float elapsedTime)
		{
			currentShip.DesiredVelocity = Vector2.Zero;  //I don't move while spinning
		//	turning = true;  //Im spinning!
			/*
			if (startedRotation) //If I just started rotating, rotate right
				currentShip.DesiredRotation = currentShip.Rotation + 1.0f;
			else
			{
				//I don't like being unable to say turn counterclockwise
				if (Angle.Distance(currentShip.Rotation, startingAngle) < MathHelper.Pi)
					currentShip.DesiredRotation = currentShip.Rotation + 1.0f;
				else
					currentShip.DesiredRotation = startingAngle;
			}
			 */
			currentShip.DesiredRotation = currentShip.Rotation + 1.0f; //I always turn now
			if (CanSee(currentShip, lookingFor))  //I found who shot someone, time to do something
			{
				if (answeringDistressCall)
				{
					answeringDistressCall = false;
					target = lookingFor;
					nextState = State.Allied;
					timeSinceLastStateChange = 0;
				}
				else
				{
					target = lookingFor;
					nextState = State.Alert;
					timeSinceLastStateChange = 0;
				}
			}
			else
			{
				//if (Angle.DistanceMag(currentShip.Rotation, startingAngle) < currentShip.MaxRotVel * elapsedTime / 2 && !startedRotation) // I made a complete Revolution
					//I added in the /2 just because it wouldn't work otherwse, maybe use a different value
				if(timeSinceLastStateChange > 1) //I spin for 1 second now
				{
					lookingFor = null;
					nextState = oldState;
					timeSinceLastStateChange = 0;
				}
				else //Nothing happened, Im still confused
					nextState = State.Confused;
			}
			//startedRotation = false;
		}

		/// <summary>
		///  AI behavior for Disabled State
		///  Inputs : currentShip, oldState
		///  Outputs : nextState
		/// </summary>
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
				timeSinceLastStateChange = 0;
				nextState = oldState; // I would like to do something else here
				//Id like to up the alertness level, ie, if you tractor or freeze me, i become alert or hostile
				//I can currently do this for tractor due to knowing the tractoring ship, but can't for freezing.
			}
		}

		/// <summary>
		///  AI behavior for Allied State
		///  Inputs : currentShip, target
		///  Outputs : nextState
		/// </summary>
		private void Allied(float elapsedTime)
		{
			Vector2 destination = target.Position;  //Im going to my target's position
			float wantedDirection = Angle.Direction(currentShip.Position, destination);  //I want to face my targets direction

			if (Vector2.Distance(currentShip.Position, destination) < 200) //Keep a certain distance from target
			//Good chance I want to incorporate levelScal in here somewhere
			{
				currentShip.DesiredVelocity = Vector2.Zero;
			//	if (wantedDirection == currentShip.Rotation)
			//		turning = false;
			//	else
			//		turning = true;
				currentShip.DesiredRotation = wantedDirection; //Even though I dont move, I want to face my targets direction
			}
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) // Im facing my target
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection; //Does not count as rotation
		//		turning = false;
			}
			else //Im not facing my target
			{
				currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
		//		turning = true;
			}
			//Shoot if my target shoots
			//Casting makes me sad, as long as ships dont follow boss, this works
			if (((Ship)target).isShooting)
				currentShip.Shoot(elapsedTime);
			//Is my target dead
			if (target.ShouldCull())
			{
				target = null;
				nextState = State.Neutral;  //If I did, back to default state
				timeSinceLastStateChange = 0;
			}
			else
			{
				nextState = State.Allied;  //More following 
			}
		}

		/// <summary>
		///  Call when controlled ship gets frozen
		/// </summary>
		public void GotFrozen()
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			//s.DesiredRotation = 0.0f; You probably dont want to make the ship turn towards 0 radians
			oldState = currentState;
			nextState = State.Disabled;
			timeSinceLastStateChange = 0;// Shouldnt matter
		}

		/// <summary>
		///  Call when controlled ship gets tractored
		/// </summary>
		public void GotTractored()
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			//s.DesiredRotation = 0.0f; You probably dont want to make the ship turn towards 0 radians
			oldState = currentState;
			nextState = State.Disabled;
			timeSinceLastStateChange = 0;// Shouldnt matter
		}

        /// <summary>
		/// Preliminary Vision, given starting GameEntity s and GameEntity Ship f, can s see f 
        /// </summary>
		private bool CanSee(GameEntity s, GameEntity f)
        {
			//TODO For some reason this case happens, evidently something is going wrong somewhere
			if (s == null || f == null)
			{
				return false;
			}
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
			float theta = Angle.Direction(s.Position, f.Position); //Angle that I want to see
			if (Angle.DistanceMag(theta, s.Rotation) < (MathHelper.ToRadians(20))) // If within cone of vision (20 degrees), raycast
            {
				rayCastTarget = f;
                env.CollisionWorld.RayCast(RayCastHit, s.CollisionBody.Position, f.CollisionBody.Position);
				return (hitBodyPosition.Equals(f.CollisionBody.Position));
            }
            else //Not within cone of vision
            {
                return false;
            }
        }

		/// <summary>
		/// RayCast method
		/// </summary>
        private float RayCastHit(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
			//Faction ships can see through their own faction, unless we happen to be looking at our target
			if (currentShip.GetType() == fixture.Body.UserData.GetType() && fixture.Body.UserData != rayCastTarget)
				return -1;
			//Ignore Bullets
			else if (fixture.Body.IsBullet)
				return -1;
			//Ignore Ships, weird things happen when ships block the way
			else if (fixture.Body.UserData is Ship && fixture.Body.UserData != rayCastTarget)
				return -1;
			else //look for closest, sets current GameEntity hit as the Body hit's postion
			{
				hitBodyPosition = fixture.Body.Position;
				return fraction;
			}
        }

		/// <summary>
		///  Call when s gets shot by f, due to bosses not being ships, default to GameEntity
		/// </summaryd>
		public void GotShotBy(Ship s, GameEntity f)
		{
			if (currentState != State.Allied) //Allied ships do nothing if shot
			{
				if (CanSee(currentShip, f)) //If I can see the shooter
				{
					switch (currentState)
					{
						case State.Neutral: //Currently I only become un-neutral when shot
							target = f;
							nextState = State.Alert;
							timeSinceLastStateChange = 0;
							break;
						case State.Alert:  //Someone shot me, time to do something
							if (f == target) //My target shot me, now Im mad
							{
								nextState = State.Hostile;
								target = f;
								timeSinceLastStateChange = 0;
							}
							else  //Someone else shot me, time to get suspicious of them
							{
								//I actually might want to make this behavior more faction like
								//If whoever shot me is the same faction as my previous target, I might just go hostile
								//Take this up to the game creator gods.
								nextState = State.Alert;
								target = f;
								timeSinceLastStateChange = 0; //I mean to do this
							}
							break;
						case State.Hostile:
							if (!recentlyChangedTargets)
							{
								nextState = State.Hostile;
								target = f;
								timeSinceLastStateChange = 0;
								recentlyChangedTargets = true;
							}
							break;
						default:
							//current do nothing if in Disabled or Hostile when shot
							break;
					}
				}
				else
				{
					if (currentState == State.Confused) //If Im confused, I don't make my oldState Confused,
					//but I do become more confused
					{
						lookingFor = f;
						nextState = State.Confused;
						timeSinceLastStateChange = 0; //I mean to do this
				//		startingAngle = currentShip.Rotation;
				//		startedRotation = true;
						answeringDistressCall = false;
					}
					else if (currentState != State.Hostile) //I don't become confused if im hostile
					{
						lookingFor = f;
						oldState = currentState;
						nextState = State.Confused;
						timeSinceLastStateChange = 0;
				//		startingAngle = currentShip.Rotation;
					//	startedRotation = true;
						answeringDistressCall = false;
					}
				}
			}
		}

		/// <summary>
		///  Call when controlled ship hits a wall
		/// </summary>
		public void HitWall()
		{
			if (currentState != State.Allied && !recentlyHitWall) //Allied ships and ships that recently hit a wall ignore hitting a wall
			{
				if (currentState == State.Neutral)
				{
					//This works as long as both the start and finish position aren't on the other side of the wall
					//With no pathfinding, this is probably the best I can do
					neutralState = NeutralState.FindNewPatrol;
					recentlyHitWall = true;
				}
				else
				{
					//There is no pathfinding, might as well give up
					nextState = State.Neutral;
					timeSinceLastStateChange = 0;
					target = null;
					recentlyHitWall = true;
				}
			}
		}

		/// <summary>
		///  Call when sputnik's controlled ship gets shot by f
		/// </summary>
		public void DistressCall(Ship s, GameEntity f)
		{
			if (currentState == State.Neutral) //Only non busy ships (neutral) answer
			{
				if (CanSee(currentShip, s) && !s.GetType().Equals(f.GetType()))//If i can see sputniks ship, go help him 
					//if there isnt some civil war going on
				{
					target = s;
					nextState = State.Allied;
					timeSinceLastStateChange = 0;
				}
				else  //If I can't see Sputnik's ship, go look for it.
				{
					answeringDistressCall = true;
					lookingFor = s;
					oldState = currentState;
					nextState = State.Confused;
			//		startingAngle = currentShip.Rotation;
			//		startedRotation = true;
					timeSinceLastStateChange = 0;
				}
			}
		}

		/// <summary>
		///  Call when sputnik detaches from this ship
		/// </summary>
		public void gotDetached()
		{
			spawn.Position = spawn.Entity.Position;
		}

		/// <summary>
		///  Tells if this ship is allied with player
		/// </summary>
		public bool IsAlliedWithPlayer()
		{
			return currentState == State.Allied;
		}
    }
}

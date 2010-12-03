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
		private GameEntity target, oldTarget; //Current target of attention
		private GameEntity lookingFor; //Entity that I can't see but I'm looking for
		private enum State { Allied, Neutral, Alert, Hostile, Confused, Disabled }; //All possible states
        private State oldState,currentState,nextState; // Used to control AI's FSM
		private Ship currentShip;  //Current ship I'm controlling
		private bool answeringDistressCall;  //Used to help Confused State transition
		private float timeSinceLastStateChange;
		private GameEntity rayCastTarget;
		private float timeSinceHitWall;
		private bool recentlyHitWall;
		private float timeSinceChangedTargets;
		private bool recentlyChangedTargets;
		private float timeSinceMoved;
		private List<Vector2> patrolPoints;
		private static Random r = new Random();
		private float waitTimer;
		private Vector2 oldPosition;

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
			lookingFor = null;
			currentShip = null;
			timeSinceHitWall = 0; ;
			recentlyHitWall = false;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
			waitTimer = 0;
			patrolPoints = new List<Vector2>();
			patrolPoints.Add(randomPatrolPoint());
			oldTarget = null;
			timeSinceMoved = 0;
			oldPosition = new Vector2(-1000, 1000);//I hope this is improbable
        }

        /// <summary>
        ///  Updates the State of a ship
        /// </summary>

        public void Update(Ship s, float elapsedTime)
        {
			if ((currentShip != null) && (oldPosition.Equals(new Vector2(-1000, 1000)) || !oldPosition.Equals(currentShip.Position)))
			{
				timeSinceMoved = 0;
				oldPosition = currentShip.Position;
			}
			else
			{
				//currentShip==null defaults here, but it wont matter
				timeSinceMoved += elapsedTime;
			}
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
			currentState = nextState;
			currentShip.ResetMaxRotVel();//For the turning speed slowdown;
			switch(currentState)
			{
				case State.Allied:
					Allied(elapsedTime);
					break;
				case State.Neutral:
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

		/// <summary>
		/// AI behavior for Neutral State
		/// Inputs : goingStart, currentShip, timeSinceLastMoved, oldPosition
		/// Outputs : nextState, timeSinceLastMoved, oldPosition
		/// </summary>
		private void Neutral(float elapsedTime)
		{
			if (timeSinceMoved > 3)
			{
				timeSinceMoved = 0;
				patrolPoints.RemoveAt(0);
				patrolPoints.Add(randomPatrolPoint());
				//Actually, I might want to put my new collision wall behavior here
			}
			if (waitTimer > 0)
			{
				waitTimer -= elapsedTime;
			}
			else
			{
				currentShip.MaxRotVel = MathHelper.Pi / 2; //Take my time turning
				Vector2 destination = patrolPoints.First();
				float wantedDirection = Angle.Direction(currentShip.Position, destination); //Ships want to face the direction their destination is
				if (Vector2.Distance(currentShip.Position, destination) < currentShip.maxSpeed * elapsedTime) //Im one frame from my destination
				{
					patrolPoints.RemoveAt(0);
					patrolPoints.Add(randomPatrolPoint());
					currentShip.DesiredVelocity = Vector2.Zero;
					waitTimer = (float)(r.NextDouble() * 3 + 0.3);
				}
				else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) //Im facing the direction I want to go in
				{
					currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed / 3;
					currentShip.DesiredRotation = wantedDirection; //Turn a little bit for any rounding, does not count as turning
				}
				else //Im not facting the correct direction
				{
					currentShip.DesiredVelocity = Vector2.Zero;
					currentShip.DesiredRotation = wantedDirection;
					//Added in to make movement fluid.
					if (Angle.DistanceMag(currentShip.Rotation, currentShip.DesiredRotation) < Math.PI / 3)
					{
						currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed / 3;
					}
					else
					{
						currentShip.DesiredVelocity = Vector2.Zero;
					}
				}
			}
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
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;  //Even though I want to keep a certain distance, I will still turn to face you
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) //Im facing my target
            {
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection;  //Doesnt count as turning
            }
            else  //Im not facing my target
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
            }

			//Might also change if I can't see my target
			if (timeSinceLastStateChange > 5)
				changeToNeutral();
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
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection; //Even though I dont move, I want to face my targets direction
            }
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) // Im facing my target
            {
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection; //Does not count as rotation
            }
            else //Im not facing my target
            {
                currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
            }
			//TODO What do I do if I can't see my target
			//Shoot if I see my target
			if (CanSee(currentShip, target))
				currentShip.Shoot(elapsedTime, target);

            //Did i Kill the target
            if (target.ShouldCull())
            {
				changeToNeutral();
            }
			else if (target is Ship && currentShip.IsFriendly((Ship)target)) //Darn, I can't kill my target anymore
			{
				changeToNeutral();
			}
			else if (target is Boss && currentShip.IsFriendly((Boss)target)) //Darn, I can't kill my target anymore
			{
				changeToNeutral();
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
			currentShip.DesiredRotation = currentShip.Rotation + 1.0f; //I always turn now
			if (CanSee(currentShip, lookingFor))  //I found who shot someone, time to do something
			{
				if (answeringDistressCall)
				{
					changeToAllied(lookingFor);
				}
				else
				{
					changeToAlert(lookingFor);
				}
			}
			else
			{
				if(timeSinceLastStateChange > 1) //I spin for 1 second now
				{
					changeToOld();
				}
			}
		}

		/// <summary>
		///  AI behavior for Disabled State
		///  Inputs : currentShip, oldState
		///  Outputs : nextState
		/// </summary>
		private void Disabled(float elapsedTime)
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			if (currentShip.isTractored)
			{
				// determine the position of the tractored item.
				currentShip.Position = currentShip.tractoringShip.Position + new Vector2(100, 100);
			}
			if (!currentShip.isFrozen && !currentShip.isTractored)
			{
				changeToOld();
				// I would like to do something else here
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
			{
				currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection; //Even though I dont move, I want to face my targets direction
			}
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < currentShip.MaxRotVel * elapsedTime) // Im facing my target
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection; //Does not count as rotation
			}
			else //Im not facing my target
			{
				currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;
			}
			//Shoot if my target shoots
			//Casting makes me sad, as long as ships dont follow boss, this works
			if (((Ship)target).isShooting)
				currentShip.Shoot(elapsedTime);
			//Is my target dead
			if (target.ShouldCull())
			{
				changeToNeutral();
			}
		}

		/// <summary>
		///  Call when controlled ship gets frozen
		/// </summary>
		public void GotFrozen()
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			changeToDisabled();
		}

		/// <summary>
		///  Call when controlled ship gets tractored
		/// </summary>
		public void GotTractored()
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			changeToDisabled();
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
							changeToAlert(f);
							break;
						case State.Alert:  //Someone shot me, time to do something
							if (f == target) //My target shot me, now Im mad
							{
								changeToHostile(f);
							}
							else  //Someone else shot me, time to get suspicious of them
							{
								//I actually might want to make this behavior more faction like
								//If whoever shot me is the same faction as my previous target, I might just go hostile
								//Take this up to the game creator gods.
								changeToAlert(f);
							}
							break;
						case State.Hostile:
							if (!recentlyChangedTargets)
							{
								changeToHostile(f);
							}
							break;
						default:
							//current do nothing if in Disabled or Hostile when shot
							break;
					}
				}
				else if (currentState != State.Hostile) //I don't become confused if im hostile
					{
						changeToConfused(f, false);
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
					patrolPoints.RemoveAt(0);
					patrolPoints.Add(randomPatrolPoint());
					recentlyHitWall = true;
				}
				else
				{
					//There is no pathfinding, might as well give up
					changeToNeutral();
					recentlyHitWall = true;
				}
			}
		}

		/// <summary>
		///  Call when sputnik's controlled ship (s) gets shot by f
		/// </summary>
		public void DistressCall(Ship s, GameEntity f)
		{
			if (currentState == State.Neutral) //Only non busy ships (neutral) answer
			{
				if (CanSee(currentShip, s) && !s.GetType().Equals(f.GetType()))//If i can see sputniks ship, go help him 
					//if there isnt some civil war going on
				{
					changeToAllied(s);
				}
				else  //If I can't see Sputnik's ship, go look for it.
				{
					changeToConfused(s, true);
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

		private Vector2 randomPatrolPoint()
		{
			return new Vector2(
			r.Next((int)spawn.TopLeft.X, (int)spawn.TopRight.X),
			r.Next((int)spawn.TopLeft.Y, (int)spawn.BottomRight.Y)
			);
		}

		private void changeToNeutral()
		{
			oldTarget = null;
			target = null;
			lookingFor = null;
			oldState = currentState;
			nextState = State.Neutral;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
		}

		private void changeToAlert(GameEntity t)
		{
			oldTarget = null;
			target = t;
			lookingFor = null;
			oldState = currentState;
			nextState = State.Alert;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
		}

		private void changeToHostile(GameEntity t)
		{
			oldTarget = null;
			target = t;
			lookingFor = null;
			oldState = currentState;
			nextState = State.Hostile;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
		}

		private void changeToAllied(GameEntity t)
		{
			oldTarget = null;
			target = t;
			lookingFor = null;
			oldState = currentState;
			nextState = State.Allied;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
		}

		private void changeToDisabled()
		{
			oldTarget = target;
			target = null;
			lookingFor = null;
			oldState = currentState;
			nextState = State.Disabled;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
		}

		private void changeToConfused(GameEntity l, bool adc)
		{
			oldTarget = target;
			target = null;
			lookingFor = l;
			if (currentState != State.Confused)
				oldState = currentState;
			nextState = State.Confused;
			answeringDistressCall = adc;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			recentlyChangedTargets = false;
		}

		private void changeToOld()
		{
			switch (oldState)
			{
				case State.Neutral:
					changeToNeutral();
					break;
				case State.Alert:
					changeToAlert(oldTarget);
					break;
				case State.Hostile:
					changeToHostile(oldTarget);
					break;
				case State.Allied:
					changeToAllied(oldTarget);
					break;
			}

		}
    }
}

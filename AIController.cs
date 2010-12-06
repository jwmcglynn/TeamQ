﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Sputnik
{
	class AIController : ShipController
	{
		private SpawnPoint spawn;  //Used for spawnpoint manipulation
		private GameEnvironment env;  //Game Environment reference
		private Vector2 hitBodyPosition;  //Position of the body hit by a raycast
		public GameEntity target { get; private set; }//Current target of attention
		private List<GameEntity> targetList;  //List of things to shoot
		private GameEntity lookingFor; //Entity that I can't see but I'm looking for
		private enum State {Allied, Neutral, Alert, Hostile, Confused, Disabled} //All possible states
		private State currentState,nextState; // Used to control AI's FSM
		private Ship currentShip;  //Current ship I'm controlling, code mostly assumes this ship is constant
		private bool answeringDistressCall;  //Used to help Confused State transition
		private float timeSinceLastStateChange;  //Counter used to tell time since a state change
		private GameEntity rayCastTarget;  //Target of a raycast
		private float timeSinceHitWall; // counter used to tell time since hit wall
		private float timeSinceChangedTargets; //counter used to tell time since last changed targets
		private float timeSinceMoved; //counter used to tell time since last movement
		private float timeSinceSawTarget;  //counter used to tell time sine last saw target
		private float timeSinceAnsweredDistressCall;  //Time since last answered a distress call
		private List<Vector2> patrolPoints; //List of patrol points to go to.  
		//If we had pathfinding, it makes since for this to be a List, currently its here for futureness
		private static Random r = new Random(); //Random number generator
		private float waitTimer; //countdown timer for ships to stay inert while neutral
		private Vector2 oldPosition;  //Last position of ship
		private GameEntity oldTarget;  //Last target of ship	

		/// <summary>
		///  Creates a new AI with given spawnpoint and given environment
		///  Initial state is Neutral
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
			timeSinceChangedTargets = 0;
			waitTimer = 0;
			patrolPoints = new List<Vector2>();
			patrolPoints.Add(randomPatrolPoint());
			oldTarget = null;
			timeSinceMoved = 0;
			oldPosition = new Vector2(-1000, 1000);//I hope this is improbable
			timeSinceSawTarget = 0;
			timeSinceAnsweredDistressCall = 0;
			targetList = new List<GameEntity>();
		}

		/// <summary>
		///  Updates the State of a ship
		/// </summary>
		public void Update(Ship s, float elapsedTime)
		{
			currentShip = s;
			currentState = nextState;
			currentShip.ResetMaxRotVel();//For the turning speed slowdown of neutral state
			if ((oldPosition.Equals(new Vector2(-1000, 1000)) || !oldPosition.Equals(currentShip.Position)))
			{
				timeSinceMoved = 0;
				oldPosition = currentShip.Position;
			}
			else
			{
				timeSinceMoved += elapsedTime;
			}
			timeSinceHitWall += elapsedTime;
			timeSinceChangedTargets += elapsedTime;
			timeSinceLastStateChange += elapsedTime;
			timeSinceAnsweredDistressCall += elapsedTime;
			switch (currentState)
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
			foreach (BlackHole bh in env.blackHoles)  //Black holes are scary, run away!
			{
				float distance = env.BlackHoleController.MaxRadius / GameEnvironment.k_physicsScale + 100;
				if (Vector2.Distance(currentShip.Position, bh.Position) < distance)
				{
					HitWall(bh.Position);
				}
			}
			if (timeSinceMoved > 3) // I hit a ship and it wont move (hopefully)
			{
				timeSinceMoved = 0;
				patrolPoints.RemoveAt(0);
				patrolPoints.Add(randomPatrolPoint());
			}
			waitTimer -= elapsedTime;
			if (waitTimer <= 0)  //No more waiting, time to move
			{
				Vector2 destination = patrolPoints.First();
				currentShip.MaxRotVel = currentShip.MaxRotVel / 2; //Take my time turning
				float wantedDirection = Angle.Direction(currentShip.Position, destination); //Ships want to face the direction their destination is
				if (Vector2.Distance(currentShip.Position, destination) < currentShip.maxSpeed * 0.3f) // Close to destination
				{
					patrolPoints.RemoveAt(0);
					patrolPoints.Add(randomPatrolPoint());
					currentShip.DesiredVelocity = Vector2.Zero;
					waitTimer = (float)(r.NextDouble() * 3 + 0.3);
				}
				else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < MathHelper.PiOver2) //Im almost facing the direction I want to go in
				{
					currentShip.DesiredVelocity = Angle.Vector(currentShip.Rotation) * currentShip.maxSpeed / 3;
					currentShip.DesiredRotation = wantedDirection; 
				}
				else //Im not facing the correct direction
				{
					currentShip.DesiredRotation = wantedDirection;
					currentShip.DesiredVelocity = Angle.Vector(currentShip.Rotation) * currentShip.maxSpeed / 6;
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
			if (CanSee(currentShip, target))
			{
				timeSinceSawTarget = 0;
			}
			else
			{
				timeSinceSawTarget += elapsedTime;
			}
			Vector2 destination = target.Position;  //Current Destination, is set to our target
			float wantedDirection = Angle.Direction(currentShip.Position, destination);  //I like facing my destination when I move

			if (Vector2.Distance(currentShip.Position, destination) < 300)  //I want to keep a certain distance away from my target
			{
				currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection;  //Even though I want to keep a certain distance, I will still turn to face you
			}
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < MathHelper.PiOver2)  //Im almost facing the direction I want to go in
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection; 
			}
			else  //Im not facing my target
			{
				currentShip.DesiredVelocity = Angle.Vector(currentShip.Rotation) * currentShip.maxSpeed / 2;
				currentShip.DesiredRotation = wantedDirection;
			}

			if (timeSinceLastStateChange > 5) //I lose interest in 5 seconds
				changeToNeutral();
			else if (timeSinceSawTarget > 3) //I lose interest if I haven't seen my target for 3 seconds
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

			if (Vector2.Distance(currentShip.Position, destination) < 300) //Keep a certain distance from target
			{
				currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection; //Even though I dont move, I want to face my targets direction
			}
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < MathHelper.PiOver2)  //Im almost facing the direction I want to go in
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection; //Does not count as rotation
			}
			else //Im not facing my target
			{
				currentShip.DesiredVelocity = Angle.Vector(currentShip.Rotation) * currentShip.maxSpeed / 2;
				currentShip.DesiredRotation = wantedDirection;
			}
			//Shoot if I see my target
			if (CanSee(currentShip, target))
			{
				currentShip.Shoot(elapsedTime, target);
				timeSinceSawTarget = 0;
			}
			else
			{
				timeSinceSawTarget += elapsedTime;
			}

			if (currentShip.IsAllied((TakesDamage)target)) //If I cant shoot target. time to move on
				changeToNeutral();
			else if (((TakesDamage)target).IsDead()) //If target is dead, time to move on
				changeToNeutral();
			else if (timeSinceSawTarget > 5) //Lose interest after 5 seconds if I can't see target
				changeToNeutral();
		}

		/// <summary>
		///  AI behavior for Confused State
		///  Looks around for lookingFor
		///  Inputs : lookingFor, currentShip, answeringDistressCall
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
				if (timeSinceLastStateChange > 2) //I spin for 2 second now
				{
					changeToNeutral();
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
			if (!currentShip.IsFrozen && !(currentShip is Tractorable && ((Tractorable)currentShip).IsTractored))
			{
				//Im gonna be suspicious of whoever disabled me
				if(CanSee(currentShip,target))
					changeToAlert(target);
				else
					changeToConfused(target, false);
			}
		}

		/// <summary>
		///  AI behavior for Allied State
		///  Inputs : currentShip, target
		///  Outputs : nextState
		/// </summary>
		private void Allied(float elapsedTime)
		{
			Vector2 destination = target.Position - (Angle.Vector(target.Rotation) * 200);  //Im going behind my target
			float wantedDirection = Angle.Direction(currentShip.Position, destination); //Face towards destination
			if (Vector2.Distance(currentShip.Position, destination) < 100) //Keep a certain distance from target
			{
				currentShip.DesiredVelocity = Vector2.Zero;
				currentShip.DesiredRotation = wantedDirection; //Even though I dont move, I want to face my targets direction
			}
			else if (Angle.DistanceMag(currentShip.Rotation, wantedDirection) < MathHelper.PiOver2) // Im nearly facing my target
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed;
				currentShip.DesiredRotation = wantedDirection;
			}
			else //Im not facing my target
			{
				currentShip.DesiredVelocity = Angle.Vector(wantedDirection) * currentShip.maxSpeed / 2;
				currentShip.DesiredRotation = wantedDirection;
			}

			while (targetList.Count > 0 && ((TakesDamage)targetList.First()).IsDead())
			{
				targetList.RemoveAt(0);
			}
			
			if (targetList.Count() > 0)
			{
				float targetDirection = Angle.Direction(currentShip.Position, targetList.First().Position);
				currentShip.shooter.Rotation = targetDirection;
				currentShip.Shoot(elapsedTime);
			}
			else
			{
				GamePadState gamepad = GamePad.GetState(PlayerIndex.One);
				if (gamepad.IsConnected)
				{
					const float k_aimRadius = 250.0f;
					Vector2 invertY = new Vector2(1.0f, -1.0f);
					float gamePadDirection = Angle.Direction(currentShip.Position, gamepad.ThumbSticks.Right * k_aimRadius * invertY);
					currentShip.shooter.Rotation = gamePadDirection;
				}
				else
				{
					MouseState mouse = Mouse.GetState();
					Vector2 mousePosition = env.Camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));
					float mouseDirection = (Angle.Direction(currentShip.Position, mousePosition));
					currentShip.shooter.Rotation = mouseDirection;
				}
				if (((Ship)target).isShooting)
					currentShip.Shoot(elapsedTime);
			}

			if (CanSee(currentShip, target))
			{
				timeSinceSawTarget = 0;
			}
			else
			{
				timeSinceSawTarget += elapsedTime;
			}
			//Is my target dead
			if (((TakesDamage)target).IsDead())
			{
				changeToNeutral();
			}
			else if (target != env.sputnik.controlled) //We don't follow non sputnik people
			{
				changeToNeutral();
			}
			else if (timeSinceSawTarget > 30)  //I can't see my target, go look for him
				changeToConfused(target,true);
		}

		/// <summary>
		///  Call when controlled ship gets frozen by s
		/// </summary>
		public void GotFrozen(GameEntity s)
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			changeToDisabled(s);
		}

		/// <summary>
		///  Call when controlled ship gets tractored by s
		/// </summary>
		public void GotTractored(GameEntity s)
		{
			currentShip.DesiredVelocity = Vector2.Zero;
			changeToDisabled(s);
		}

		/// <summary>
		/// Ship was just teleported through a blackhole.
		/// </summary>
		/// <param name="blackhole">Start blackhole.</param>
		/// <param name="destination">Position of destination.</param>
		public void Teleport(BlackHole blackhole, Vector2 destination) {
			
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
				// If we are touching, we can see each other
				return true;
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
			//Ignore Bullets
			if (fixture.Body.IsBullet)
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
		/// </summary>
		public void GotShotBy(Ship s, GameEntity f)
		{
			if (currentState != State.Disabled && currentState != State.Allied) //Allied and Disabled ships don't react
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
							if (timeSinceChangedTargets > 20) // If i haven't killed my target in 20 seconds, I should change to an easier target
							{
								changeToHostile(f);
							}
							break;
						default:
							//current do nothing if in Disabled or Hostile when shot
							break;
					}
				}
				else 
				{
					if (currentState == State.Hostile)
					{
						if (timeSinceChangedTargets > 20) // If i haven't killed my target in 20 seconds, I should change to an easier target
						{
							changeToConfused(f, false);
						}
					}
					else
					{
						if (timeSinceChangedTargets > 10 || s == currentShip) //Dont get confused too often, unless its me shot
						{
							changeToConfused(f, false);
						}
					}
				}
			}
		}

		/// <summary>
		///  Call when controlled ship hits a wall
		/// </summary>
		public void HitWall(Vector2 collidePosition)
		{
			if (currentState != State.Allied && timeSinceHitWall > 1 && currentState != State.Disabled) //Allied ships and ships that recently hit a wall ignore hitting a wall
			{
				timeSinceHitWall = 0;
				if (currentState == State.Neutral)
				{
					//With no pathfinding, this is probably the best I can do
					//Worst case seems to be if this happens in a corner
					float collideAngle = Angle.Direction(currentShip.Position, collidePosition);
					//I HATE that coordinate plane of screen is not a math coordinate plane
					if (collideAngle < -MathHelper.PiOver2)
						spawn.TopLeft = collidePosition;
					else if (collideAngle < 0)
						spawn.TopRight = collidePosition;
					else if (collideAngle < MathHelper.PiOver2)
						spawn.BottomRight = collidePosition;
					else
						spawn.BottomLeft = collidePosition;
					patrolPoints.RemoveAt(0);
					patrolPoints.Add(randomPatrolPoint());
				}
				else
				{
					//There is no pathfinding, might as well give up
					changeToNeutral();
				}
			}
		}

		/// <summary>
		///  Call when sputnik's controlled ship (s) gets shot by f
		/// </summary>
		public void DistressCall(Ship s, GameEntity f)
		{
			if(currentState == State.Allied)
			{
				targetList.Add(f);
			}
				
			//Don't answer distress calls too often or if you are a circloid ship and the shooter is a boss
			else if (timeSinceAnsweredDistressCall > 5 && !(currentShip is CircloidShip && f is SaphereBoss))
			{
				timeSinceAnsweredDistressCall = 0;
				if (currentState == State.Neutral && !s.IsAllied((TakesDamage)f)) //Only non busy ships (neutral) answer
				{//Don't answer of some weird civil war is going on
					if (CanSee(currentShip, s))//If i can see sputniks ship, go help him 
					{
						changeToAllied(s);
						targetList.Add(f);
					}
					else  //If I can't see Sputnik's ship, go look for it.
					{
						changeToConfused(s, true);
						targetList.Add(f);
					}
				}
			}
		}

		/// <summary>
		///  Call when sputnik detaches from this ship
		/// </summary>
		public void gotDetached()
		{
			spawn.Position = spawn.Entity.Position;
			//Its probably a good idea to generate a new patrolpoint for the new spawn
			patrolPoints.RemoveAt(0); 
			patrolPoints.Add(randomPatrolPoint());
			changeToNeutral(); //Ships that are freed go to neutral
		}

		/// <summary>
		///  Tells if this ship is allied with player
		/// </summary>
		public bool IsAlliedWithPlayer()
		{
			return currentState == State.Allied;
		}

		/// <summary>
		///  Tells if this ship is disabled
		/// </summary>
		public bool IsDisabled()
		{
			return currentState == State.Disabled;
		}

		/// <summary>
		///  returns a random point within spawn point
		/// </summary>
		private Vector2 randomPatrolPoint()
		{
			return new Vector2(
			r.Next((int)spawn.TopLeft.X, (int)spawn.TopRight.X),
			r.Next((int)spawn.TopLeft.Y, (int)spawn.BottomRight.Y)
			);
		}

		/// <summary>
		///  Method to call to change to neutral state
		/// </summary>
		private void changeToNeutral()
		{
			timeSinceSawTarget = 0;
			oldTarget = target;
			target = null;
			lookingFor = null;
			currentState = State.Neutral;
			nextState = State.Neutral;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			targetList.Clear();
		}

		/// <summary>
		///  Method to call to change to alert state with target t
		/// </summary>
		private void changeToAlert(GameEntity t)
		{
			if (currentState != State.Alert)
			{
				env.AlertEffect.Trigger(currentShip.Position);
			}
			timeSinceSawTarget = 0;
			oldTarget = target;
			target = t;
			lookingFor = null;
			currentState = State.Alert;
			nextState = State.Alert;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			targetList.Clear();
		}

		/// <summary>
		///  Method to call to change to hostile state with target t
		/// </summary>
		private void changeToHostile(GameEntity t)
		{
			timeSinceSawTarget = 0;
			oldTarget = target;
			target = t;
			lookingFor = null;
			currentState = State.Hostile;
			nextState = State.Hostile;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			targetList.Clear();
		}

		/// <summary>
		///  Method to call to change to allied state with target t
		/// </summary>
		private void changeToAllied(GameEntity t)
		{
			timeSinceSawTarget = 0;
			oldTarget = target;
			target = t;
			lookingFor = null;
			currentState = State.Allied;
			nextState = State.Allied;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
		}

		/// <summary>
		///  Method to call to change to disabled state
		/// </summary>
		private void changeToDisabled(GameEntity t)
		{
			timeSinceSawTarget = 0;
			oldTarget = target;
			target = t;
			lookingFor = null;
			currentState = State.Disabled;
			nextState = State.Disabled;
			answeringDistressCall = false;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
			targetList.Clear();
		}

		/// <summary>
		///  Method to call to change to confused state while looking for l and if adc(AnsweringDistressCall)
		/// </summary>
		private void changeToConfused(GameEntity l, bool adc)
		{
			timeSinceSawTarget = 0;
			oldTarget = target;
			target = null;
			lookingFor = l;
			currentState = State.Confused;
			nextState = State.Confused;
			answeringDistressCall = adc;
			timeSinceLastStateChange = 0;
			timeSinceChangedTargets = 0;
		}
	}
}

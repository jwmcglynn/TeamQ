﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

// Comment out later
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Ship : GameEntity, TakesDamage
	{
		internal ShipController ai;
		private ShipController previousAI = null;
		public int health = 100;
		private bool m_shouldCull = false;
		public float shooterRotation;
		protected BulletEmitter shooter = null;
		protected Ship attachedShip = null;
		public float maxSpeed = 350.0f;
		public bool isShooting;
		public bool isFrozen;
		public bool isTractored;
		public Ship tractoringShip;
		protected float passiveShield;
		protected SpawnPoint spawn;

		// Smooth rotation.
		public float DesiredRotation;
		private float m_lastRotDir = 1.0f;
		public float MaxRotVel = 2.0f * (float) Math.PI;

		public void ResetMaxRotVel() {
			MaxRotVel = 2.0f * (float) Math.PI; // Up to 1 rotation per second.
		}

		//Used for friendliness
		bool sputnikDetached;
		float timeSinceDetached;

		public Ship(GameEnvironment env, Vector2 pos)
				: base(env)
		{
			Position = pos;
			shooterRotation = Rotation;
			sputnikDetached = false;
			timeSinceDetached = 0;
			VisualRotationOnly = true;
		}

		public Ship(GameEnvironment env, SpawnPoint sp)
				: base(env, sp)
		{
			sputnikDetached = false;
			timeSinceDetached = 0;
			spawn = sp;
			VisualRotationOnly = true;
		}

		public override void Update(float elapsedTime)
		{
			if (sputnikDetached)
				timeSinceDetached += elapsedTime;
			isShooting = false;
			if (ai != null)
			{
				ai.Update(this, elapsedTime);
			}
			if (shooter != null && !isFrozen)
			{
				// Update emitter position.
				shooter.Rotation = shooterRotation;
				shooter.Position = Position;
			}

			if (Rotation != DesiredRotation && !isFrozen) {
				float distPos = Angle.Distance(DesiredRotation, Rotation);
				float dir = Math.Sign(distPos);

				if (Math.Abs(distPos) > Math.PI * 3 / 4) dir = m_lastRotDir;
				if (dir != 0) m_lastRotDir = dir;

				float del = dir * MaxRotVel * elapsedTime;
				if (Math.Abs(del) > Math.Abs(distPos)) del = distPos;
				Rotation += del;
			}
			shooterRotation = Rotation;
			base.Update(elapsedTime);
		}

		// Attach Sputnik to the ship
		public virtual void Attach(SputnikShip sp)
		{
			this.previousAI = this.ai;
			this.ai = sp.GetAI();
			this.attachedShip = sp;
			isFrozen = false;
			isTractored = false;
			sputnikDetached = false;
			timeSinceDetached = 0;
		}

		public virtual void Detach()
		{
			this.ai = this.previousAI;
			this.attachedShip.Detach();
			this.attachedShip = null;
			sputnikDetached = true;
			timeSinceDetached = 0;
			spawn.Position = Position;
			ai.gotDetached();
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			return !(entB is Ship) || (entB is SputnikShip);
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is Bullet)
			{
				//Horrible Casting makes me sad.
				ai.GotShotBy(this, (GameEntity)((Bullet)entB).owner);
			}
			else if (entB is Environment)
			{
				ai.HitWall();
			}
				
			base.OnCollide(entB, contact);
		}

		public override bool ShouldCull() {
			if (m_shouldCull) return true;
			if (attachedShip != null && this is TriangulusShip) return false;
			
			return !InsideCullRect(Rectangle.Union(VisibleRect, SpawnPoint.Rect));
		}

		public virtual void TakeHit(int damage)
		{
			if(this is SquaretopiaShip) {
				if(passiveShield > 0) {
					passiveShield -= damage;
				} else {
					health -= damage;
				}
			} else {
				health -= damage;
			}
			if (health < 1) {
				InstaKill();
			}
		}

		public virtual void Shoot(float elapsedTime)
		{
			shooter.Shoot(elapsedTime);
			isShooting = true;
		}

		public virtual void Shoot(float elapsedTime, GameEntity target)
		{
			shooter.Shoot(elapsedTime);
			isShooting = true;
			if (target == Environment.sputnik.controlled)
				Environment.sputnik.controlled.ai.GotShotBy(Environment.sputnik.controlled, this);
		}

		public bool isSputnik()
		{
			if (this.ai is PlayerController)
				return true;
			return false;
		}

		public void InstaKill()
		{
			// Perform what ever actions are necessary to 
			// Destory a ship
			// TODO: Animations / explosions.
			this.health = 0;
			m_shouldCull = true;
		}

		//Tells if this is friendly to s
		public virtual bool IsFriendly(Ship s)
		{
			if (this.Equals(s)) //Im always friendly to myself
				return true;
			else if (s == Environment.sputnik.controlled ) //Nobody is ever friendly to Sputnik's ship unless they are allied
				return ai.IsAlliedWithPlayer();
			else if (Environment.sputnik.controlled == this) //Sputnik is friendly to nobody
				return false;
			else {
				if (this.GetType().Equals(s.GetType()))
				{
					if (s.sputnikDetached)
						return s.timeSinceDetached > 3;  //Friendly if Sputnik detached and 3 seconds passed
					else
						return true; //Otherwise friendly since both are same type
				}
				else{
					if (ai is AIController)
					{
						return ((AIController)ai).Target != s;
					}
					else
					{
						return false;  //If we get here, playercontroller hates everyone
					}
					}
			}
		}

		public virtual bool IsFriendly(Boss s)
		{
			if (ai is AIController)
			{
				//GDD says so, so allied circloid ships do this too
				return ((AIController)ai).Target != s;
			}
			else
			{
				return false;
			}
		}
	}
}

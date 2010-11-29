using System;
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
		public float maxSpeed = 200.0f;
		public bool isShooting;
		public bool isFrozen;
		public bool isTractored;
		public Ship tractoringShip;

		protected Rectangle m_patrolRect;

		// Smooth rotation.
		public float DesiredRotation;
		private float m_lastRotDir = 1.0f;
		public float MaxRotVel = 3.0f * (float) Math.PI; // Up to 1.5 rotations per second.

		public Ship(GameEnvironment env, Vector2 pos)
				: base(env)
		{
			Position = pos;
			shooterRotation = Rotation;
		}

		public Ship(GameEnvironment env, SpawnPoint sp)
				: base(env, sp)
		{
		}

		public override void Update(float elapsedTime)
		{
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
			isShooting = false;
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
		}

		public virtual void Detach()
		{
			this.ai = this.previousAI;
			this.attachedShip.Detach();
			this.attachedShip = null;
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			/*This results in silliness, we collide all the time now
			if (fixture.IsSensor || entBFixture.IsSensor) return true;
			return !(entB is Ship) || (entB is SputnikShip);
			 */
			return true;
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

			if (m_patrolRect != null) {
				return !InsideCullRect(Rectangle.Union(VisibleRect, m_patrolRect));
			} else {
				return !InsideCullRect(VisibleRect);
			}
		}

		public virtual void TakeHit(int damage)
		{
			this.health -= damage;
			if(this.health < 1) {
				this.InstaKill();
			}
		}

		public virtual void Shoot(float elapsedTime)
		{
			shooter.Shoot(elapsedTime, IsFriendly());
			isShooting = true;
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

		public virtual bool IsFriendly()
		{
			return (this.ai is PlayerController);
		}
	}
}

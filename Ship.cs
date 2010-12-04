using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

// Comment out later
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury;

namespace Sputnik
{
	class Ship : GameEntity, TakesDamage
	{
		internal ShipController ai;
		private ShipController previousAI = null;
		public int health = 100;
		public float shooterRotation;
		internal BulletEmitter shooter = null;
		protected Ship attachedShip = null;
		public float maxSpeed = 350.0f;
		public bool isShooting;
		public bool isFrozen;
		public Ship tractoringShip;
		protected float passiveShield;
		private bool m_isDead = false;

		public Vector2 RelativeShooterPos = Vector2.Zero;

		// Smooth rotation.
		public float DesiredRotation;
		private float m_lastRotDir = 1.0f;
		public float MaxRotVel = 2.0f * (float)Math.PI;

		public void ResetMaxRotVel()
		{
			MaxRotVel = 2.0f * (float)Math.PI; // Up to 1 rotation per second.
		}

		private static int k_zindexOffset = 0;

		//Used for friendliness
		bool sputnikDetached;
		float timeSinceDetached;

		public Ship(GameEnvironment env, Vector2 pos)
			: base(env)
		{
			Position = pos;
			Initialize();
		}

		public Ship(GameEnvironment env, SpawnPoint sp)
			: base(env, sp)
		{
			Initialize();
		}

		private void Initialize()
		{
			shooterRotation = Rotation;
			sputnikDetached = false;
			timeSinceDetached = 0;
			ResetZIndex();
		}

		public void ResetZIndex()
		{
			Zindex = 0.5f + MathHelper.Clamp((float)k_zindexOffset / 100000.0f, 0.0f, 0.5f);
			++k_zindexOffset;
		}

		public override void Update(float elapsedTime)
		{
			if (timeSinceDetached > 3)
			{
				sputnikDetached = false;
				timeSinceDetached = 0;
			}
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

				Matrix rotMatrix = Matrix.CreateRotationZ(Rotation);
				shooter.Position = Position + Vector2.Transform(RelativeShooterPos, rotMatrix);
			}

			if (Rotation != DesiredRotation && !isFrozen && !((this is Tractorable) && ((Tractorable) this).IsTractored)) {
				float distPos = Angle.Distance(DesiredRotation, Rotation);
				float dir = Math.Sign(distPos);

				if (Math.Abs(distPos) > Math.PI * 3 / 4) dir = m_lastRotDir;
				if (dir != 0) m_lastRotDir = dir;

				float del = dir * MaxRotVel;
				if (Math.Abs(del * elapsedTime) > Math.Abs(distPos)) del = distPos * 10.0f;

				if (CollisionBody != null) {
					CollisionBody.AngularVelocity = del;
				} else {
					Rotation += del * elapsedTime;
				}
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
			if (this is Tractorable) ((Tractorable)this).IsTractored = false;
			sputnikDetached = false;
			timeSinceDetached = 0;
			Zindex = 0.26f;

			sp.SputnikAttach(this);
		}

		public virtual void Detach()
		{
			if (attachedShip == null) return;

			this.ai = this.previousAI;
			this.attachedShip.Detach();
			this.attachedShip = null;
			sputnikDetached = true;
			timeSinceDetached = 0;
			SpawnPoint.Position = Position;
			ai.gotDetached();
			ResetZIndex();
		}

		public bool AvoidShips {
			get {
				return !(this is SputnikShip) && !isFrozen && !((this is Tractorable) && ((Tractorable) this).IsTractored);
			}
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
			else if (entB is Environment || entB.CollisionBody.IsStatic)
			{
				FarseerPhysics.Collision.WorldManifold manifold;
				contact.GetWorldManifold(out manifold);
				if (ai != null) ai.HitWall(manifold.Points[0] * GameEnvironment.k_invPhysicsScale);
			}

			base.OnCollide(entB, contact);
		}

		public bool IsDead()
		{
			return m_isDead;
		}

		public override bool ShouldCull()
		{
			if (attachedShip != null && this is TriangulusShip) return false;

			return !InsideCullRect(Rectangle.Union(VisibleRect, SpawnPoint.Rect));
		}

		public virtual void TakeHit(int damage)
		{
			if (this is SquaretopiaShip)
			{
				if (passiveShield > 0)
				{
					passiveShield -= damage;
				}
				else
				{
					health -= damage;
				}
			}
			else
			{
				health -= damage;
			}
			if (health < 1)
			{
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
			m_isDead = true;
			this.health = 0;
			OnNextUpdate += () => Dispose();

			Environment.ExplosionEffect.Trigger(Position);
		}

		/// <summary>
		/// Is this ship a member of the player's clique?
		/// </summary>
		/// <returns></returns>
		public bool IsFriendly() {
			return ai.IsAlliedWithPlayer();
		}

		//Tells if this is allied to s
		public bool IsAllied(TakesDamage s) {
			if (this == s) return true;
			if (this is CircloidShip && s is SaphereBoss) return true;

			// If both are allied.
			if (IsFriendly() && s.IsFriendly()) return true;

			if (GetType() != s.GetType()) return false;

			// Both are not part of player's clique.
			if (s is Ship) {
				Ship ship = (Ship) s;
				return !ship.sputnikDetached || ship.timeSinceDetached >= 3.0f; //Allied if Sputnik detached and 3 seconds passed
			}

			return false;
		}
	}
}

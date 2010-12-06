using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using ProjectMercury;

namespace Sputnik
{
	class Ship : GameEntity, TakesDamage
	{
		internal ShipController ai;
		private ShipController previousAI = null;
		protected int MaxHealth = 30;
		public int health;
		public float shooterRotation;
		internal BulletEmitter shooter = null;
		protected Ship attachedShip = null;
		public float maxSpeed = 350.0f;
		public bool isShooting;
		protected int m_frozenCount;
		public Ship tractoringShip;
		private bool m_isDead = false;

		private float m_colorTimer = 0.0f; // 0 for non-friendly color, 1 for friendly color.  Used for strobing effect.
		private float m_colorDir = 1.0f; // Direction of fading.
		private Color m_tintColor = Color.White;

		public Vector2 RelativeShooterPos = Vector2.Zero;

		public bool IsFrozen {
			get {
				return (m_frozenCount > 0);
			}
		}

		// Smooth rotation.
		public float DesiredRotation;
		private float m_lastRotDir = 1.0f;
		public float MaxRotVel = 1.5f * (float)Math.PI;

		public void ResetMaxRotVel()
		{
			MaxRotVel = 1.5f * (float)Math.PI; // Up to 3/4 rotation per second.
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
			SpawnPoint.RespawnCooldown = 0.0f;
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
			if (shooter != null && !IsFrozen)
			{
				// Update emitter position.
				shooter.Rotation = shooterRotation;

				Matrix rotMatrix = Matrix.CreateRotationZ(Rotation);
				shooter.Position = Position + Vector2.Transform(RelativeShooterPos, rotMatrix);
			}

			///// Change color based on friendliness and frozen-ness.
			const float s_colorVel = 1.0f;
			bool friendTimer = (IsFriendly() && !(this is SputnikShip));
			bool frozenTimer = (IsFrozen || (this is Tractorable && ((Tractorable) this).IsTractored));

			if (friendTimer) m_tintColor = new Color(0.6f, 1.0f, 0.6f); // Green.
			if (frozenTimer) m_tintColor = new Color(1.0f, 0.8f, 0.0f); // Yellow.

			if (friendTimer || frozenTimer) {
				m_colorTimer += m_colorDir * (s_colorVel * elapsedTime);
				
				// Handle changing fade directions.
				if (m_colorDir > 0.0f && m_colorTimer >= 1.0f) {
					m_colorDir *= -1.0f;
					m_colorTimer = 1.0f;
				} else if (m_colorDir < 0.0f && m_colorTimer <= 0.5f) {
					m_colorDir *= -1.0f;
					m_colorTimer = 0.5f;
				}

				VertexColor = m_tintColor;
			} else {
				m_colorDir = 1.0f;
				m_colorTimer -= s_colorVel * elapsedTime;
				if (m_colorTimer < 0.0f) m_colorTimer = 0.0f;
			}

			if (m_colorTimer == 0.0f) {
				VertexColor = Color.White;
			} else {
				VertexColor = Color.Lerp(Color.White, m_tintColor, m_colorTimer);
			}

			///// Smooth rotation.
			if (IsFrozen || ((this is Tractorable) && ((Tractorable) this).IsTractored)) {
				// Don't smooth rotate when tractored or frozen.
			} else if (Rotation != DesiredRotation) {
				float distPos = Angle.Distance(DesiredRotation, Rotation);
				float dir = Math.Sign(distPos);

				if (Math.Abs(distPos) > Math.PI * 3 / 4) dir = m_lastRotDir;
				if (dir != 0) m_lastRotDir = dir;

				float del = dir * MaxRotVel;
				if (Math.Abs(del * elapsedTime) > Math.Abs(distPos)) del = distPos;

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
			m_frozenCount = 0;
			if (this is Tractorable) ((Tractorable)this).IsTractored = false;
			sputnikDetached = false;
			timeSinceDetached = 0;
			Zindex = 0.26f;

			if (this.health < this.MaxHealth / 2)
				this.health = this.MaxHealth / 2;

			sp.SputnikAttach(this);
		}

		public virtual float HealthPercent {
			get {
				return (float) health / MaxHealth;
			}
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
				return !(this is SputnikShip) && !IsFrozen && !((this is Tractorable) && ((Tractorable) this).IsTractored);
			}
		}

		public override void Teleport(BlackHole blackhole, Vector2 destination, Vector2 exitVelocity) {
			if (ai != null) ai.Teleport(blackhole, destination);
			base.Teleport(blackhole, destination, exitVelocity);
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
			Sound.PlayCue("hit_by_bullet", this);

			if (isSputnik() && this.Environment.isFrostMode) return;

			health -= damage;
			
			if (health < 1) InstaKill();
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
			m_isDead = true;
			this.health = 0;

			OnNextUpdate += () => {
				Dispose();
				Environment.ExplosionEffect.Trigger(Position);
				Sound.PlayCue("explosion", this);
			};
		}

		public override void OnCull() {
			if (m_isDead) SpawnPoint.RespawnCooldown = 30.0f;
			base.OnCull();
		}

		/// <summary>
		/// Is this ship a member of the player's clique?
		/// </summary>
		/// <returns></returns>
		public bool IsFriendly() {
			return (ai != null) && ai.IsAlliedWithPlayer();
		}

		//Tells if this is allied to s
		public bool IsAllied(TakesDamage s) {
			if (this == s)
			{
				return true; //I like myself
			}
			else if (this == Environment.sputnik.controlled) //Im sputnik
			{
				return s.IsFriendly(); //Sputnik only likes his friendly buddies
			}
			else if (s == Environment.sputnik.controlled) //The target is sputnik
			{
				return IsFriendly(); //Nobody but allied ships like sputnik
			}
			else if (s is Ship) //Im a ship, and I better have the AI controlling me and my target is a ship
			{
				if (((Ship)s).ai.IsDisabled())
				{
					return false;  //No mercy for the weak
				}
				else if (!(ai is AIController))
					return false;  //This case is strange, I'm actually not sure when this happens, but I don't like it
				else if (((AIController)ai).target == s) //You are my target
				{
					if (((Ship)s).sputnikDetached) //We forgive you if sputnik did bad things to you
						return ((Ship)s).timeSinceDetached >= 3.0f;
					else
						return false;  //Otherwise we don't like you
				}
				else
					return true;  //You aren't my target, you are okay
			}
			else if (s is Boss)  //Im a ship and my target is a boss
			{
				return !IsFriendly(); //Only friendly people hate boss
			}
			else
			{
				return false;  //If its not a ship or a boss, we don't like it
			}
		}
	}
}

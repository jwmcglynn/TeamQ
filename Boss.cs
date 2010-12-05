using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Sputnik
{
	class Boss : GameEntity, TakesDamage
	{
		protected const int MaxHP = 400;
		protected int health = MaxHP;
		protected BulletEmitter bm1, bm2, bm3;
		protected BossAI ai;
		protected bool isShooting = false;
		public bool useSpecial = false;
		public float maxSpeed = 50.0f;
		public float maxTurn = 0.025f;
		private Vector2 
			top = new Vector2(0, -75),
			left = new Vector2(-75, 40), 
			right = new Vector2(75, 40);
		protected float shooterRotation = 1.5f;
		protected GameEnvironment env;
		Fixture takesDamage, sensor;
		protected Vector2 target;
		protected SpawnPoint sp = null;
		private GameEntity shootTarget;
		public GameEntity ShootTarget { get { return shootTarget; } }
		bool m_isDead = false;
		protected bool isUnhappy = false;

		public Boss(GameEnvironment env) : base(env)
		{
			initialize(env);
		}

		public Boss(GameEnvironment env, SpawnPoint sp) : base(env, sp)
		{
			initialize(env);
			this.sp = sp;
			Position = sp.Position;
		}

		protected virtual void initialize(GameEnvironment env)
		{
			this.env = env;
			this.bm1 = new BulletEmitter(env, this, BulletEmitter.BulletStrength.Medium);
			this.bm2 = new BulletEmitter(env, this, BulletEmitter.BulletStrength.Medium);
			this.bm3 = new BulletEmitter(env, this, BulletEmitter.BulletStrength.Medium);

			AddChild(bm1);
			AddChild(bm2);
			AddChild(bm3);

			Registration = new Vector2(465.0f, 465.0f);
			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			Zindex = 0.3f;

			takesDamage = AddCollisionCircle(170.0f, Vector2.Zero);
			sensor = AddCollisionCircle(Texture.Width * 1.25f, Vector2.Zero);

			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true;

			Vector2[] temp;

			if (env.SpawnedBossPatrolPoints.Count == 0)
			{
				temp = new Vector2[4];
				temp[0] = new Vector2(env.ScreenVirtualSize.X, env.ScreenVirtualSize.Y);
				temp[1] = new Vector2(0f, env.ScreenVirtualSize.Y);
				temp[2] = new Vector2(0, 0);
				temp[3] = new Vector2(env.ScreenVirtualSize.X, 0);
			}
			else
			{
				temp = new Vector2[env.SpawnedBossPatrolPoints.Count];
				for (int i = 0; i < temp.Length; ++i)
					temp[i] = env.SpawnedBossPatrolPoints.ElementAt(i);

			}
			this.ai = new BossAI(env, this, temp);
		}

		public override void Update(float elapsedTime)
		{
			if (useSpecial)
				ShootSpecial(target);
			ai.Update(elapsedTime);

			base.Update(elapsedTime);

			shooterRotation = (float)Math.Atan2(env.sputnik.Position.Y - this.Position.Y, env.sputnik.Position.X - this.Position.X);

			bm1.Position = this.Position + top;
			bm1.Rotation = shooterRotation;
			bm2.Position = this.Position + right;
			bm2.Rotation = shooterRotation;
			bm3.Position = this.Position + left;
			bm3.Rotation = shooterRotation;
		}

		public bool Shooting
		{
			get
			{
				return this.isShooting;
			}
		}

		public bool IsFriendly() {
			// Boss != player.
			return false;
		}

		public bool IsAllied(TakesDamage s) {
			return (s != shootTarget);
		}

		public void Shoot(float elapsedTime)
		{
			bm1.Shoot(elapsedTime);
			bm2.Shoot(elapsedTime);
			bm3.Shoot(elapsedTime);
		}

		public void InstaKill()
		{
			// Do nothing.
		}

		public bool IsDead()
		{
			return m_isDead;
		}

		public override bool ShouldCull()
		{
			return false;
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture)
		{
			return ((fixture == takesDamage) && (entB is Bullet)) || ((entB is Ship) && fixture == sensor);
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (contact.FixtureA == sensor || contact.FixtureB == sensor) contact.Enabled = false;

			if (entB is Ship && ((Ship) entB).IsFriendly()) {
				isShooting = true;
				shooterRotation = (float)Math.Atan2(entB.Position.Y - this.Position.Y, entB.Position.X - this.Position.X);
				shootTarget = (Ship) entB;
				target = entB.Position;
			}

			base.OnCollide(entB, contact);
		}

		public override void OnSeparate(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB == shootTarget) {
				isShooting = false;
				shootTarget = null;
			}
			base.OnSeparate(entB, contact);
		}

		protected virtual void ShootSpecial(Vector2 position)
		{
		}

		public virtual void TakeHit(int damage)
		{
			Sound.PlayCue("hit_by_bullet", this);

			this.health -= damage;
			if (this.health < 1) {
				SpawnPoint.AllowRespawn = false;
				// TODO: Cool death sequence?
				// TODO: End game screen.
				OnNextUpdate += () => Dispose();
				m_isDead = true;
			}
		}
	}
}

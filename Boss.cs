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
		private int health = 100;
		protected BulletEmitter bm1, bm2, bm3;
		protected BossAI ai;
		private bool m_shouldCull = false;
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

		public Boss(GameEnvironment env) : base(env)
		{
			initialize(env);
		}

		public Boss(GameEnvironment env, SpawnPoint sp) : base(env, sp)
		{
			initialize(env);
		}

		protected virtual void initialize(GameEnvironment env)
		{
			this.env = env;
			this.bm1 = new BulletEmitter(env, this, BulletEmitter.BulletStrength.Medium, false);
			this.bm2 = new BulletEmitter(env, this, BulletEmitter.BulletStrength.Medium, false);
			this.bm3 = new BulletEmitter(env, this, BulletEmitter.BulletStrength.Medium, false);

			AddChild(bm1);
			AddChild(bm2);
			AddChild(bm3);

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			
			takesDamage = AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			sensor = AddCollisionCircle(Texture.Width * 1.5f, Vector2.Zero);

			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true;
		}

		public override void Update(float elapsedTime)
		{
			ai.Update(elapsedTime);

			base.Update(elapsedTime);

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

		public bool IsFriendly()
		{
			return false;
		}

		public void Shoot(float elapsedTime)
		{
			bm1.Shoot(elapsedTime, false);
			bm2.Shoot(elapsedTime, false);
			bm3.Shoot(elapsedTime, false);
		}

		public void InstaKill()
		{
			m_shouldCull = true;
		}

		public override bool ShouldCull()
		{
			if (m_shouldCull) return true;
			return base.ShouldCull();
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture)
		{
			return ((fixture == takesDamage) && (entB is Bullet)) || ((entB is SputnikShip) && fixture == sensor);
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			base.OnCollide(entB, contact);

			if (entB is SputnikShip)
			{
				isShooting = true;
				shooterRotation = (float)Math.Atan2(entB.Position.Y - this.Position.Y, entB.Position.X - this.Position.X);

				if (useSpecial)
					ShootSpecial(entB.Position);
			}
		}

		public override void OnSeparate(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is SputnikShip)
			{
				isShooting = false;
			}
			base.OnSeparate(entB, contact);
		}

		protected virtual void ShootSpecial(Vector2 position)
		{

		}

		public void TakeHit(int damage)
		{
			this.health -= damage;
			if (this.health < 1)
				InstaKill();
		}
	}
}

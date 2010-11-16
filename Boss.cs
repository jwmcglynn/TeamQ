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
		public float maxSpeed = 50.0f;
		public float maxTurn = 0.025f;
		private Vector2 
			top = new Vector2(0, -75),
			left = new Vector2(-75, 40), 
			right = new Vector2(75, 40);


		public Boss(GameEnvironment env) : base(env)
		{

		}

		public Boss(GameEnvironment env, SpawnPoint sp) : base(env, sp)
		{
			
		}

		public override void Update(float elapsedTime)
		{
			ai.Update(elapsedTime);

			base.Update(elapsedTime);

			bm1.Position = this.Position + top;
			bm2.Position = this.Position + right;
			bm3.Position = this.Position + left;
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
			return (entB is Bullet);
		}

		public void TakeHit(int damage)
		{
			this.health -= damage;
			if (this.health < 1)
				InstaKill();
		}
	}
}

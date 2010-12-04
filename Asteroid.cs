using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Asteroid : GameEntity, Tractorable {
		private bool m_tractored;
		public bool IsTractored { get { return m_tractored; } set { m_tractored = value; } }

		private Vector2 m_tractorTarget;
		private bool m_fling = false;
		private float m_flingTime = 0.0f;

		Ship tractoringShip;

		public Asteroid(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;

			LoadTexture(Environment.contentManager, "astroid_1");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			Zindex = 0.4f;

			CreateAsteroidCollision(true);
		}

		void CreateAsteroidCollision(bool makeStatic) {
			CreateCollisionBody(Environment.CollisionWorld, makeStatic ? FarseerPhysics.Dynamics.BodyType.Static : FarseerPhysics.Dynamics.BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Height / 3, Vector2.Zero);
		}

		public override void Update(float elapsedTime) {
			if (m_fling) {
				m_flingTime -= elapsedTime;
				if (m_flingTime < 0.0f) {
					m_fling = false;
					IsTractored = false;

					DestroyCollisionBody();
					CreateAsteroidCollision(true);
				}

			} else if (IsTractored) {
				const float moveSpeed = 500.0f;

				Vector2 dir = (m_tractorTarget - Position);
				float distance = dir.Length();

				if (distance < moveSpeed * elapsedTime) DesiredVelocity = dir * 60.0f;
				else DesiredVelocity = Vector2.Normalize(dir) * moveSpeed;
			}

			base.Update(elapsedTime);
		}

		public override bool ShouldCull() {
			if (IsTractored) return false;
			return base.ShouldCull();
		}

		public void TractorReleased() {
			m_fling = true;
			m_flingTime = 1.0f;

			DestroyCollisionBody();
			CreateAsteroidCollision(false);
			CollisionBody.LinearDamping = 0.0f;
			CollisionBody.ApplyAngularImpulse(CollisionBody.Mass * RandomUtil.NextFloat(-5.0f, 5.0f));
		}

		public void Tractored(Ship s){
			tractoringShip = s;
			IsTractored = true;
			CollisionBody.IsStatic = false;
			m_fling = false;

			DestroyCollisionBody();
		}

		public void UpdateTractor(Vector2 position) {
			m_tractorTarget = position;
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			if (CollisionBody.IsStatic) return true;
			else return !((entB is Ship) && ((Ship) entB).IsFriendly());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Asteroid : GameEntity, Tractorable, TakesDamage {
		private bool m_tractored;
		public bool IsTractored { get { return m_tractored; } set { m_tractored = value; } }

		private Vector2 m_tractorTarget;
		private bool m_fling = false;
		private float m_flingTime = 0.0f;

		private float m_colorTimer = 0.0f; // 0 for non-friendly color, 1 for friendly color.  Used for strobing effect.
		private float m_colorDir = 1.0f; // Direction of fading.
		
		private int tractoredDmg = 10;
		private float m_lastCollideTime = 0.0f;
		private int asteroidHealth = 30;

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
			m_lastCollideTime -= elapsedTime;

			if (m_fling) {
				m_flingTime -= elapsedTime;
				if (m_flingTime < 0.0f) {
					m_fling = false;
					IsTractored = false;

					// Destroy collision and re-create a static one so we don't move.
					DestroyCollisionBody();
					CreateAsteroidCollision(true);
				}

			} else if (IsTractored) {
				const float moveSpeed = 750.0f;

				Vector2 dir = (m_tractorTarget - Position);
				float distance = dir.Length();

				if (distance < moveSpeed * elapsedTime) DesiredVelocity = dir * 60.0f;
				else DesiredVelocity = Vector2.Normalize(dir) * moveSpeed;
			}

			///// Change color if we are tractored.
			const float s_colorVel = 1.0f;
			Color tintColor = new Color(1.0f, 0.8f, 0.0f); // Yellow.

			if (IsTractored) {
				m_colorTimer += m_colorDir * (s_colorVel * elapsedTime);

				// Handle changing fade directions.
				if (m_colorDir > 0.0f && m_colorTimer >= 1.0f) {
					m_colorDir *= -1.0f;
					m_colorTimer = 1.0f;
				} else if (m_colorDir < 0.0f && m_colorTimer <= 0.5f) {
					m_colorDir *= -1.0f;
					m_colorTimer = 0.5f;
				}

				VertexColor = tintColor;
			} else {
				m_colorDir = 1.0f;
				m_colorTimer -= s_colorVel * elapsedTime;
				if (m_colorTimer < 0.0f) m_colorTimer = 0.0f;
			}

			if (m_colorTimer == 0.0f) {
				VertexColor = Color.White;
			} else {
				VertexColor = Color.Lerp(Color.White, tintColor, m_colorTimer);
			}

			base.Update(elapsedTime);
		}

		public override bool ShouldCull() {
			if (IsTractored) return false;
			return base.ShouldCull();
		}

		public override void Dispose() {
			m_tractored = false;
			base.Dispose();
		}


		public void TractorReleased() {
			m_fling = true;
			m_flingTime = 1.0f;
			m_tractored = false;

			if (CollisionBody == null) return;

			CollisionBody.LinearDamping = 0.0f;
			CollisionBody.ApplyAngularImpulse(CollisionBody.Mass * RandomUtil.NextFloat(-5.0f, 5.0f));
		}

		public void Tractored(Ship s){
			tractoringShip = s;
			IsTractored = true;
			m_fling = false;

			// Destroy collision and re-create a static one so the asteroid can be moved.
			DestroyCollisionBody();
			CreateAsteroidCollision(false);

			// Give a random amount of angular impulse for cool unstable rotating!
			CollisionBody.ApplyAngularImpulse(CollisionBody.Mass * RandomUtil.NextFloat(-5.0f, 5.0f));
		}

		public void UpdateTractor(Vector2 position) {
			m_tractorTarget = position;
		}

		public override void Teleport(BlackHole blackhole, Vector2 destination, Vector2 exitVelocity) {
			if (IsTractored && !m_fling) return;
			base.Teleport(blackhole, destination, exitVelocity);
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			if (CollisionBody.IsStatic) return true;
			else return !((entB is Ship) && ((Ship) entB).IsFriendly());
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if(IsTractored && m_lastCollideTime <= 0.0f) {
				if(entB is TakesDamage && (ActualVelocity - entB.ActualVelocity).Length() > 700.0f) /*&& if contact force is strong*/ {
					((TakesDamage)entB).TakeHit(tractoredDmg);
					m_lastCollideTime = 0.5f;
					Sound.PlayCue("crash", this);
					asteroidHealth -= tractoredDmg;

					if(asteroidHealth <= 0) InstaKill();
				}
			}

			base.OnCollide(entB, contact);
		}

		public void TakeHit(int damage) {
			return;
		}

		public void InstaKill() {
			// Hit by a blackhole, ouch.
			OnNextUpdate += () => {
				Dispose();
				Environment.ExplosionEffect.Trigger(Position); // TODO: Asteroid-specific explosion?
				Sound.PlayCue("explosion", this);
			};
		}

		public bool IsDead() {
			return false;
		}

		public bool IsFriendly() {
			return false;
		}

		public bool IsAllied(TakesDamage other) {
			return false;
		}
	}
}

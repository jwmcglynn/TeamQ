using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Bullet : GameEntity
	{
		private int bulletStrength = 1;
		const float k_speed = 600.0f; // pixels per second
        public TakesDamage owner;
		private bool m_shouldCull = false;
		private float m_lifetime = 0.0f;

		public Bullet(GameEnvironment env, TakesDamage s,Vector2 position, double angle)
				: base(env)
		{
            owner = s;
			LoadTexture(env.contentManager, "bullet");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			Zindex = 0.0f;
			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Dynamic, CollisionFlags.IsBullet | CollisionFlags.DisableSleep | CollisionFlags.FixedRotation);
			AddCollisionCircle(Texture.Height/2, Vector2.Zero);
			VisualRotationOnly = true;

			CollisionBody.LinearDamping = 0.0f;

			Position = position;
			Rotation = (float) angle;
			SetPhysicsVelocityOnce(new Vector2(k_speed * (float) Math.Cos(angle), k_speed * (float) Math.Sin(angle)));
		}

		public override void Update(float elapsedTime)
		{
			m_lifetime += elapsedTime;

			Rotation = (float) Math.Atan2((double) ActualVelocity.Y, (double) ActualVelocity.X);
			base.Update(elapsedTime);
		}

		public override bool ShouldCull() {
			if (m_shouldCull || m_lifetime > 5.0f) return true;
			return base.ShouldCull();
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			if (entB is Bullet) return false; // Don't collide with other bullets.

			if (entB is TakesDamage) {
				if (((TakesDamage) owner).IsAllied((TakesDamage) entB)) return false;
			}

			return true;
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact) {
			m_shouldCull = !(entB is BlackHole);
			if (entB is TakesDamage) {
				((TakesDamage) entB).TakeHit(bulletStrength);
			}

			// Disable collision response.
			contact.Enabled = false;

			base.OnCollide(entB, contact);
		}
	}
}

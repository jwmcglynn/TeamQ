using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Bullet : Entity
	{
		private int bulletStrength = 1;
		const float k_speed = 300.0f; // pixels per second
		public bool ShotByPlayer; // to figure out who will be the target of our bullet
		
		private bool m_shouldCull = false;
		private float m_lifetime = 0.0f;

		public Bullet(GameEnvironment env, Vector2 position, double angle, bool playerShotBullet)
		{
			LoadTexture(env.contentManager, "bullet");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			ShotByPlayer = playerShotBullet;

			Zindex = 0.0f;
			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Dynamic, CollisionFlags.IsBullet | CollisionFlags.DisableSleep | CollisionFlags.FixedRotation);
			AddCollisionCircle(Texture.Height/2, Vector2.Zero);
			VisualRotationOnly = true;

			CollisionBody.LinearDamping = 0.0f;

			Position = position;
			Rotation = (float) angle;
			SetPhysicsVelocityOnce(new Vector2(k_speed * (float) Math.Cos(angle), k_speed * (float) Math.Sin(angle)));
		}

		// 1 is for weak bullet, 3 is for strong bullet.
		public bool setBulletStrength(int strength) 
		{
			if (strength == 1 || strength == 2 || strength == 3)
			{
				bulletStrength = strength;
				return true;
			}
			else
			{
				return false;
			}
		}

		public int getBulletStrength()
		{
			return bulletStrength;
		}

		public override void Update(float elapsedTime)
		{
			m_lifetime += elapsedTime;
			if (m_shouldCull || m_lifetime > 5.0f) Destroy();

			Rotation = (float) Math.Atan2((double) ActualVelocity.Y, (double) ActualVelocity.X);
			base.Update(elapsedTime);
		}

		public override bool ShouldCollide(Entity entB) {
			if (entB is Bullet) return false; // Don't collide with other bullets.
			if (entB is TakesDamage) {
				// Enemy bullets only collide with player, player bullets only collide with enemies.
				bool targetIsPlayer = ((TakesDamage) entB).IsFriendly();
				if (targetIsPlayer && ShotByPlayer) return false;
				else if (!targetIsPlayer && !ShotByPlayer) return false;
			}

			return true;
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact) {
			m_shouldCull = true;
			if (entB is TakesDamage) {
				((TakesDamage) entB).TakeHit(bulletStrength);
			}

			// Disable collision response.
			contact.Enabled = false;

			base.OnCollide(entB, contact);
		}
	}
}

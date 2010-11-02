using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class Bullet : Entity
	{
		private int bulletStrength;
		const float k_speed = 300.0f; // pixels per second
		public bool ShotByPlayer; // to figure out who will be the target of our bullet

		public Bullet(GameEnvironment env, Vector2 position, double angle, bool playerShotBullet)
		{
			LoadTexture(env.contentManager, "bullet");

			ShotByPlayer = playerShotBullet;

			Zindex = 0.0f;
			CreateCollisionBody(env.CollisionWorld, FarseerPhysics.Dynamics.BodyType.Dynamic, CollisionFlags.IsBullet | CollisionFlags.DisableSleep | CollisionFlags.FixedRotation);
			//AddCollisionRectangle(new Vector2(13.0f, 4.5f), new Vector2(18.0f, 9.5f));
			AddCollisionCircle(4.5f, Vector2.Zero);
			VisualRotationOnly = true;
			Registration = new Vector2(18.0f, 9.5f);

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
			Rotation = (float) Math.Atan2((double) ActualVelocity.Y, (double) ActualVelocity.X);
			base.Update(elapsedTime);
		}
	}
}

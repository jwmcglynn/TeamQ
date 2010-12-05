using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class BulletEmitter : Entity
	{
		private GameEnvironment env; 
		private float emitterDistance = 20.0f;
		public TakesDamage owner;

		public enum BulletStrength {
			Weak, 
			Medium, 
			Strong
		};

		private BulletStrength strength;

		private float updateAccum; 
		private float bulletInterval;
		private const float weakBulletInterval = 1.0f / 8.0f; // our threshold for when our bullet should be shot
		private const float mediumBulletInterval = 1.0f / 12.0f;
		private const float strongBulletInterval = 1.0f / 7.0f;



		private float mediumBulletSpread = 10 * (float) Math.PI / 180;

		public BulletEmitter(GameEnvironment e, TakesDamage s, BulletStrength type)
		{
			owner = s;
			env = e;
			strength = type;

			switch (strength) {
				case BulletStrength.Weak:
					bulletInterval = weakBulletInterval;
					break;
				case BulletStrength.Medium:
					bulletInterval = mediumBulletInterval;
					break;
				case BulletStrength.Strong:
					bulletInterval = strongBulletInterval;
					break;
			}
		}

		public override void Update(float elapsedTime) {
			updateAccum += elapsedTime;
			if (updateAccum > bulletInterval) updateAccum = bulletInterval;

			base.Update(elapsedTime);
		}

		public void Shoot(float elapsedTime)
		{
			if (updateAccum < bulletInterval) return;
			updateAccum -= bulletInterval;

			// determines how often the bullets will be shot
			switch (strength) {
				case BulletStrength.Weak: {
					// Spawn a bullet
					if (owner == env.sputnik.controlled) Sound.PlayCue("bullet_fire");
					else Sound.PlayCue("bullet_fire", this);
					Bullet bullet = new Bullet(env, owner, Position, (double)Rotation);
					env.AddChild(bullet);
					break;						
				} case BulletStrength.Medium: {
					// BulletAngle will be randomized across a spread of mediumBulletSpread degrees.
					double randAngle = Rotation + RandomUtil.NextFloat(-mediumBulletSpread, mediumBulletSpread);

					if (owner == env.sputnik.controlled) Sound.PlayCue("bullet_fire");
					else Sound.PlayCue("bullet_fire", this);
					Bullet bullet = new Bullet(env, owner, Position, (double) randAngle);
					env.AddChild(bullet);
					break;
				} case BulletStrength.Strong: {
					updateAccum -= strongBulletInterval;
						
					float horizontalAngle = Rotation + (float) Math.PI/2;
					Vector2 horizontalDistance = 
						new Vector2(emitterDistance * (float) Math.Cos(horizontalAngle), 
									emitterDistance * (float) Math.Sin(horizontalAngle));

					Vector2 rightBulletPos = Position + horizontalDistance;
					Vector2 leftBulletPos = Position - horizontalDistance;

					if (owner == env.sputnik.controlled) Sound.PlayCue("bullet_fire");
					else Sound.PlayCue("bullet_fire", this);
					Bullet rightBullet = new Bullet(env, owner, rightBulletPos, (double)Rotation);
					env.AddChild(rightBullet);

					Bullet leftBullet = new Bullet(env, owner, leftBulletPos, (double)Rotation);
					env.AddChild(leftBullet);
					break;
				}
			}
		}
	}
}

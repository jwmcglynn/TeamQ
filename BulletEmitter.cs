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
		public bool ShotByPlayer;
		private float emitterDistance = 20.0f;
		Random rand = new Random();

		public enum BulletStrength {
			Weak, 
			Medium, 
			Strong
		};

		private BulletStrength strength;

		private float updateAccum; 
		private float weakBulletInterval = 1.0f / 8.0f; // our threshold for when our bullet should be shot
		private float mediumBulletInterval = 1.0f / 12.0f;
		private float strongBulletInterval = 1.0f / 7.0f;

		private float mediumBulletSpread = 10 * (float) Math.PI / 180;

		public BulletEmitter(GameEnvironment e, BulletStrength type, bool playerShotBullet)
		{	
			env = e;
			strength = type;
			ShotByPlayer = playerShotBullet;
		}

		public void Shoot(float elapsedTime)
		{
			// time that has passed since last bullet and if its passed a certain threshold we will shoot a bullet
			// after a bullet is shot, we must subtract from our cooldowntime the threshold, so we can continue using
			// time spent so far. 
			updateAccum += elapsedTime;

			// determines how often the bullets will be shot
			switch (strength) {
				case BulletStrength.Weak: {
					if (updateAccum > weakBulletInterval)
					{
						updateAccum -= weakBulletInterval;
						
						// Spawn a bullet
						Bullet bullet = new Bullet(env, Position, (double)Rotation, ShotByPlayer);
						env.AddChild(bullet);
					}
					break;						
				} case BulletStrength.Medium: {
					if (updateAccum > mediumBulletInterval)
					{
						updateAccum -= mediumBulletInterval;

						// BulletAngle will be randomized across a spread of mediumBulletSpread degrees.
						double randAngle = Rotation + (rand.NextDouble() - 0.5f) * mediumBulletSpread;

						Bullet bullet = new Bullet(env, Position, (double)randAngle, ShotByPlayer);
						env.AddChild(bullet);
					}
					break;
				} case BulletStrength.Strong: {
					if (updateAccum > strongBulletInterval)
					{
						updateAccum -= strongBulletInterval;
						
						float horizontalAngle = Rotation + (float) Math.PI/2;
						Vector2 horizontalDistance = 
							new Vector2(emitterDistance * (float) Math.Cos(horizontalAngle), 
										emitterDistance * (float) Math.Sin(horizontalAngle));

						Vector2 rightBulletPos = Position + horizontalDistance;
						Vector2 leftBulletPos = Position - horizontalDistance;

						Bullet rightBullet = new Bullet(env, rightBulletPos, (double)Rotation, ShotByPlayer);
						env.AddChild(rightBullet);

						Bullet leftBullet = new Bullet(env, leftBulletPos, (double)Rotation, ShotByPlayer);
						env.AddChild(leftBullet);
					}
					break;
				}
			}
		}
	}
}

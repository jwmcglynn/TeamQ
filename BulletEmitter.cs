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
		public bool IsShooting;
		public bool ShotByPlayer;
		private float emitterDistance = 20.0f;

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

		public BulletEmitter(GameEnvironment e, BulletStrength type, bool playerShotBullet) // type
		{	
			env = e;
			strength = type;
			ShotByPlayer = playerShotBullet;
		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);

			if(!IsShooting) 
			{
				return;
			}

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
						Bullet bullet = new Bullet(env, Position, (double)Rotation, IsShooting);
						AddChild(bullet);
					}
					break;						
				} case BulletStrength.Medium: {
					if (updateAccum > mediumBulletInterval)
					{
						updateAccum -= mediumBulletInterval;

						// spawn 1.5 times more often
						// the bullets will be spread within a cone infront. 
						// approximately 5 degrees (has to be in radians)
						// "5 Degrees of spread"
						float horizontalAngle = Rotation + (float)Math.PI / 2; // Rotation is in radians
						Vector2 horizontalDistance =
							new Vector2(emitterDistance * (float)Math.Cos(horizontalAngle),
										emitterDistance * (float)Math.Sin(horizontalAngle));

						// BulletAngle will be randomized across a spread of mediumBulletSpread degrees.
						Random rand = new Random();
						float randAngle = (float) rand.NextDouble();

						//Console.WriteLine(randAngle); 
						if(randAngle > 0.5) {
							randAngle = Rotation + randAngle * mediumBulletSpread;
						} else {
							randAngle = Rotation - randAngle * mediumBulletSpread;
						}
						//Console.WriteLine(randAngle);
						//randAngle = Rotation;

						Vector2 bulletPos = Position + horizontalDistance;

						Bullet bullet = new Bullet(env, bulletPos, (double) randAngle, IsShooting);
						AddChild(bullet);
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

						Bullet rightBullet = new Bullet(env, rightBulletPos, (double)Rotation, IsShooting);
						AddChild(rightBullet);

						Bullet leftBullet = new Bullet(env, leftBulletPos, (double)Rotation, IsShooting);
						AddChild(leftBullet);
					}
					break;
				}
			}
		}
	}
}

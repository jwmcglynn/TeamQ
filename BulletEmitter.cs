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

		public enum BulletStrength {
			Weak, 
			Medium, 
			Strong
		};

		private BulletStrength strength;
		private float updateAccum; 
		private float bulletInterval = 1.0f / 8.0f; // our threshold for when our bullet should be shot
		 
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
					if (updateAccum > bulletInterval)
					{
						updateAccum -= bulletInterval;
						
						// Spawn a bullet
						Bullet bullet = new Bullet(env, Position, (double)Rotation, IsShooting);
						AddChild(bullet);
					}
					break;						
				} case BulletStrength.Medium: {
				// spawn 1.5 times more often
				// the bullets will be spread within a cone infront. 
				// approximately 5 degrees (has to be in radians)
				// "5 Degrees of spread"
					break;
				} case BulletStrength.Strong: {
					break;
				}
			}
		}
	}
}

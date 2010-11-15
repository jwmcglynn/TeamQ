using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
	class Boss : Entity, TakesDamage
	{
		private int health = 100;

		public Boss(GameEnvironment env)
		{

		}

		public Boss(GameEnvironment env, SpawnPoint sp)
		{

		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}

		public override bool ShouldCollide(Entity entB)
		{
			return base.ShouldCollide(entB);
		}

		public bool IsFriendly()
		{
			return false;
		}

		public void InstaKill()
		{

		}

		public void TakeHit(int damage)
		{
			this.health -= damage;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
	class Boss : Entity, TakesDamage
	{
		private int health = 100;
		protected BulletEmitter bm1, bm2, bm3;
		protected BossAI ai;
		private bool m_shouldCull = false;

		public Boss(GameEnvironment env) //: base(env)
		{

		}

		public Boss(GameEnvironment env, SpawnPoint sp) //: base(env, sp)
		{
			
		}

		public override void Update(float elapsedTime)
		{
			ai.Update(elapsedTime);

			base.Update(elapsedTime);

			bm1.Position = this.Position;
			bm2.Position = this.Position;
			bm3.Position = this.Position;
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
			m_shouldCull = true;
		}

		/*public override bool ShouldCull()
		{
			if (m_shouldCull) return true;
			return base.ShouldCull();
		}*/

		public void TakeHit(int damage)
		{
			this.health -= damage;
			if (this.health < 1)
				InstaKill();
		}
	}
}

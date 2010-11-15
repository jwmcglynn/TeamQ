using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class BossAI
	{
		private Vector2[] points;
		private int index;
		private GameEnvironment env;
		private Boss boss;
		private SaphereBoss saphereBoss;
		private SputnikShip player_2;
		private Vector2[] temp;

		public BossAI(GameEnvironment env, Boss boss, Vector2[] points)
		{
			this.index = 1;
			this.points = points;
			this.env = env;
			this.boss = boss;
			boss.Position = points[0];
		}

		public void Update(float elapsedTime)
		{
			if (this.index >= points.Length)
				index = 0;
			
			// add in boss movement.
		}
	}
}

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

			float wantedDirection = (float)Math.Atan2(points[index].Y - boss.Position.Y, points[index].X - boss.Position.X);
			while (wantedDirection < 0)
				wantedDirection += MathHelper.Pi * 2.0f;

			boss.DesiredVelocity = new Vector2((float)Math.Cos(wantedDirection) * boss.maxSpeed, (float)Math.Sin(wantedDirection) * boss.maxSpeed);

			Vector2 temp = boss.Position - points[index];
			if (Math.Abs(temp.X) < 5 && Math.Abs(temp.Y) < 5)
				index++;
			boss.Shoot(elapsedTime);

		}
	}
}

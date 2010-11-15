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
			while (boss.Rotation < 0)
				boss.Rotation += MathHelper.Pi * 2.0f;
			boss.Rotation %= MathHelper.Pi * 2.0f;
			wantedDirection %= MathHelper.Pi * 2.0f;

			if (Vector2.Distance(boss.Position, points[index]) < boss.maxSpeed * elapsedTime) //This number needs tweaking, 0 does not work
			{
				boss.DesiredVelocity = Vector2.Zero;
			}
			else if (Math.Abs(wantedDirection - boss.Rotation) < boss.maxTurn)
			{
				boss.DesiredVelocity = new Vector2((float)Math.Cos(boss.Rotation) * boss.maxSpeed, (float)Math.Sin(boss.Rotation) * boss.maxSpeed);
			}
			else
			{
				boss.DesiredVelocity = Vector2.Zero;
				float counterclockwiseDistance = Math.Abs(wantedDirection - (boss.Rotation + boss.maxTurn) % (MathHelper.Pi * 2));
				float clockwiseDistance = Math.Abs(wantedDirection - (boss.Rotation - boss.maxTurn + MathHelper.Pi * 2) % (MathHelper.Pi * 2));
				if (counterclockwiseDistance < clockwiseDistance)
				{
					if (counterclockwiseDistance < boss.maxTurn)
					{
						boss.Rotation = wantedDirection;
					}
					else
					{
						boss.Rotation += boss.maxTurn;
					}
				}
				else
				{
					if (clockwiseDistance < boss.maxTurn)
					{
						boss.Rotation = wantedDirection;
					}
					else
					{
						boss.Rotation -= boss.maxTurn;
					}
				}
			} 
			if(boss.Position.X == points[index].X && boss.Position.Y == points[index].Y)
				index++;
			boss.Shoot(elapsedTime);

		}
	}
}

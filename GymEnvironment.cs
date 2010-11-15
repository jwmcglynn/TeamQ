﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik
{
	class GymEnvironment : GameEnvironment
	{
		Random r = new Random();

		private Vector2 randomPosition() {
			return new Vector2((float) r.NextDouble(), (float) r.NextDouble()) * ScreenVirtualSize;
		}

		public GymEnvironment(Controller ctrl)
			: base(ctrl)
		{
			for (int i = 0; i < 20; i++)
			{
				Ship s = new CircloidShip(
					this, randomPosition(), randomPosition(), randomPosition()
				);

				AddChild(s);
			}

			Entity e = new SaphereBoss(this);
			AddChild(e);

			AddChild(new Crosshair(this));
			LoadMap("gym.tmx");

			Sound.PlayCue("music");
		}
	}
}
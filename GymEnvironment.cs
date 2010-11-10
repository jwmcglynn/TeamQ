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

		//BulletEmitter emit;
		Random r = new Random();
		BlackHole blackHole;

		private Vector2 randomPosition() {
			return new Vector2((float) r.NextDouble() * Controller.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * Controller.Graphics.GraphicsDevice.Viewport.Height);
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
			//AddChild(new TestShip(150, 150, 0, 0, this));
			AddChild(new Crosshair(this));

			//blackHole = new BlackHole(this, false);
			//AddChild(blackHole);

			LoadMap("gym.tmx");
		}

		public override void Update(float elapsedTime)
		{
			KeyboardState kb = Keyboard.GetState();
			//emit.IsShooting = kb.IsKeyDown(Keys.Space);

			base.Update(elapsedTime);
		}
	}
}
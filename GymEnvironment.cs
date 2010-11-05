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

		public GymEnvironment(Controller ctrl)
			: base(ctrl)
		{
		/*
			SpecialAbility special = new SpecialAbility();
			special.LoadTexture(contentManager, "bullet");
			special.Position = new Vector2(300.0f, 100.0f);
			special.DesiredVelocity = new Vector2(0.0f, 0.0f);
			AddChild(special);
		 */

		//	emit = new BulletEmitter(this, BulletEmitter.BulletStrength.Weak, true);
		//	emit = new BulletEmitter(this, BulletEmitter.BulletStrength.Medium, true);
		//	emit = new BulletEmitter(this, BulletEmitter.BulletStrength.Strong, true);

		//	emit.Position = new Vector2(210.0f, 300.0f);
		//	emit.Rotation = (float)3.14 * 3 / 2;
		//	AddChild(emit);

			for (int i = 0; i < 20; i++)
			{
				/*AddChild(new TestShip((float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
					0, 0,
					(float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
					(float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
					this));*/

				Ship s = new CircloidShip((float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
					0, 0,
					(float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
					(float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Width, (float)r.NextDouble() * ctrl.Graphics.GraphicsDevice.Viewport.Height,
					this);
                AddChild(s);
			}
            Entity s2 = new SputnikShip(150, 150, 0, 0, this);
            AddChild(s2);
			//AddChild(new TestShip(150, 150, 0, 0, this));
			AddChild(new Crosshair(this));

			blackHole = new BlackHole(this);
			AddChild(blackHole);

			LoadMap("gym.tmx");
		}

		public override void Update(float elapsedTime)
		{
			KeyboardState kb = Keyboard.GetState();
			//emit.IsShooting = kb.IsKeyDown(Keys.Space);

			if(kb.IsKeyDown(Keys.B)) blackHole.Update(elapsedTime);

			const float k_cameraVel = 150.0f;
			if (kb.IsKeyDown(Keys.Up)) m_viewportPosition.Y -= k_cameraVel * elapsedTime;
			if (kb.IsKeyDown(Keys.Left)) m_viewportPosition.X -= k_cameraVel * elapsedTime;
			if (kb.IsKeyDown(Keys.Down)) m_viewportPosition.Y += k_cameraVel * elapsedTime;
			if (kb.IsKeyDown(Keys.Right)) m_viewportPosition.X += k_cameraVel * elapsedTime;

			base.Update(elapsedTime);
		}
	}
}
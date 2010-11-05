﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SquaretopiaShip : Ship
	{
		public SquaretopiaShip(float x, float y, float vx, float vy, float sx, float sy, float fx, float fy, GameEnvironment env)
			: base(x, y, vx, vy, sx, sy, fx, fy, env)
		{
			this.shooter = new BulletEmitter(env, BulletEmitter.BulletStrength.Strong, IsFriendly());
			env.AddChild(this.shooter);
			this.ai = new AIController(new Vector2(sx, sy), new Vector2(fx, fy), env);
			this.LoadTexture(env.contentManager, "squaretopia");

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
		}
	}
}
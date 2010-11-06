using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class TriangulusShip : Ship
	{
		public TriangulusShip(float x, float y, float vx, float vy, float sx, float sy, float fx, float fy, GameEnvironment env)
			: base(x, y, vx, vy, sx, sy, fx, fy, env)
		{
			this.shooter = new BulletEmitter(env, BulletEmitter.BulletStrength.Weak, IsFriendly());
			env.AddChild(this.shooter);
			ai = new AIController(new Vector2(sx, sy), new Vector2(fx, fy), env);
			this.LoadTexture(env.contentManager, "triangulus");

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
		}

		public TriangulusShip(GameEnvironment env, SpawnPoint sp)
				: this(sp.Position.X, sp.Position.Y, 0.0f, 0.0f, sp.TopLeft.X, sp.TopLeft.Y, sp.BottomRight.X, sp.BottomRight.Y, env) {
		}
	}
}

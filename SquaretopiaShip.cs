using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SquaretopiaShip : Ship
	{
		public SquaretopiaShip(GameEnvironment env, Vector2 pos, Vector2 patrolStart, Vector2 patrolEnd)
			: base(env, pos)
		{
			Initialize(patrolStart, patrolEnd);
		}

		private void Initialize(Vector2 patrolStart, Vector2 patrolEnd) {
			shooter = new BulletEmitter(Environment, BulletEmitter.BulletStrength.Strong, IsFriendly());
			AddChild(shooter);
			ai = new AIController(patrolStart, patrolEnd, Environment);
			LoadTexture(Environment.contentManager, "squaretopia");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
		}

		public SquaretopiaShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(Position, Position + new Vector2(50.0f, 50.0f)); // FIXME: Find a better way to get positions.
		}
	}
}

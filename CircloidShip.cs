using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class CircloidShip : Ship, Tractorable
	{
		public CircloidShip(GameEnvironment env, Vector2 pos, Vector2 patrolStart, Vector2 patrolEnd) 
			: base(env, pos)
		{
			Initialize(patrolStart, patrolEnd);
		}

		private void Initialize(Vector2 patrolStart, Vector2 patrolEnd) {
			shooter = new BulletEmitter(Environment, this, BulletEmitter.BulletStrength.Medium, IsFriendly());
			AddChild(shooter);
			ai = new AIController(patrolStart, patrolEnd, Environment);
			LoadTexture(Environment.contentManager, "circloid");

			m_patrolRect = new Rectangle((int) patrolStart.X, (int) patrolEnd.Y, (int) (patrolEnd.X - patrolStart.X), (int) (patrolEnd.Y - patrolStart.Y));

			Registration = new Vector2(117.0f, 101.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(60.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true; // The circloid will not be affected by its own black hole. 
		}

		public CircloidShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp.TopLeft, sp.BottomRight); // FIXME: Find a better way to get positions.
		}
	}
}

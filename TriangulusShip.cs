using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Physics = FarseerPhysics;
using FarseerPhysics.Common;

namespace Sputnik
{
	class TriangulusShip : Ship, Tractorable, Freezable
	{
		bool isFrozen;
		private float tractorBeamSpread = 20 * (float)Math.PI / 180;

		public TriangulusShip(GameEnvironment env, Vector2 pos, Vector2 patrolStart, Vector2 patrolEnd)
			: base(env, pos)
		{
			Initialize(patrolStart, patrolEnd);
		}

		private void Initialize(Vector2 patrolStart, Vector2 patrolEnd) {
			shooter = new BulletEmitter(Environment, this,BulletEmitter.BulletStrength.Weak, IsFriendly());
			AddChild(shooter);
			ai = new AIController(patrolStart, patrolEnd, Environment);
			LoadTexture(Environment.contentManager, "triangulus");

			m_patrolRect = new Rectangle((int) patrolStart.X, (int) patrolEnd.Y, (int) (patrolEnd.X - patrolStart.X), (int) (patrolEnd.Y - patrolStart.Y));

			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
		}

		public TriangulusShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp.TopLeft, sp.BottomRight); // FIXME: Find a better way to get positions.
		}

		public override void OnCollide(Entity entB, Physics.Dynamics.Contacts.Contact contact)
		{
			if(contact.FixtureA.IsSensor && (entB is Tractorable)) {
				
			}
			base.OnCollide(entB, contact);
		}

		public override bool ShouldCollide(Entity entB, Fixture fixture, Fixture entBFixture)
		{
			if (fixture.IsSensor && !(entB is Tractorable)) return false;
			return base.ShouldCollide(entB, fixture, entBFixture);
		}

		public void Freeze()
		{
			isFrozen = true;
		}

		public void Unfreeze()
		{
			isFrozen = false;
		}

		public bool IsFrozen()
		{
			return isFrozen;
		}
	}
}

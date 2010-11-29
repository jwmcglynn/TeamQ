using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;

namespace Sputnik
{
	class TriangulusShip : Ship, Tractorable, Freezable
	{
		private float tractorBeamSpread = 20 * (float)Math.PI / 180;

		public TriangulusShip(GameEnvironment env, Vector2 pos, SpawnPoint sp)
			: base(env, pos)
		{
			Initialize(sp);
			env.triangles.Add(this);
		}

		private void Initialize(SpawnPoint sp) {
			shooter = new BulletEmitter(Environment, this,BulletEmitter.BulletStrength.Weak, IsFriendly());
			AddChild(shooter);
			ai = new AIController(sp, Environment);
			LoadTexture(Environment.contentManager, "triangulus");

			m_patrolRect = new Rectangle((int) sp.TopLeft.X, (int) sp.BottomRight.Y, (int) (sp.BottomRight.X - sp.TopLeft.X), (int) (sp.BottomRight.Y - sp.TopLeft.Y));

			Registration = new Vector2(100.0f, 100.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(40.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			/*
			List<Vector2> vertices = new List<Vector2>();
			vertices.Add(new Vector2(0, 0));
			vertices.Add(new Vector2(20, -(float)(Math.Tan(MathHelper.ToRadians(20)) * 20)));
			vertices.Add(new Vector2(20, (float)(Math.Tan(MathHelper.ToRadians(20)) * 20)));
			Fixture sensor = CollisionBody.CreateFixture(new PolygonShape(new Vertices(vertices)), 0);
			sensor.IsSensor = true;
			*/ 
		}

		public TriangulusShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp); // FIXME: Find a better way to get positions.
			env.triangles.Add(this);
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

		public void Tractored(Ship shipTractoring)
		{
			tractoringShip = shipTractoring;
			isTractored = true;
		}

		public void TractorReleased() {
			isTractored = false;
		}

		public override void OnCull()
		{
			Environment.triangles.Remove(this);
			base.OnCull();
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (contact.FixtureA.IsSensor && (entB is Tractorable))
			{

			}
			else if (entB is Bullet)
			{
				//Horrible Casting makes me sad.
				foreach (TriangulusShip t in Environment.triangles)
					t.ai.GotShotBy(t, (GameEntity)((Bullet)entB).owner);
			}
			base.OnCollide(entB, contact);
		}
	}
}

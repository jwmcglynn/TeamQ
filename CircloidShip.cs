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
	class CircloidShip : Ship, Tractorable
	{
		public CircloidShip(GameEnvironment env, Vector2 pos, SpawnPoint sp) 
			: base(env, pos)
		{
			Initialize(sp);
			env.circles.Add(this);
		}

		private void Initialize(SpawnPoint sp) {
			shooter = new BulletEmitter(Environment, this, BulletEmitter.BulletStrength.Medium);
			AddChild(shooter);
			ai = new AIController(sp, Environment);
			LoadTexture(Environment.contentManager, "circloid");

			m_patrolRect = new Rectangle((int) sp.TopLeft.X, (int) sp.BottomRight.Y, (int) (sp.BottomRight.X - sp.TopLeft.X), (int) (sp.BottomRight.Y - sp.TopLeft.Y));

			Registration = new Vector2(117.0f, 101.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(60.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
			CollisionBody.IgnoreGravity = true; // The circloid will not be affected by its own black hole. 
			/*
			List<Vector2> vertices = new List<Vector2>();
			vertices.Add(new Vector2(0, 0));
			vertices.Add(new Vector2(20, -(float)(Math.Tan(MathHelper.ToRadians(20)) * 20)));
			vertices.Add(new Vector2(20, (float)(Math.Tan(MathHelper.ToRadians(20)) * 20)));
			Fixture sensor = CollisionBody.CreateFixture(new PolygonShape(new Vertices(vertices)), 0);
			sensor.IsSensor = true;
			*/ 
		}

		public CircloidShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp); // FIXME: Find a better way to get positions.
			env.circles.Add(this);
		}

		public void Tractored(Ship shipTractoring)
		{
			tractoringShip = shipTractoring;
			isTractored = true;
			ai.GotTractored();
		}

		public void TractorReleased()
		{
			isTractored = false;
		}

		public override void OnCull()
		{
			Environment.circles.Remove(this);
			base.OnCull();
		}

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is Bullet && attachedShip == null)
			{
				//Horrible Casting makes me sad.
				foreach (CircloidShip c in Environment.circles)
					c.ai.GotShotBy(c, (GameEntity)((Bullet)entB).owner);
			}
			base.OnCollide(entB, contact);
		}
	}
}

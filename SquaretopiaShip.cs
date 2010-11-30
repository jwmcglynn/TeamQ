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
	class SquaretopiaShip : Ship, Freezable
	{

		public SquaretopiaShip(GameEnvironment env, Vector2 pos, SpawnPoint sp)
			: base(env, pos)
		{
			Initialize(sp);
			env.squares.Add(this);
		}

		private void Initialize(SpawnPoint sp) {
			shooter = new BulletEmitter(Environment, this,BulletEmitter.BulletStrength.Strong, IsFriendly());
			AddChild(shooter);
			ai = new AIController(sp, Environment);
			LoadTexture(Environment.contentManager, "squaretopia");

			m_patrolRect = new Rectangle((int) sp.TopLeft.X, (int) sp.BottomRight.Y, (int) (sp.BottomRight.X - sp.TopLeft.X), (int) (sp.BottomRight.Y - sp.TopLeft.Y));

			Registration = new Vector2(100.0f, 125.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(50.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;

			passiveShield = 20.0f;
			/*
			List<Vector2> vertices = new List<Vector2>();
			vertices.Add(new Vector2(0, 0));
			vertices.Add(new Vector2(20, -(float)(Math.Tan(MathHelper.ToRadians(20)) * 20)));
			vertices.Add(new Vector2(20, (float)(Math.Tan(MathHelper.ToRadians(20)) * 20)));
			Fixture sensor = CollisionBody.CreateFixture(new PolygonShape(new Vertices(vertices)), 0);
			sensor.IsSensor = true;
			*/ 
		}

		public SquaretopiaShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp); // FIXME: Find a better way to get positions.
			env.squares.Add(this);
		}

		public void Freeze() {
			isFrozen = true;
			ai.GotFrozen();
		}

		public void Unfreeze()
		{
			isFrozen = false;
		}

		public override void OnCull()
		{
			Environment.squares.Remove(this);
			base.OnCull();
		}
		

		public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB is Bullet)
			{
				//Horrible Casting makes me sad.
				foreach (SquaretopiaShip s in Environment.squares)
					s.ai.GotShotBy(s, (GameEntity)((Bullet)entB).owner);
			}
			base.OnCollide(entB, contact);
		}
	}
}

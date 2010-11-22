using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SquaretopiaShip : Ship, Freezable
	{
		bool isFrozen;

		public SquaretopiaShip(GameEnvironment env, Vector2 pos, Vector2 patrolStart, Vector2 patrolEnd)
			: base(env, pos)
		{
			Initialize(patrolStart, patrolEnd);
			env.squares.Add(this);
		}

		private void Initialize(Vector2 patrolStart, Vector2 patrolEnd) {
			shooter = new BulletEmitter(Environment, this,BulletEmitter.BulletStrength.Strong, IsFriendly());
			AddChild(shooter);
			ai = new AIController(patrolStart, patrolEnd, Environment);
			LoadTexture(Environment.contentManager, "squaretopia");

			m_patrolRect = new Rectangle((int) patrolStart.X, (int) patrolEnd.Y, (int) (patrolEnd.X - patrolStart.X), (int) (patrolEnd.Y - patrolStart.Y));

			Registration = new Vector2(100.0f, 125.0f);
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(50.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f;
		}

		public SquaretopiaShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			Initialize(sp.TopLeft, sp.BottomRight); // FIXME: Find a better way to get positions.
			env.squares.Add(this);
		}

		public void Freeze() {
		
		}

		public void Unfreeze() {
		
		}

		public bool IsFrozen() {
			return isFrozen;
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

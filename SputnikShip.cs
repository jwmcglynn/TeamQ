using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Sputnik
{
	class SputnikShip : Ship
	{
		public bool attached = false;
		public bool attaching = false;
		private Ship controlled = null; 
		private Ship recentlyControlled = null;
		private ShipController playerAI = null;

		public SputnikShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
					
			env.sputnik = this;
			Zindex = 0.0f;
			Position = sp.Position;
			
			this.maxSpeed = 500;

			LoadTexture(env.contentManager, "Sputnik");
			Registration = new Vector2(70.0f, 33.0f);

			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(20.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.

			ai = playerAI = new PlayerController(env);

			// Adjust camera.
			env.Camera.TeleportAndFocus(this);
		}

		public override void Update(float elapsedTime)
		{
			if ((controlled == null || controlled.health == 0) && attached)
			{
				Detach();
			}
			
			if (attached && controlled != null)
			{
				if(Vector2.Distance(this.Position, this.controlled.Position) < (this.maxSpeed * 3) * elapsedTime || !attaching)
				{
					attaching = false;
					this.Rotation = this.controlled.Rotation;
					this.Position = this.controlled.Position;
				} else {
					this.DesiredRotation = Angle.Direction(this.Position, this.controlled.Position);
					this.DesiredVelocity = Angle.Vector(this.DesiredRotation) * this.maxSpeed * 3;
				}
			}
			base.Update(elapsedTime);
		}

		public override void Dispose() {
			Environment.Camera.Focus = null;
			base.Dispose();
		}

		public override bool ShouldCull() {
			return false; // No, not Sputnik!  Don't cull him!
		}

		public override void Shoot(float elapsedTime)
		{
			// Do Nothing
		}

		public ShipController GetAI()
		{
			return this.ai;
		}

		public override void Detach()
		{
			recentlyControlled = controlled;
			attached = false;
			controlled = null;
			ai = playerAI;
		}

		public override bool IsFriendly() {
			return true;
		}

		public override void OnSeparate(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
		{
			if (entB == recentlyControlled && !attached)
				recentlyControlled = null;
			base.OnSeparate(entB, contact);
		}

		public override bool ShouldCollide(Entity entB, FarseerPhysics.Dynamics.Fixture fixture, FarseerPhysics.Dynamics.Fixture entBFixture) {
			return !(entB is Environment) && !(entB is Bullet);
		}

		public override void TakeHit(int damage) {
			// Do nothing.
		}

        public override void OnCollide(Entity entB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
			contact.Enabled = false;
			if (entB is Ship && !attached && entB != recentlyControlled)
			{
				this.attached = true;
				this.attaching = true;
				((Ship)entB).Attach(this);
				this.ai = null;
				controlled = (Ship)entB;
			}
			base.OnCollide(entB, contact);
        }

		public override void OnCull()
		{
			Environment.sputnik = null;
			base.OnCull();
		}

	}
}

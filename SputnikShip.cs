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

		public SputnikShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
			Position = sp.Position;
			
			this.maxSpeed = 200;
			
			shooter = new BulletEmitter(env, this,BulletEmitter.BulletStrength.Weak, IsFriendly());
			AddChild(shooter);

			LoadTexture(env.contentManager, "Sputnik_Old");
			Registration = new Vector2(Texture.Width, Texture.Height) * 0.5f;

			CreateCollisionBody(env.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(Texture.Width * 0.5f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.

			ai = new PlayerController(env);

			// Adjust camera.
			env.Camera.TeleportAndFocus(this);
		}

		public override void Update(float elapsedTime)
		{
			ai.Update(this, elapsedTime);
			
			if ((controlled == null || controlled.health == 0) && attached)
			{
				Detatch();
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

		public override void Detatch()
		{
			recentlyControlled = controlled;
			attached = false;
			controlled = null;
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
				controlled = (Ship)entB;
			}
			base.OnCollide(entB, contact);
        }
	}
}

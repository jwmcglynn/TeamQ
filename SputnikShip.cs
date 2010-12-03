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
		public Ship controlled = null; 
		private Ship recentlyControlled = null;
		private ShipController playerAI = null;

		public SputnikShip(GameEnvironment env, SpawnPoint sp)
				: base(env, sp) {
					
			env.sputnik = this;
			Zindex = 0.25f;
			Position = sp.Position;
			
			this.maxSpeed = 500;

			LoadTexture(env.contentManager, "Sputnik");
			Registration = new Vector2(70.0f, 33.0f);

			SputnikCreateCollision();
			ai = playerAI = new PlayerController(env);

			// Adjust camera.
			env.Camera.TeleportAndFocus(this);
		}

		private void SputnikCreateCollision() {
			CreateCollisionBody(Environment.CollisionWorld, BodyType.Dynamic, CollisionFlags.Default);
			AddCollisionCircle(20.0f, Vector2.Zero);
			CollisionBody.LinearDamping = 8.0f; // This value causes a small amount of slowing before stop which looks nice.
		}

		public override void Update(float elapsedTime)
		{
			if ((controlled == null || controlled.health == 0) && attached)
			{
				Detach();
			}

			// Thruster particle.
			if (!attached && DesiredVelocity.LengthSquared() > (maxSpeed / 4) * (maxSpeed / 4)) {
				Environment.ThrusterEffect.Trigger(Position + Angle.Vector(Rotation + MathHelper.Pi) * 20.0f);
			}

			if (attached) {
				if(!attaching) {
					Rotation = controlled.Rotation;
					Position = controlled.Position;
				} else {
					if (Vector2.Distance(Position, controlled.Position) < 10.0f && Angle.DistanceMag(Rotation, controlled.Rotation) < 0.5f) {
						attaching = false;
						DesiredVelocity = Vector2.Zero;

						Environment.AttachEffect.Trigger(Position);
					} else {
						DesiredVelocity = Vector2.Normalize(controlled.Position - Position) * maxSpeed;
						DesiredRotation = controlled.Rotation;
					}
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

		public void SputnikAttach(Ship target) {
			DestroyCollisionBody();

			attached = true;
			attaching = true;
			controlled = target;
			this.ai = null;
		}

		public override void Detach()
		{
			if (!attached) return;

			recentlyControlled = controlled;
			attached = false;
			controlled = null;
			ai = playerAI;

			SputnikCreateCollision();
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
				OnNextUpdate += () => ((Ship) entB).Attach(this);
			}
			base.OnCollide(entB, contact);
		}
	}
}
